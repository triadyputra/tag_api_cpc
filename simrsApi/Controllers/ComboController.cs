using cpcApi.Data;
using cpcApi.Model;
using cpcApi.Model.DTO;
using cpcApi.Model.DTO.Cpc.orderkaset;
using cpcApi.Model.ENUM;
using cpcApi.Services.Combo;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection;


namespace cpcApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ComboController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<ApplicationRole> roleManager;
        private readonly IRepoCombo _comboRepository;

        public ComboController(ApplicationDbContext context, RoleManager<ApplicationRole> roleManager, IRepoCombo comboRepository)
        {
            _context = context;
            this.roleManager = roleManager;
            _comboRepository = comboRepository;
        }

        [HttpGet]
        [Route("ComboGroup")]
        public async Task<IActionResult> ComboGroup()
        {
            var Group = await roleManager.Roles.ToListAsync();
            var listGroup = Group.Select(p => new { value = p.Name, title = p.Name });

            return Ok(listGroup);
        }

        [HttpGet]
        [Route("ComboCabang")]
        public async Task<IActionResult> ComboCabang()
        {
            var cabang = await _comboRepository.ComboCabang();

            return Ok(cabang);
        }

        [HttpGet]
        [Route("ComboBank")]
        public async Task<IActionResult> ComboBank()
        {
            var cabang = await _comboRepository.ComboBank();

            return Ok(cabang);
        }

        [HttpGet]
        [Route("ComboMerekKaset")]
        public async Task<IActionResult> ComboMerekKaset()
        {
            var data = await _context.MasterMerekKaset.ToListAsync();
            var Opt = data.Where(x => x.Aktif == true).Select(p => new { Id = p.KdMerek, Name = p.NmMerek });

            return Ok(Opt);
        }

        [HttpGet]
        [Route("ComboTipeKaset")]
        public IActionResult ComboTipeKaset()
        {
            var result = new List<ComboViewModel>
            {
                new ComboViewModel
                {
                    Id = "ATM",
                    Name = "ATM",
                    Keterangan = "Automated Teller Machine"
                },
                new ComboViewModel
                {
                    Id = "CRM",
                    Name = "CRM",
                    Keterangan = "Cash Recycling Machine"
                }
            };

            return Ok(result);
        }

        [HttpGet]
        [Route("ComboJenisKaset")]
        public IActionResult ComboJenisKaset()
        {
            var result = typeof(KasetJenis)
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(f => f.FieldType == typeof(string))
                .Select(f =>
                {
                    var value = f.GetValue(null)?.ToString()!;
                    return new ComboViewModel
                    {
                        Id = value,
                        Name = value,
                        Keterangan = string.Empty
                    };
                })
                .ToList();

            return Ok(result);
        }

        [HttpGet]
        [Route("ComboStatusKaset")]
        public IActionResult ComboStatusKaset()
        {
            var result = typeof(KasetStatus)
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(f => f.FieldType == typeof(string))
                .Select(f =>
                {
                    var value = f.GetValue(null)?.ToString()!;
                    return new ComboViewModel
                    {
                        Id = value,
                        Name = value,
                        Keterangan = string.Empty
                    };
                })
                .ToList();

            return Ok(result);
        }

        [HttpGet]
        [Route("ComboKasetByCab")]
        public async Task<IActionResult> ComboKasetByCab(string kdcab, string bank)
        {
            var data = await _context.MasterKaset.Where(x=> x.KdCabang == kdcab && x.KdBank == bank).Where(x=> x.StatusFisik == "EMPTY").ToListAsync();
            var Opt = data.Select(p => new { Id = p.IdKaset, Name = p.KdKasetBank });

            return Ok(Opt);
        }

        [HttpGet]
        [Route("SearchForPengembalian")]
        public async Task<IActionResult> SearchForPengembalian(
            [FromQuery] string kode,
            [FromQuery] int limit = 10
        )
        {
            if (string.IsNullOrWhiteSpace(kode))
                return Ok(new { Data = new List<OrderPengisianKasetLookupDto>() });

            var today = DateTime.Today; // ⬅️ hari ini jam 00:00

            var data = await _context.OrderPengisianKaset
                .AsNoTracking()
                .Include(x => x.Details)
                .Where(x =>
                    (x.NomorMesin.Contains(kode) || x.Id.Contains(kode))
                    && x.TanggalOrder < today                        // ⛔ KECUALI HARI INI
                    && !_context.PengembalianKaset
                        .Any(p => p.OrderPengisianId == x.Id)
                )
                .OrderByDescending(x => x.createdat)              // 🔥 PALING TERAKHIR
                .Take(limit)
                .Select(x => new OrderPengisianKasetLookupDto
                {
                    Id = x.Id,
                    NomorMesin = x.NomorMesin,
                    Lokasi = x.Lokasi,
                    MerekMesin = x.MerekMesin,
                    KDBANK = x.KDBANK,
                    KDCABANG = x.KDCABANG,
                    Jumlah = x.Jumlah,
                    Details = x.Details
                        .OrderBy(d => d.Kaset)
                        .Select(d => new OrderPengisianKasetDetailLookupDto
                        {
                            Kaset = d.Kaset,
                            KodeKaset = d.KodeKaset,
                            NoSeal = d.NoSeal,
                            Denom = d.Denom,
                            Lembar = d.Lembar
                        }).ToList()
                })
                .ToListAsync();

            return Ok(new { Data = data });
        }
    }
}
