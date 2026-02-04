using cpcApi.Data;
using cpcApi.Filter;
using cpcApi.Model.DTO;
using cpcApi.Model.MasterData;
using cpcApi.Services.Cabang;
using cpcApi.Services.Mesin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace cpcApi.Controllers.MasterData
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MasterMesinController : ControllerBase
    {
        private readonly DapperSistagContext _daperContext;
        private readonly IRepoMesin _mesin;
        private readonly ApplicationDbContext _context;
        public MasterMesinController(ApplicationDbContext context, DapperSistagContext daperContext, IRepoMesin mesin)
        {
            _daperContext = daperContext;
            _mesin = mesin;
            _context = context;
        }

        [ApiKeyAuthorize]
        [HttpGet]
        [Route("GetListMesin")]
        public async Task<ActionResult<PaginatedResponse<MasterMesin>>> GetListMesin(
        [FromQuery] string? cabang = null,
        [FromQuery] string? wsid = null,
        [FromQuery] string? merek = null,
        [FromQuery] string? tipe = null,
        [FromQuery] string? kdAreaCr = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                // 🔹 Panggil repository (SESUAI INTERFACE)
                var result = await _mesin.GetMesin(
                    cabang,
                    wsid,
                    merek,
                    tipe,
                    kdAreaCr,
                    page,
                    pageSize);

                // 🔹 Mapping ke DTO
                //var items = result.Data.Select(x => new MasterMesin
                //{
                //    KODE = x.KODE,
                //    WSID = x.WSID,
                //    NMBANK = x.NMBANK,
                //    LOKASI = x.LOKASI,
                //    TIPE = x.TIPE,
                //    MEREK = x.MEREK,
                //    KDAREACR = x.KDAREACR,
                //    NMAREACR = x.NMAREACR,
                //    STMESIN = x.STMESIN,
                //    LIMITSALDO = x.LIMITSALDO
                //}).ToList();

                return Ok(new PaginatedResponse<MasterMesin>
                {
                    Data = result.Data,
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
