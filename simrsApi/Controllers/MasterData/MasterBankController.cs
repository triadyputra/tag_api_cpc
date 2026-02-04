using ClosedXML.Excel;
using cpcApi.Data;
using cpcApi.Filter;
using cpcApi.Model.DTO;
using cpcApi.Model.MasterData;
using cpcApi.Services.Bank;
using cpcApi.Services.Mesin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace cpcApi.Controllers.MasterData
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MasterBankController : ControllerBase
    {
        private readonly DapperSistagContext _daperContext;
        private readonly IRepoBank _bank;
        private readonly ApplicationDbContext _context;
        public MasterBankController(ApplicationDbContext context, DapperSistagContext daperContext, IRepoBank bank)
        {
            _daperContext = daperContext;
            _bank = bank;
            _context = context;
        }

        [ApiKeyAuthorize]
        [HttpGet]
        [Route("GetListMasterBank")]
        public async Task<ActionResult<PaginatedResponse<MasterBank>>> GetListMasterBank(
        [FromQuery] string? filter = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                // 🔹 Panggil repository (SESUAI INTERFACE)
                var result = await _bank.GetBank(
                    filter,
                    page,
                    pageSize);

                return Ok(new PaginatedResponse<MasterBank>
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
