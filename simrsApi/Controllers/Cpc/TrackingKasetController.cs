using cpcApi.Data;
using cpcApi.Model.DTO.Cpc;
using cpcApi.Model.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace cpcApi.Controllers.Cpc
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TrackingKasetController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TrackingKasetController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // GET LIST TRACKING
        // =====================================================
        [HttpGet("GetListTrackingKaset")]
        public async Task<ActionResult<PaginatedResponse<TrackingKasetDto>>> GetListTrackingKaset(
            [FromQuery] string? filter,
            [FromQuery] string? kdCabang,
            [FromQuery] string? status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = _context.KasetStock
                .Include(x => x.Kaset)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter))
            {
                query = query.Where(x =>
                    x.KdKaset.Contains(filter));
            }

            if (!string.IsNullOrWhiteSpace(kdCabang))
            {
                query = query.Where(x => x.Kaset.KdCabang == kdCabang);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(x => x.Status == status);
            }

            var totalCount = await query.CountAsync();

            var data = await query
                .OrderByDescending(x => x.UpdatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new TrackingKasetDto
                {
                    KdKaset = x.KdKaset,
                    NmBank = x.Kaset.NmBank,
                    KdCabang = x.Kaset.KdCabang,
                    Status = x.Status,
                    LocationType = x.LocationType,
                    LocationId = x.LocationId,
                    StatusFisik = x.Kaset.StatusFisik,
                    UpdatedAt = x.UpdatedAt
                })
                .ToListAsync();

            return Ok(new PaginatedResponse<TrackingKasetDto>
            {
                Data = data,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            });
        }

        // =====================================================
        // GET HISTORY PER KASET
        // =====================================================
        [HttpGet("GetHistory/{kdKaset}")]
        public async Task<ActionResult<ApiResponse<List<KasetMovementDto>>>> GetHistory(string kdKaset)
        {
            var movements = await _context.KasetMovement
                .Where(x => x.KdKaset == kdKaset)
                .OrderByDescending(x => x.createdat)
                .Select(x => new KasetMovementDto
                {
                    IdMovement = x.IdMovement,
                    Action = x.Action,
                    FromLocation = x.FromLocation,
                    ToLocation = x.ToLocation,
                    NoWO = x.NoWO,
                    Wsid = x.Wsid,
                    CreatedAt = x.createdat
                })
                .ToListAsync();

            return Ok(ApiResponse<List<KasetMovementDto>>
                .Success(movements));
        }
    }
}
