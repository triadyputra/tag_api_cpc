using cpcApi.Data;
using cpcApi.Filter;
using cpcApi.Model.DTO;
using cpcApi.Model.DTO.MasterData.MasterJenisKaset;
using cpcApi.Model.MasterData;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace cpcApi.Controllers.MasterData
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MasterMerekKasetController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MasterMerekKasetController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // GET LIST (Paging + Filter)
        // =====================================================
        [ApiKeyAuthorize]
        [HttpGet("GetListMasterMerekKaset")]
        public async Task<ActionResult<PaginatedResponse<ViewMasterMerekKasetDto>>> GetListMasterMerekKaset(
            [FromQuery] string? filter = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                IQueryable<MasterMerekKaset> query = _context.MasterMerekKaset
                    .AsNoTracking();

                if (!string.IsNullOrWhiteSpace(filter))
                {
                    var like = $"%{filter}%";

                    query = query.Where(x =>
                        EF.Functions.Like(x.KdMerek, like) ||
                        EF.Functions.Like(x.NmMerek, like)
                    );
                }

                var count = await query.CountAsync();

                var items = await query
                    .OrderBy(x => x.NmMerek)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(x => new ViewMasterMerekKasetDto
                    {
                        KdMerek = x.KdMerek,
                        NmMerek = x.NmMerek,
                        Keterangan = x.Keterangan,
                        Aktif = x.Aktif
                    })
                    .ToListAsync();

                return Ok(new PaginatedResponse<ViewMasterMerekKasetDto>
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
        // GET BY ID
        // =====================================================
        //[ApiKeyAuthorize]
        [HttpGet("{kdMerek}")]
        public async Task<ActionResult<ApiResponse<FormMasterMerekKasetDto>>> GetById(string kdMerek)
        {
            var data = await _context.MasterMerekKaset
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.KdMerek == kdMerek);

            if (data == null)
                return Ok(ApiResponse<object>.Error("Data tidak ditemukan", "404"));

            return Ok(ApiResponse<FormMasterMerekKasetDto>.Success(new FormMasterMerekKasetDto
            {
                KdMerek = data.KdMerek,
                NmMerek = data.NmMerek,
                Keterangan = data.Keterangan,
                Aktif = data.Aktif
            }));
        }

        // =====================================================
        // CREATE
        // =====================================================
        [ApiKeyAuthorize]
        [HttpPost]
        public async Task<ActionResult<ApiResponse<object>>> PostMasterMerekKaset([FromBody] FormMasterMerekKasetDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.KdMerek))
                    return Ok(ApiResponse<object>.Error("Kode merek wajib diisi", "400"));

                if (string.IsNullOrWhiteSpace(dto.NmMerek))
                    return Ok(ApiResponse<object>.Error("Nama merek wajib diisi", "400"));

                var exists = await _context.MasterMerekKaset
                    .AnyAsync(x => x.KdMerek == dto.KdMerek);

                if (exists)
                    return Ok(ApiResponse<object>.Error("Kode merek sudah terdaftar", "400"));

                var entity = new MasterMerekKaset
                {
                    KdMerek = dto.KdMerek,
                    NmMerek = dto.NmMerek,
                    Keterangan = dto.Keterangan,
                    Aktif = dto.Aktif
                };

                _context.MasterMerekKaset.Add(entity);
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<object>.SuccessNoData());
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
        [HttpPut("{kdMerek}")]
        public async Task<ActionResult<ApiResponse<object>>> PutMasterMerekKaset(string kdMerek, [FromBody] FormMasterMerekKasetDto dto)
        {
            try
            {
                var entity = await _context.MasterMerekKaset
                    .FirstOrDefaultAsync(x => x.KdMerek == kdMerek);

                if (entity == null)
                    return Ok(ApiResponse<object>.Error("Data tidak ditemukan", "404"));

                entity.NmMerek = dto.NmMerek;
                entity.Keterangan = dto.Keterangan;
                entity.Aktif = dto.Aktif;

                await _context.SaveChangesAsync();

                return Ok(ApiResponse<object>.SuccessNoData());
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<object>.Error(ex.Message, "500"));
            }
        }

        // =====================================================
        // NONAKTIFKAN (SAFE DELETE)
        // =====================================================
        [ApiKeyAuthorize]
        [HttpDelete("{kdMerek}")]
        public async Task<ActionResult<ApiResponse<object>>> DisableMasterMerekKaset(string kdMerek)
        {
            var entity = await _context.MasterMerekKaset
                .FirstOrDefaultAsync(x => x.KdMerek == kdMerek);

            if (entity == null)
                return Ok(ApiResponse<object>.Error("Data tidak ditemukan", "404"));

            entity.Aktif = false;
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<object>.SuccessNoData("Merek berhasil dinonaktifkan"));
        }
    }
}
