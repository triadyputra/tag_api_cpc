using cpcApi.Data;
using cpcApi.Filter;
using cpcApi.Model.DTO.konfigurasi;
using cpcApi.Model.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace cpcApi.Controllers.konfigurasi
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AuditLoginController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AuditLoginController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // GET LIST AUDIT LOGIN
        // =====================================================
        [ApiKeyAuthorize]
        [HttpGet("GetListAuditLogin")]
        public async Task<ActionResult<PaginatedResponse<ViewAuditLoginDto>>> GetListAuditLogin(
            [FromQuery] string? username = null,
            [FromQuery] bool? isSuccess = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                var query = _context.AuditLogin
                    .AsNoTracking()
                    .AsQueryable();

                // ================= FILTER USERNAME =================
                if (!string.IsNullOrWhiteSpace(username))
                {
                    query = query.Where(x =>
                        x.Username != null &&
                        x.Username.ToLower().Contains(username.ToLower()));
                }

                // ================= FILTER SUCCESS =================
                if (isSuccess.HasValue)
                {
                    query = query.Where(x => x.IsSuccess == isSuccess.Value);
                }

                // ================= FILTER DATE RANGE =================
                if (startDate.HasValue)
                {
                    var start = DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc);
                    query = query.Where(x => x.LoginTime >= start);
                }

                if (endDate.HasValue)
                {
                    var end = DateTime.SpecifyKind(endDate.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);
                    query = query.Where(x => x.LoginTime <= end);
                }

                var count = await query.CountAsync();

                var items = await query
                    .OrderByDescending(x => x.LoginTime)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(x => new ViewAuditLoginDto
                    {
                        Id = x.Id,
                        UserId = x.UserId,
                        Username = x.Username,
                        IpAddress = x.IpAddress,
                        Device = x.Device,
                        UserAgent = x.UserAgent,
                        LoginTime = x.LoginTime.AddHours(7),   // WIB
                        LogoutTime = x.LogoutTime.HasValue ? x.LogoutTime.Value.AddHours(7) : null,
                        IsSuccess = x.IsSuccess
                    })
                    .ToListAsync();

                return Ok(new PaginatedResponse<ViewAuditLoginDto>
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
    }
}
