using cpcApi.Data;
using cpcApi.Model.Cpc;
using cpcApi.Model.DTO;
using cpcApi.Model.DTO.Cpc.pengembaliankaset;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IdentityModel.Claims;

namespace cpcApi.Controllers.Cpc
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PengembalianKasetController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PengembalianKasetController(ApplicationDbContext context)
        {
            _context = context;
        }


        [HttpGet]
        [Route("GetListPengembalianKaset")]
        public async Task<ActionResult<PaginatedResponse<PengembalianKasetListDto>>> GetListPengembalianKaset(
        [FromQuery] string? filter = null,
        [FromQuery] string? cabang = null,
        [FromQuery] string? bank = null,
        [FromQuery] string? tanggalawal = null,
        [FromQuery] string? tanggalakhir = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10
        )
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                var query = _context.PengembalianKaset.AsQueryable();

                // =============================
                // FILTER TEXT
                // =============================
                if (!string.IsNullOrWhiteSpace(filter))
                {
                    query = query.Where(x =>
                        x.NomorMesin.Contains(filter) ||
                        x.Lokasi.Contains(filter)
                    );
                }

                // =============================
                // FILTER BANK
                // =============================
                if (!string.IsNullOrWhiteSpace(bank))
                {
                    query = query.Where(x => x.KDBANK == bank);
                }

                // =============================
                // FILTER CABANG
                // =============================
                if (!string.IsNullOrWhiteSpace(cabang))
                {
                    query = query.Where(x => x.KDCABANG == cabang);
                }

                // =============================
                // FILTER TANGGAL
                // =============================
                if (DateTime.TryParse(tanggalawal, out var startDate))
                {
                    startDate = startDate.Date; // 00:00:00
                    query = query.Where(x => x.TanggalTerima >= startDate);
                }

                if (DateTime.TryParse(tanggalakhir, out var endDate))
                {
                    endDate = endDate.Date.AddDays(1).AddTicks(-1); // 23:59:59
                    query = query.Where(x => x.TanggalTerima <= endDate);
                }

                // =============================
                // TOTAL DATA
                // =============================
                var total = await query.CountAsync();

                // =============================
                // PAGING DATA
                // =============================
                var data = await query
                    .OrderByDescending(x => x.TanggalTerima)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(x => new PengembalianKasetListDto
                    {
                        Id = x.Id,
                        NomorMesin = x.NomorMesin,
                        Lokasi = x.Lokasi,
                        Jumlah = x.Jumlah,
                        TanggalTerima = x.TanggalTerima
                    })
                    .ToListAsync();

                return Ok(new PaginatedResponse<PengembalianKasetListDto>
                {
                    Data = data,
                    TotalCount = total,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling(total / (double)pageSize)
                });
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<object>.Error(ex.Message, "500"));
            }
        }

        // =============================
        // GET LIST
        // =============================
        //[HttpGet]
        //public async Task<IActionResult> GetList()
        //{
        //    var data = await _context.PengembalianKaset
        //        .OrderByDescending(x => x.TanggalTerima)
        //        .Select(x => new
        //        {
        //            x.Id,
        //            x.NomorMesin,
        //            x.Lokasi,
        //            x.Jumlah,
        //            x.TanggalTerima
        //        })
        //        .ToListAsync();

        //    return Ok(data);
        //}

        // =============================
        // GET DETAIL
        // =============================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetail(string id)
        {
            var data = await _context.PengembalianKaset
                .Include(x => x.Details)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (data == null)
                return NotFound("Data tidak ditemukan");

            return Ok(data);
        }

        // =============================
        // CREATE / UPDATE
        // =============================
        [HttpPost]
        public async Task<IActionResult> PostPengembalianKaset(
            [FromBody] PengembalianKasetRequest request
        )
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? "system";

            if (request.Details == null || request.Details.Count == 0)
                return BadRequest("Detail kaset wajib diisi.");

            if (request.Details.Select(x => x.KodeKaset).Distinct().Count()
                != request.Details.Count)
                return BadRequest("Kaset tidak boleh duplikat.");

            await using var trx = await _context.Database.BeginTransactionAsync();

            try
            {
                // =============================
                // 1️⃣ CEK DATA
                // =============================
                var entity = await _context.PengembalianKaset
                    .Include(x => x.Details)
                    .FirstOrDefaultAsync(x => x.Id == request.Id);

                var isEdit = entity != null;

                DateTime timeNow = DateTime.UtcNow;

                // =============================
                // 2️⃣ CREATE HEADER
                // =============================
                if (!isEdit)
                {
                    entity = new PengembalianKaset
                    {
                        Id = string.IsNullOrEmpty(request.Id)
                            ? $"RET-{DateTime.Now:yyyyMMddHHmmss}|{request.NomorMesin}"
                            : request.Id,

                        OrderPengisianId = request.OrderPengisianId,
                        NomorMesin = request.NomorMesin,
                        Lokasi = request.Lokasi,
                        MerekMesin = request.MerekMesin,
                        KDBANK = request.KDBANK,
                        KDCABANG = request.KDCABANG,
                        TanggalTerima = request.TanggalTerima,
                        DiterimaOleh = request.DiterimaOleh,
                        Catatan = request.Catatan,
                        created = username,
                        createdat = timeNow
                    };

                    _context.PengembalianKaset.Add(entity);
                }
                else
                {
                    // =============================
                    // 3️⃣ UPDATE HEADER
                    // =============================
                    entity.TanggalTerima = request.TanggalTerima;
                    entity.DiterimaOleh = request.DiterimaOleh;
                    entity.Catatan = request.Catatan;
                    entity.updated = username;
                    entity.updatedat = timeNow;

                    // =============================
                    // 3️⃣.5 ROLLBACK STOK LAMA
                    // =============================
                    var oldKasetIds = entity.Details
                        .Select(d => d.KodeKaset)
                        .Distinct()
                        .ToList();

                    var oldStocks = await _context.KasetStock
                        .Where(x => oldKasetIds.Contains(x.IdKaset))
                        .ToListAsync();

                    foreach (var ks in oldStocks)
                    {
                        ks.Status = "ON_TRIP";     // dikembalikan ke kondisi sebelum retur
                        ks.LocationType = "WO";
                        ks.LocationId = entity.OrderPengisianId;
                        ks.UpdatedAt = timeNow;
                    }

                    _context.PengembalianKasetDetail.RemoveRange(entity.Details);
                    entity.Details.Clear();
                }

                // =============================
                // 4️⃣ VALIDASI & UPDATE STOK BARU
                // =============================
                var kasetIds = request.Details
                    .Select(d => d.KodeKaset)
                    .Distinct()
                    .ToList();

                var kasetStocks = await _context.KasetStock
                    .Where(x => kasetIds.Contains(x.IdKaset))
                    .ToListAsync();

                if (kasetStocks.Count != kasetIds.Count)
                    return BadRequest("Ada kaset yang tidak ditemukan.");

                var invalid = kasetStocks
                    .FirstOrDefault(x => x.Status != "ON_TRIP" && x.Status != "INSTALLED");

                if (invalid != null)
                    return Conflict($"Kaset {invalid.IdKaset} tidak dalam status ON_TRIP.");

                foreach (var ks in kasetStocks)
                {
                    ks.Status = "EMPTY";                 // ✅ kembali kosong
                    ks.LocationType = "CPC";             // / VAULT
                    ks.LocationId = request.KDCABANG;
                    ks.UpdatedAt = DateTime.UtcNow;
                }

                // =============================
                // 5️⃣ INSERT DETAIL BARU
                // =============================
                foreach (var d in request.Details)
                {
                    entity.Details.Add(new PengembalianKasetDetail
                    {
                        PengembalianId = entity.Id,
                        Kaset = d.Kaset,
                        KodeKaset = d.KodeKaset,
                        NoSeal = d.NoSeal,
                        Denom = d.Denom,
                        Lembar = d.Lembar
                    });
                }

                // =============================
                // 6️⃣ HITUNG TOTAL
                // =============================
                entity.Jumlah = entity.Details.Sum(x => x.Denom * x.Lembar);

                await _context.SaveChangesAsync();
                await trx.CommitAsync();

                return Ok(new
                {
                    message = isEdit
                        ? "Pengembalian kaset berhasil diperbarui"
                        : "Pengembalian kaset berhasil disimpan",
                    entity.Id,
                    entity.Jumlah
                });
            }
            catch (Exception ex)
            {
                await trx.RollbackAsync();
                return StatusCode(500, ex.Message);
            }
        }


        // =============================
        // DELETE SINGLE
        // =============================
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePengembalianKaset(string id)
        {
            await using var trx = await _context.Database.BeginTransactionAsync();

            try
            {
                var entity = await _context.PengembalianKaset
                    .Include(x => x.Details)
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (entity == null)
                    return NotFound("Data pengembalian kaset tidak ditemukan");

                // hapus detail dulu
                _context.PengembalianKasetDetail.RemoveRange(entity.Details);

                // hapus header
                _context.PengembalianKaset.Remove(entity);

                await _context.SaveChangesAsync();
                await trx.CommitAsync();

                return Ok(new
                {
                    message = "Pengembalian kaset berhasil dihapus",
                    id
                });
            }
            catch (Exception ex)
            {
                await trx.RollbackAsync();
                return StatusCode(500, ex.Message);
            }
        }
    }
}
