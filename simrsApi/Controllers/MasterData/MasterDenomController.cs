using cpcApi.Data;
using cpcApi.Filter;
using cpcApi.Model.DTO.MasterData;
using cpcApi.Model.DTO;
using cpcApi.Model.MasterData;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace cpcApi.Controllers.MasterData
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MasterDenomController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MasterDenomController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // GET LIST
        // =====================================================
        [ApiKeyAuthorize]
        [HttpGet("GetListMasterDenom")]
        public async Task<ActionResult<PaginatedResponse<FormDenomDto>>> GetListMasterDenom(
            [FromQuery] string? filter = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                var query = _context.MasterDenom.AsNoTracking().AsQueryable();

                if (!string.IsNullOrWhiteSpace(filter))
                {
                    query = query.Where(x =>
                        x.Nominal.ToString().Contains(filter));
                }

                var count = await query.CountAsync();

                var items = await query
                    .OrderBy(x => x.Urutan)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(x => new FormDenomDto
                    {
                        Id = x.Id,
                        Nominal = x.Nominal,
                        Urutan = x.Urutan,
                        Active = x.Active
                    })
                    .ToListAsync();

                return Ok(new PaginatedResponse<FormDenomDto>
                {
                    Data = items,
                    TotalCount = count,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling(count / (double)pageSize)
                });
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<object>.Error(ex.Message, "500"));
            }
        }

        // =====================================================
        // GET DETAIL
        // =====================================================
        [HttpGet("{nominal}")]
        public async Task<ActionResult<ApiResponse<object>>> GetMasterDenom(int nominal)
        {
            var item = await _context.MasterDenom
                .FirstOrDefaultAsync(x => x.Nominal == nominal);

            if (item == null)
                return Ok(ApiResponse<object>.Error("Data not found", "404"));

            return Ok(ApiResponse<FormDenomDto>.Success(new FormDenomDto
            {
                Nominal = item.Nominal,
                Urutan = item.Urutan,
                Active = item.Active
            }));
        }

        // =====================================================
        // CREATE
        // =====================================================
        [ApiKeyAuthorize]
        [HttpPost]
        public async Task<ActionResult<ApiResponse<object>>> PostMasterDenom([FromBody] FormDenomDto item)
        {
            try
            {
                if (item.Nominal <= 0)
                    return Ok(ApiResponse<object>.Error("Nominal harus lebih dari 0", "400"));

                var exists = await _context.MasterDenom
                    .AnyAsync(x => x.Id == item.Id);

                if (exists)
                    return Ok(ApiResponse<object>.Error("Denom sudah ada", "400"));

                var denom = new MasterDenom
                {
                    Nominal = item.Nominal,
                    Urutan = item.Urutan,
                    Active = item.Active
                };

                _context.MasterDenom.Add(denom);
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<object>.SuccessNoData("Data created"));
            }
            catch (Exception ex)    
            {
                return Ok(ApiResponse<object>.Error(ex.Message, "500"));
            }
        }

        // =====================================================
        // UPDATE
        // =====================================================
        [ApiKeyAuthorize]
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> PutMasterDenom(int id, [FromBody] FormDenomDto item)
        {
            try
            {

                var denom = await _context.MasterDenom
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (denom == null)
                    return Ok(ApiResponse<object>.Error("Data not found", "404"));

                denom.Nominal = item.Nominal;
                denom.Urutan = item.Urutan;
                denom.Active = item.Active;

                await _context.SaveChangesAsync();

                return Ok(ApiResponse<object>.SuccessNoData("Data updated"));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<object>.Error(ex.Message, "500"));
            }
        }

        // =====================================================
        // DELETE
        // =====================================================
        [ApiKeyAuthorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteMasterDenom(int id)
        {
            try
            {
                var denom = await _context.MasterDenom
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (denom == null)
                    return Ok(ApiResponse<object>.Error("Data not found", "404"));

                // 🔥 Contoh validasi jika sudah dipakai transaksi
                /*
                var used = await _context.OrderCpcDetail
                    .AnyAsync(x => x.Nominal == nominal);

                if (used)
                    return Ok(ApiResponse<object>.Error("Denom sudah dipakai transaksi", "400"));
                */

                _context.MasterDenom.Remove(denom);
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<object>.SuccessNoData("Data deleted"));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<object>.Error(ex.Message, "500"));
            }
        }
    }
}
