using cpcApi.Data;
using cpcApi.Model;
using cpcApi.Model.DTO;
using cpcApi.Model.DTO.konfigurasi;
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
        private readonly RoleManager<ApplicationRole> roleManager;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, IConfiguration configuration, ApplicationDbContext context)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            _configuration = configuration;
            _context = context;
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] FormLoginDto model)
        {
            var user = await userManager.FindByNameAsync(model.username);


            if (user == null)
            {
                return Ok(new
                {
                    metadata = new { code = "201", message = "Username tidak ditemukan" },
                    response = ""

                });
            }

            if (user.Active == false)
            {
                return Ok(new
                {
                    metadata = new { code = "201", message = "Pengguna sudah tidak aktif" },
                    response = ""

                });
            }

            if (await userManager.CheckPasswordAsync(user, model.password) == false)
            {
                //return BadRequest(new { message = "Pasword tidak sesuai" });
                return Ok(new
                {
                    metadata = new { code = "201", message = "Pasword tidak sesuai" },
                    response = ""

                });
            }

            if (user != null && await userManager.CheckPasswordAsync(user, model.password))
            {
                var userRoles = await userManager.GetRolesAsync(user);
                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }

                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

                var token = new JwtSecurityToken(
                    issuer: _configuration["JWT:ValidIssuer"],
                    audience: _configuration["JWT:ValidAudience"],
                    //expires: DateTime.Now.AddYears(1),
                    expires: DateTime.Now.AddMinutes(10), // 🔥 PENTING
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                    );

                var refreshToken = GenerateRefreshToken(user.Id);

                _context.RefreshToken.Add(refreshToken);
                await _context.SaveChangesAsync();

                var roles = await (
                                from usr in _context.Users
                                join userRole in _context.UserRoles.AsNoTracking() on usr.Id equals userRole.UserId
                                join role in _context.Roles on userRole.RoleId equals role.Id
                                where usr.UserName == user.UserName
                                select role
                            ).ToListAsync();

                var _acces = new List<AccesModel>();

                if (roles.Count() > 0)
                {
                    foreach (var role in roles)
                    {
                        if (!string.IsNullOrWhiteSpace(role.Access))
                        {
                            var accessList = JsonConvert.DeserializeObject<IEnumerable<AccesModel>>(role.Access);
                            foreach (var _accessList in accessList)
                            {

                                _acces.Add(new AccesModel
                                {
                                    IdController = _accessList.IdController,
                                    IdAction = _accessList.IdAction,
                                });
                            }
                        }


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

                string _token = new JwtSecurityTokenHandler().WriteToken(token);
                return Ok(new
                {
                    metadata = new { code = "200", message = "ok" },
                    response = new
                    {
                        token = _token,
                        refreshToken = refreshToken.Token,
                        expiresIn = 600
                        //user = new {
                        //    fullName = user.FullName,
                        //    username = user.UserName,
                        //    avatar = user.Photo,
                        //    group = userRoles,
                        //    role = userRoles[0]
                        //},
                        //acces = sendRole,
                        //akses = _configuration["HeaderValidation:SecretKey"],
                    }
                });
            }
            return Ok(new
            {
                metadata = new { code = "201", message = "Tidak bisa login" },
                response = ""

            });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromHeader(Name = "X-Refresh-Token")] string refreshToken)
        {
            var storedToken = await _context.RefreshToken
                .FirstOrDefaultAsync(x => x.Token == refreshToken);

            // ❌ token tidak ditemukan / expired / sudah dipakai
            if (storedToken == null ||
                storedToken.ExpiredAt < DateTime.UtcNow ||
                storedToken.IsUsed ||
                storedToken.IsRevoked)
            {
                return Unauthorized(new
                {
                    metadata = new { code = "401", message = "Invalid refresh token" }
                });
            }

            // 🔒 tandai token lama sudah dipakai
            storedToken.IsUsed = true;

            var user = await userManager.FindByIdAsync(storedToken.UserId);

            // 🔁 buat access token baru
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["JWT:Secret"])
            );

            var newAccessToken = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.UtcNow.AddMinutes(15),
                claims: claims,
                signingCredentials: new SigningCredentials(
                    key,
                    SecurityAlgorithms.HmacSha256
                )
            );

            // 🔁 generate refresh token BARU
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

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            var user = await userManager.FindByNameAsync(username);

            var tokens = _context.RefreshToken
                .Where(x => x.UserId == user.Id && !x.IsRevoked);

            foreach (var token in tokens)
            {
                token.IsRevoked = true;
            }

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

                ["/master-data/cabang"] = "MasterCabang",
                ["/master-data/mesin-atm"] = "MasterMesin",
                ["/master-data/kaset"] = "MasterKaset",
                ["/master-data/bank"] = "MasterBank",
                ["/master-data/merek-kaset"] = "MasterMerekKaset",

                ["/logistik/register-seal"] = "RegisterSeal",

                ["/cpc/order-pengisian-kaset"] = "OrderCpc",
                ["/cpc/penerimaan-sisa-lokasi"] = "PengembalianKaset",
                ["/cpc/form-cadangan"] = "OrderCpc",
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
                new
                {
                    id = Guid.NewGuid(),
                    title = "Master Pendukung",
                    icon = "IconAdjustmentsAlt",
                    href = "#", // parent only
                    children = new List<object>
                    {
                        new {
                            id = Guid.NewGuid(),
                            title = "Master Bank",
                            icon = "IconPoint",
                            href = "/master-data/bank"
                        },
                        new {
                            id = Guid.NewGuid(),
                            title = "Master Merek Kaset",
                            icon = "IconPoint",
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
                    title = "Form Cadangan CPC ATM",
                    icon = "IconWashMachine",
                    href = "/cpc/form-cadangan"
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
    }
}
