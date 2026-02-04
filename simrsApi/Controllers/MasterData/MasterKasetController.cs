using cpcApi.Data;
using cpcApi.Filter;
using cpcApi.Model.DTO;
using cpcApi.Model.DTO.MasterData.MasterKaset;
using cpcApi.Model.MasterData;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace cpcApi.Controllers.MasterData
{
    //[Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MasterKasetController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MasterKasetController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // GET LIST
        // =====================================================
        [ApiKeyAuthorize]
        [HttpGet("GetListMasterKaset")]
        public async Task<ActionResult<PaginatedResponse<ViewMasterKasetDto>>> GetListMasterKaset(
            [FromQuery] string? filter = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                IQueryable<MasterKaset> query = _context.MasterKaset
                    .AsNoTracking();

                if (!string.IsNullOrWhiteSpace(filter))
                {
                    var like = $"%{filter}%";

                    query = query.Where(x =>
                        EF.Functions.Like(x.NoSerial, like) ||
                        EF.Functions.Like(x.KdBank, like) ||
                        EF.Functions.Like(x.NmBank, like)
                    );
                }

                var count = await query.CountAsync();

                var items = await query
                    .OrderBy(x => x.NoSerial)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(x => new ViewMasterKasetDto
                    {
                        IdKaset = x.IdKaset,
                        NoSerial = x.NoSerial,

                        KdBank = x.KdBank,
                        NmBank = x.NmBank,

                        KdMerek = x.KdMerek,
                        NmMerek = x.Merek != null ? x.Merek.NmMerek : "-",

                        Tipe = x.Tipe,
                        Jenis = x.Jenis,
                        StatusFisik = x.StatusFisik,
                        KdCabang = x.KdCabang
                    })
                    .ToListAsync();

                return Ok(new PaginatedResponse<ViewMasterKasetDto>
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
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<FormMasterKasetDto>>> GetById(string id)
        {
            var kaset = await _context.MasterKaset
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.IdKaset == id);

            if (kaset == null)
                return Ok(ApiResponse<object>.Error("Data tidak ditemukan", "404"));

            return Ok(ApiResponse<FormMasterKasetDto>.Success(new FormMasterKasetDto
            {
                IdKaset = kaset.IdKaset,
                KdKasetBank = kaset.KdKasetBank,
                KdBank = kaset.KdBank,
                NmBank = kaset.NmBank,
                KdMerek = kaset.KdMerek,
                Tipe = kaset.Tipe,
                Jenis = kaset.Jenis,
                NoSerial = kaset.NoSerial,
                StatusFisik = kaset.StatusFisik,
                KdCabang = kaset.KdCabang
            }));
        }

        // =====================================================
        // CREATE
        // =====================================================
        [ApiKeyAuthorize]
        [HttpPost]
        public async Task<ActionResult<ApiResponse<object>>> PostMasterKaset([FromBody] FormMasterKasetDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.NoSerial))
                    return Ok(ApiResponse<object>.Error("NoSerial wajib diisi", "400"));

                var exists = await _context.MasterKaset
                    .AnyAsync(x => x.NoSerial == dto.NoSerial);

                if (exists)
                    return Ok(ApiResponse<object>.Error("NoSerial sudah terdaftar", "400"));

                var kaset = new MasterKaset
                {
                    KdKasetBank = dto.KdKasetBank,
                    KdBank = dto.KdBank,
                    NmBank = dto.NmBank,
                    KdMerek = dto.KdMerek,
                    Tipe = dto.Tipe,
                    Jenis = dto.Jenis,
                    NoSerial = dto.NoSerial,
                    StatusFisik = dto.StatusFisik,
                    KdCabang = dto.KdCabang
                };

                _context.MasterKaset.Add(kaset);
                await _context.SaveChangesAsync();


                // 2️⃣ AUTO CREATE KASET STOCK
                var kasetStock = new KasetStock
                {
                    IdKaset = kaset.IdKaset,     // ✅ BENAR
                    Status = "EMPTY",
                    LocationType = "VAULT",
                    LocationId = dto.KdCabang,   // ✅ BENAR
                    UpdatedAt = DateTime.UtcNow
                };

                _context.KasetStock.Add(kasetStock);
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
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> PutMasterKaset(string id, [FromBody] FormMasterKasetDto dto)
        {
            try
            {
                var kaset = await _context.MasterKaset
                    .FirstOrDefaultAsync(x => x.IdKaset == id);

                if (kaset == null)
                    return Ok(ApiResponse<object>.Error("Data tidak ditemukan", "404"));

                kaset.KdKasetBank = dto.KdKasetBank;
                kaset.KdMerek = dto.KdMerek;
                kaset.Tipe = dto.Tipe;
                kaset.Jenis = dto.Jenis;
                kaset.StatusFisik = dto.StatusFisik;
                kaset.KdCabang = dto.KdCabang;
                kaset.NoSerial = dto.NoSerial;
                kaset.KdBank = dto.KdBank;
                kaset.NmBank = dto.NmBank;
                // ⚠️ BANK TIDAK BOLEH DIUBAH SEMBARANGAN
                // kaset.KdBank & NmBank TIDAK DIUPDATE


                // 2️⃣ AUTO CREATE KASET STOCK
                //var kasetStock = new KasetStock
                //{
                //    IdKaset = kaset.IdKaset,     // ✅ BENAR
                //    Status = "EMPTY",
                //    LocationType = "VAULT",
                //    LocationId = dto.KdCabang,   // ✅ BENAR
                //    UpdatedAt = DateTime.UtcNow
                //};

                //_context.KasetStock.Add(kasetStock);

                await _context.SaveChangesAsync();


                return Ok(ApiResponse<object>.SuccessNoData());
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<object>.Error(ex.Message, "500"));
            }
        }

        // =====================================================
        // DELETE (HARD DELETE – DEV ONLY / ADMIN)
        // =====================================================
        [ApiKeyAuthorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteMasterKaset(string id)
        {
            var kaset = await _context.MasterKaset
                .FirstOrDefaultAsync(x => x.IdKaset == id);

            if (kaset == null)
                return Ok(ApiResponse<object>.Error("Data tidak ditemukan", "404"));

            _context.MasterKaset.Remove(kaset);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<object>.SuccessNoData("Data berhasil dihapus"));
        }
    }
}
