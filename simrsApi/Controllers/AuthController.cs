using cpcApi.Data;
using cpcApi.Model;
using cpcApi.Model.DTO;
using cpcApi.Model.DTO.konfigurasi;
using cpcApi.Model.Konfigurasi;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace cpcApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration,
            ApplicationDbContext context)
        {
            this.userManager = userManager;
            _configuration = configuration;
            _context = context;
        }

        // =====================================================
        // LOGIN (Single Session + Audit IP/Device)
        // =====================================================
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] FormLoginDto model)
        {
            var ip = GetIpAddress();
            var device = GetDeviceInfo();

            var user = await userManager.FindByNameAsync(model.username);

            if (user == null)
            {
                await SaveAuditLogin(null, model.username, ip, device, false);
                //return Unauthorized(new { message = "Username tidak ditemukan" });
                return Ok(new
                {
                    metadata = new { code = "201", message = "Username tidak ditemukan" },
                    response = ""

                });
            }

            if (!user.Active)
            {
                await SaveAuditLogin(user.Id, user.UserName, ip, device, false);
                //return Unauthorized(new { message = "User tidak aktif" });
                return Ok(new
                {
                    metadata = new { code = "201", message = "User tidak aktif" },
                    response = ""

                });
            }

            if (!await userManager.CheckPasswordAsync(user, model.password))
            {
                await SaveAuditLogin(user.Id, user.UserName, ip, device, false);
                //return Unauthorized(new { message = "User password tidak cocok" });
                return Ok(new
                {
                    metadata = new { code = "201", message = "User password tidak sesuai" },
                    response = ""

                });
            }

            // 🔥 Single session: naikkan session version (biar device lama langsung 401)
            user.SessionVersion += 1;
            await userManager.UpdateAsync(user);

            // 🔥 Revoke semua refresh token lama
            var oldTokens = _context.RefreshToken
                .Where(x => x.UserId == user.Id && !x.IsRevoked);

            foreach (var oldToken in oldTokens)
                oldToken.IsRevoked = true;

            // ✅ Audit login sukses
            var auditId = await SaveAuditLogin(user.Id, user.UserName, ip, device, true);

            await _context.SaveChangesAsync();

            // Build access token (WAJIB include SessionVersion)
            var accessToken = BuildAccessToken(user.UserName, user.SessionVersion);

            // Create refresh token baru (kalau mau, simpan auditId juga di refresh token)
            var refreshToken = GenerateRefreshToken(user.Id);

            _context.RefreshToken.Add(refreshToken);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                metadata = new { code = "200", message = "ok" },
                response = new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(accessToken),
                    refreshToken = refreshToken.Token,
                    expiresIn = 600,
                    auditLoginId = auditId // optional buat tracking
                }
            });
        }

        // =====================================================
        // REFRESH (WAJIB include SessionVersion)
        // =====================================================
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromHeader(Name = "X-Refresh-Token")] string refreshToken)
        {
            var storedToken = await _context.RefreshToken
                .FirstOrDefaultAsync(x => x.Token == refreshToken);

            if (storedToken == null ||
                storedToken.ExpiredAt < DateTime.UtcNow ||
                storedToken.IsUsed ||
                storedToken.IsRevoked)
            {
                return Unauthorized(new { message = "Invalid refresh token" });
            }

            storedToken.IsUsed = true;

            var user = await userManager.FindByIdAsync(storedToken.UserId);
            if (user == null || !user.Active)
                return Unauthorized(new { message = "User tidak valid / tidak aktif" });

            // ✅ Access token baru harus bawa SessionVersion terbaru dari DB
            var newAccessToken = BuildAccessToken(user.UserName, user.SessionVersion);

            // Refresh token baru
            var newRefreshToken = GenerateRefreshToken(user.Id);
            _context.RefreshToken.Add(newRefreshToken);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                metadata = new { code = "200", message = "ok" },
                response = new
                {
                    accessToken = new JwtSecurityTokenHandler().WriteToken(newAccessToken),
                    refreshToken = newRefreshToken.Token,
                    expiresIn = 600
                }
            });
        }

        // =====================================================
        // LOGOUT (revoke token + set logout time audit terakhir)
        // =====================================================
        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrWhiteSpace(username))
                return Unauthorized();

            var user = await userManager.FindByNameAsync(username);
            if (user == null) return Unauthorized();

            // revoke refresh token user
            var tokens = _context.RefreshToken
                .Where(x => x.UserId == user.Id && !x.IsRevoked);

            foreach (var t in tokens)
                t.IsRevoked = true;

            // set logout time pada audit login terakhir (yang belum logout)
            var lastAudit = await _context.AuditLogin
                .Where(x => x.UserId == user.Id && x.IsSuccess && x.LogoutTime == null)
                .OrderByDescending(x => x.LoginTime)
                .FirstOrDefaultAsync();

            if (lastAudit != null)
                lastAudit.LogoutTime = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                metadata = new { code = "200", message = "Logged out" }
            });
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetUserProfile()
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value;

            var user = await userManager.FindByNameAsync(username);
            var userRoles = await userManager.GetRolesAsync(user);

            // Ambil roles beserta akses
            var roles = await (
                from usr in _context.Users
                join userRole in _context.UserRoles.AsNoTracking() on usr.Id equals userRole.UserId
                join role in _context.Roles on userRole.RoleId equals role.Id
                where usr.UserName == username
                select role
            ).ToListAsync();

            var _acces = new List<AccesModel>();

            foreach (var role in roles)
            {
                if (!string.IsNullOrEmpty(role.Access))
                {
                    var accessList = JsonConvert.DeserializeObject<IEnumerable<AccesModel>>(role.Access);
                    _acces.AddRange(accessList.Select(a => new AccesModel
                    {
                        IdController = a.IdController,
                        IdAction = a.IdAction
                    }));
                }
            }

            var distinctList = _acces.GroupBy(s => s.IdAction).Select(s => s.First()).ToList();
            ModulClass xData = new ModulClass();
            //var menuItem = xData.Action().Where(x => x.NamaAction.ToLower().Contains("lihat")).ToList();

            /* new */
            var allControllers = xData.GetListMenu()
                .SelectMany(m => m.ControllerViewModel ?? new List<ControllerViewModel>())
                .ToList();

            var userAccessibleControllers = allControllers
                .Select(controller => new ControllerViewModel
                {
                    IdController = controller.IdController,
                    NoUrut = controller.NoUrut,
                    Controller = controller.Controller,
                    IdMenu = controller.IdMenu,
                    ActionViewModel = (controller.ActionViewModel ?? new List<ActionViewModel>())
                        .Where(action =>
                            //!string.IsNullOrEmpty(action.NamaAction) &&
                            //action.NamaAction.ToLower().Contains("lihat") &&
                            distinctList.Any(a =>
                                a.IdController == controller.IdController &&
                                a.IdAction == action.IdAction)
                        )
                        .ToList()
                })
                .Where(ctrl => ctrl.ActionViewModel.Any())
                .ToList();

            var sendRole = userAccessibleControllers
            .SelectMany(ctrl => ctrl.ActionViewModel
                //.Where(a => !string.IsNullOrEmpty(a.NamaAction) &&
                //            a.NamaAction.ToLower().Contains("lihat"))
                .Select(a => new FilterMenuWeb
                {
                    action = a.IdAction,
                    subject = a.IdController
                }))
            .ToList();

            // =====================================================
            // CONTROLLER YANG BOLEH MUNCUL DI MENU (HANYA "LIHAT")
            // =====================================================
            var allowedControllers = userAccessibleControllers
                .Where(c => c.ActionViewModel.Any(a =>
                    a.NamaAction.Equals("Lihat", StringComparison.OrdinalIgnoreCase)))
                .Select(c => c.IdController)
                .ToHashSet();

            // =====================================================
            // MAPPING MENU → CONTROLLER
            // =====================================================
            var menuControllerMap = new Dictionary<string, string>
            {
                ["/konfigurasi/akun"] = "Akun",
                ["/konfigurasi/group"] = "Role",
                ["/konfigurasi/audit-login"] = "AuditLogin",

                ["/master-data/cabang"] = "MasterCabang",
                ["/master-data/mesin-atm"] = "MasterMesin",
                ["/master-data/kaset"] = "MasterKaset",
                ["/master-data/bank"] = "MasterBank",
                ["/master-data/merek-kaset"] = "MasterMerekKaset",
                ["/master-data/denom"] = "MasterDenom",

                ["/logistik/register-seal"] = "RegisterSeal",

                ["/cpc/order-pengisian-kaset"] = "OrderCpc",
                ["/cpc/penerimaan-sisa-lokasi"] = "PengembalianKaset",
                ["/cpc/form-cadangan"] = "OrderCpc",
                ["/cpc/tracking-kaset"] = "TrackingKaset",
                ["/cpc/stok-vault"] = "StokVault",
            };


            // Buat sendMenu ala front-end
            var sendMenu = new List<object>
            {
                // ======================
                // HOME
                // ======================
                new { navlabel = true, subheader = "Home" },
                new {
                    id = Guid.NewGuid(),
                    title = "Dashboard",
                    icon = "IconChartHistogram",
                    href = "/"
                },

                // ======================
                // KONFIGURASI
                // ======================
                new { navlabel = true, subheader = "Konfigurasi" },
                new {
                    id = Guid.NewGuid(),
                    title = "User Akun",
                    icon = "IconUsers",
                    href = "/konfigurasi/akun"
                },
                new {
                    id = Guid.NewGuid(),
                    title = "Group Akun",
                    icon = "IconLockAccess",
                    href = "/konfigurasi/group"
                },
                new {
                    id = Guid.NewGuid(),
                    title = "Log Login",
                    icon = "IconHistory",
                    href = "/konfigurasi/audit-login"
                },

                // ======================
                // MASTER DATA
                // ======================
                new { navlabel = true, subheader = "Master Data" },
                new {
                    id = Guid.NewGuid(),
                    title = "Master Cabang",
                    icon = "IconHome",
                    href = "/master-data/cabang"
                },
                new {
                    id = Guid.NewGuid(),
                    title = "Master Bank",
                    icon = "IconBuildingBank",
                    href = "/master-data/bank"
                },
                new
                {
                    id = Guid.NewGuid(),
                    title = "Master CPC",
                    icon = "IconSandbox",
                    href = "#", // parent only
                    children = new List<object>
                    {
                        new {
                            id = Guid.NewGuid(),
                            title = "Master Mesin",
                            icon = "IconWashMachine",
                            href = "/master-data/mesin-atm"
                        },
                        new {
                            id = Guid.NewGuid(),
                            title = "Master Kaset",
                            icon = "IconBriefcase2",
                            href = "/master-data/kaset"
                        },
                        new {
                            id = Guid.NewGuid(),
                            title = "Master Denom",
                            icon = "IconCoin",
                            href = "/master-data/denom"
                        },
                        new {
                            id = Guid.NewGuid(),
                            title = "Master Merek Kaset",
                            icon = "IconFavicon",
                            href = "/master-data/merek-kaset"
                        },
                    }
                },

                // ======================
                // LOGISTIK
                // ======================
                new { navlabel = true, subheader = "Logistik" },
                new {
                    id = Guid.NewGuid(),
                    title = "Register Nomor Seal",
                    icon = "IconBarcode",
                    href = "/logistik/register-seal"
                },

                // ======================
                // CPC
                // ======================
                new { navlabel = true, subheader = "Cash Processing Center" },
                new
                {
                    id = Guid.NewGuid(),
                    title = "Operasional",
                    icon = "IconSandbox",
                    href = "#", // parent only
                    children = new List<object>
                    {
                        new {
                            id = Guid.NewGuid(),
                            title = "Order Pengisian ATM",
                            icon = "IconCashBanknote",
                            href = "/cpc/order-pengisian-kaset"
                        },
                        new {
                            id = Guid.NewGuid(),
                            title = "Sisa Lokasi Atm",
                            icon = "IconMapPin",
                            href = "/cpc/penerimaan-sisa-lokasi"
                        },
                        new {
                            id = Guid.NewGuid(),
                            title = "Form Proses",
                            icon = "IconWashMachine",
                            href = "/cpc/form-cadangan"
                        },
                        new {
                            id = Guid.NewGuid(),
                            title = "Tracking Kaset",
                            icon = "IconMapSearch",
                            href = "/cpc/tracking-kaset"
                        },
                    }
                },
                new
                {
                    id = Guid.NewGuid(),
                    title = "Stock Uang",
                    icon = "IconStackMiddle",
                    href = "#", // parent only
                    children = new List<object>
                    {
                        new {
                            id = Guid.NewGuid(),
                            title = "Stock Vault",
                            icon = "IconMoneybag",
                            href = "/cpc/stok-vault"
                        },
                    }
                },


                new { navlabel = true, subheader = "Cash Vault" },





            };

            // =====================================================
            // FILTER MENU BERDASARKAN HAK LIHAT
            // =====================================================
            List<object> FilterMenu(List<object> menus)
            {
                var result = new List<object>();
                object? pendingNavLabel = null;

                foreach (var item in menus)
                {
                    // =====================================
                    // NAVLABEL → TAHAN DULU
                    // =====================================
                    if (item.GetType().GetProperty("navlabel") != null)
                    {
                        pendingNavLabel = item;
                        continue;
                    }

                    var hrefProp = item.GetType().GetProperty("href");
                    var childrenProp = item.GetType().GetProperty("children");

                    var href = hrefProp?.GetValue(item)?.ToString();

                    // =====================================
                    // DASHBOARD (GLOBAL)
                    // =====================================
                    if (href == "/")
                    {
                        if (pendingNavLabel != null)
                        {
                            result.Add(pendingNavLabel);
                            pendingNavLabel = null;
                        }

                        result.Add(item);
                        continue;
                    }

                    // =====================================
                    // MENU DENGAN CHILD (PARENT)
                    // =====================================
                    if (childrenProp != null)
                    {
                        var children = childrenProp.GetValue(item) as IEnumerable<object>;
                        if (children == null) continue;

                        var validChildren = new List<object>();

                        foreach (var child in children)
                        {
                            var childHref = child.GetType()
                                .GetProperty("href")
                                ?.GetValue(child)
                                ?.ToString();

                            if (string.IsNullOrWhiteSpace(childHref) || childHref == "#")
                                continue;

                            if (!menuControllerMap.TryGetValue(childHref, out var controller))
                                continue;

                            if (!allowedControllers.Contains(controller))
                                continue;

                            // ✅ child valid
                            validChildren.Add(child);
                        }

                        // ❌ TIDAK ADA CHILD VALID → SKIP PARENT
                        if (!validChildren.Any())
                            continue;

                        // ✅ ADA CHILD VALID → TAMBAHKAN
                        if (pendingNavLabel != null)
                        {
                            result.Add(pendingNavLabel);
                            pendingNavLabel = null;
                        }

                        // rebuild parent dengan child hasil filter
                        result.Add(new
                        {
                            id = item.GetType().GetProperty("id")?.GetValue(item),
                            title = item.GetType().GetProperty("title")?.GetValue(item),
                            icon = item.GetType().GetProperty("icon")?.GetValue(item),
                            href = "#",
                            children = validChildren
                        });

                        continue;
                    }

                    // =====================================
                    // MENU BIASA
                    // =====================================
                    if (!string.IsNullOrEmpty(href) &&
                        menuControllerMap.TryGetValue(href, out var ctrl) &&
                        allowedControllers.Contains(ctrl))
                    {
                        if (pendingNavLabel != null)
                        {
                            result.Add(pendingNavLabel);
                            pendingNavLabel = null;
                        }

                        result.Add(item);
                    }
                }

                return result;
            }

            var filteredMenu = FilterMenu(sendMenu);

            return Ok(new
            {
                metadata = new { code = "200", message = "ok" },
                response = new
                {
                    user = new
                    {
                        fullName = user.FullName,
                        username = user.UserName,
                        avatar = user.Photo,
                        group = userRoles,
                        role = userRoles.FirstOrDefault()
                    },
                    acces = sendRole,
                    Menu = filteredMenu,
                    XMenu = sendMenu
                }
            });
        }


        // =====================================================
        // Helpers
        // =====================================================
        private JwtSecurityToken BuildAccessToken(string username, int sessionVersion)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username),
                new Claim("SessionVersion", sessionVersion.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["JWT:Secret"])
            );

            return new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.UtcNow.AddMinutes(10),
                claims: claims,
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );
        }

        private RefreshToken GenerateRefreshToken(string userId)
        {
            return new RefreshToken
            {
                UserId = userId,
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                ExpiredAt = DateTime.UtcNow.AddDays(14),
                CreatedAt = DateTime.UtcNow,
                IsUsed = false,
                IsRevoked = false
            };
        }

        private async Task<long?> SaveAuditLogin(string? userId, string? username, string ip, string device, bool success)
        {
            var audit = new AuditLogin
            {
                UserId = userId,
                Username = username,
                IpAddress = ip,
                Device = device,
                UserAgent = device,
                LoginTime = DateTime.UtcNow,
                IsSuccess = success
            };

            _context.AuditLogin.Add(audit);
            await _context.SaveChangesAsync();

            return audit.Id;
        }

        private string GetIpAddress()
        {
            // kalau ada proxy/IIS reverse proxy, X-Forwarded-For bisa berisi "clientIp, proxyIp"
            var xff = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(xff))
                return xff.Split(',')[0].Trim();

            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "-";
        }

        private string GetDeviceInfo()
        {
            return HttpContext.Request.Headers["User-Agent"].ToString();
        }
    
    }
}
