using cpcApi.Data;
using cpcApi.Filter;
using cpcApi.Model.DTO;
using cpcApi.Model.MasterData;
using cpcApi.Services.Cabang;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace cpcApi.Controllers.MasterData
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MasterCabangController : ControllerBase
    {
        private readonly DapperSistagContext _daperContext;
        private readonly IRepoCabang _cabang;
        private readonly ApplicationDbContext _context;
        public MasterCabangController(ApplicationDbContext context, DapperSistagContext daperContext, IRepoCabang cabang)
        {
            _daperContext = daperContext;
            _cabang = cabang;
            _context = context;
        }

        [ApiKeyAuthorize]
        [HttpGet]
        [Route("GetListCabang")]
        public async Task<ActionResult<IEnumerable<dynamic>>> GetListCabang(string? filter, int page = 1, int pageSize = 10)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                // 🔹 Ambil data dari repository
                var result = await _cabang.GetCabang(filter, page, pageSize);

                // 🔹 Mapping ke DTO
                var items = result.Data.Select(x => new MasterCabang
                {
                    KDCABANG = x.KDCABANG,
                    NMCABANG = x.NMCABANG,
                    ALAMAT = x.ALAMAT,
                    TELEPON = x.TELEPON,
                    FAX = x.FAX,
                    KDPOS = x.KDPOS,
                    KOTA = x.KOTA,
                    KACAB = x.KACAB,
                    GRUP = x.GRUP,
                    NOCAB = x.NOCAB,
                    KODECAB = x.KODECAB,
                }).ToList();

                return Ok(new PaginatedResponse<MasterCabang>
                {
                    Data = items,
                    TotalCount = result.Total,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling(result.Total / (double)pageSize)
                });
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<object>.Error(ex.Message, "500"));
            }
        }
    }
}
