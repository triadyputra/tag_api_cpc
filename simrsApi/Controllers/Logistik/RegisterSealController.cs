using cpcApi.Data;
using cpcApi.Filter;
using cpcApi.Model.DTO.Cpc.logistik;
using cpcApi.Model.DTO;
using cpcApi.Model.Logistik;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace cpcApi.Controllers.Logistik
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegisterSealController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RegisterSealController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // GET LIST REGISTER SEAL
        // =====================================================
        [ApiKeyAuthorize]
        [HttpGet("GetListRegisterSeal")]
        public async Task<ActionResult<PaginatedResponse<ViewRegisterSealDto>>> GetListRegisterSeal(
            [FromQuery] string? filter = null,
            [FromQuery] bool? active = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                IQueryable<RegisterSeal> query = _context.RegisterSeal
                    .AsNoTracking();

                // ================= FILTER =================
                if (!string.IsNullOrWhiteSpace(filter))
                {
                    var like = $"%{filter}%";
                    query = query.Where(x =>
                        EF.Functions.Like(x.NomorSeal, like));
                }

                if (active.HasValue)
                {
                    query = query.Where(x => x.Active == active.Value);
                }

                var count = await query.CountAsync();

                var items = await query
                    .OrderBy(x => x.NomorSeal)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(x => new ViewRegisterSealDto
                    {
                        NomorSeal = x.NomorSeal,
                        Status = x.Status,
                        Active = x.Active
                    })
                    .ToListAsync();

                return Ok(new PaginatedResponse<ViewRegisterSealDto>
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
        // CREATE BULK (AUTO RANGE)
        // =====================================================
        [ApiKeyAuthorize]
        [HttpPost]
        public async Task<ActionResult<ApiResponse<object>>> PostRegisterSeal(
        [FromBody] RegisterSealRangeDto dto)
        {
            if (dto.NomorAwal <= 0 || dto.NomorAkhir <= 0)
                return Ok(ApiResponse<object>.Error("Nomor harus > 0", "400"));

            if (dto.NomorAwal > dto.NomorAkhir)
                return Ok(ApiResponse<object>.Error(
                    "NomorAwal tidak boleh lebih besar dari NomorAkhir", "400"));

            var total = dto.NomorAkhir - dto.NomorAwal + 1;

            var strategy = _context.Database.CreateExecutionStrategy();

            try
            {
                ApiResponse<object>? errorResponse = null;

                await strategy.ExecuteAsync(async () =>
                {
                    using var tx = await _context.Database.BeginTransactionAsync();

                    var start = dto.NomorAwal.ToString("D8");
                    var end = dto.NomorAkhir.ToString("D8");

                    var existing = await _context.RegisterSeal
                        .AsNoTracking()
                        .Where(x =>
                            string.Compare(x.NomorSeal, start) >= 0 &&
                            string.Compare(x.NomorSeal, end) <= 0)
                        .Select(x => x.NomorSeal)
                        .FirstOrDefaultAsync();

                    if (existing != null)
                    {
                        // ⛔ ERROR BISNIS → JANGAN THROW
                        errorResponse = ApiResponse<object>.Error(
                            $"Sebagian nomor seal sudah ada ({existing})",
                            "400");

                        return; // keluar dari execution strategy
                    }

                    var list = new List<RegisterSeal>(total);

                    for (int i = dto.NomorAwal; i <= dto.NomorAkhir; i++)
                    {
                        list.Add(new RegisterSeal
                        {
                            NomorSeal = i.ToString("D8"),
                            Active = dto.Active,
                            Status = SealStatus.AVAILABLE,
                        });
                    }

                    _context.RegisterSeal.AddRange(list);
                    await _context.SaveChangesAsync();

                    await tx.CommitAsync();
                });

                // 🔥 jika ada error bisnis
                if (errorResponse != null)
                    return Ok(errorResponse);

                return Ok(ApiResponse<object>.SuccessNoData(
                    $"Berhasil insert {total} nomor seal"));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<object>.Error(
                    ex.InnerException?.Message ?? ex.Message, "500"));
            }
        }


        // =====================================================
        // UPDATE (TANPA DELETE)
        // =====================================================
        [ApiKeyAuthorize]
        [HttpPut("{nomorSeal}")]
        public async Task<ActionResult<ApiResponse<object>>> PutRegisterSeal(
            string nomorSeal,
            [FromBody] RegisterSealUpdateDto dto)
        {
            try
            {
                var data = await _context.RegisterSeal
                    .FirstOrDefaultAsync(x => x.NomorSeal == nomorSeal);

                if (data == null)
                    return Ok(ApiResponse<object>.Error("Data tidak ditemukan", "404"));

                data.Active = dto.Active;

                await _context.SaveChangesAsync();

                return Ok(ApiResponse<object>.SuccessNoData("Data berhasil diupdate"));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<object>.Error(
                    ex.InnerException?.Message ?? ex.Message, "500"));
            }
        }
    }
}
