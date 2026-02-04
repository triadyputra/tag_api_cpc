using cpcApi.Data;
using cpcApi.Filter;
using cpcApi.Model.Cpc;
using cpcApi.Model.DTO;
using cpcApi.Model.DTO.Cpc.Order;
using cpcApi.Model.DTO.Cpc.Proses;
using cpcApi.Model.ENUM;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace cpcApi.Controllers.Cpc
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProsesPersiapanUangController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProsesPersiapanUangController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // LIST (BANK / TANGGAL / STATUS)
        // =====================================================
        [ApiKeyAuthorize]
        [HttpGet("GetListProsesCpc")]
        public async Task<ActionResult<PaginatedResponse<ViewProsesPersiapanUangCpcDto>>> GetListProsesCpc(
        [FromQuery] string? filter = null,
        [FromQuery] string? bank = null,
        [FromQuery] StatusProsesCpc? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                IQueryable<ProsesPersiapanUangCpc> query = _context.ProsesPersiapanUangCpc
                    .AsNoTracking();

                // ================= FILTER =================

                if (!string.IsNullOrWhiteSpace(filter))
                {
                    var like = $"%{filter}%";
                    query = query.Where(x =>
                        EF.Functions.Like(x.NomorDvr, like) ||
                        EF.Functions.Like(x.NamaPetugas, like) ||
                        EF.Functions.Like(x.Meja, like)
                    );
                }

                if (!string.IsNullOrWhiteSpace(bank))
                {
                    query = query.Where(x => x.KodeBank == bank);
                }

                if (status.HasValue)
                {
                    query = query.Where(x => x.Status == status);
                }

                var count = await query.CountAsync();

                var items = await query
                    .OrderByDescending(x => x.TanggalDibuat)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(x => new ViewProsesPersiapanUangCpcDto
                    {
                        Id = x.Id,
                        KodeBank = x.KodeBank,
                        TanggalProses = x.TanggalProses,

                        NomorDvr = x.NomorDvr,
                        Meja = x.Meja,

                        NamaPetugas = x.NamaPetugas,
                        JabatanPetugas = x.JabatanPetugas,

                        JenisProses = x.JenisProses,
                        Status = x.Status,

                        JumlahSet = x.DaftarSet.Count,
                        TanggalDibuat = x.TanggalDibuat,

                        KdCabang = x.KdCabang
                    })
                    .ToListAsync();

                return Ok(new PaginatedResponse<ViewProsesPersiapanUangCpcDto>
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
        // CREATE / SAVE DRAFT
        // =====================================================
        [ApiKeyAuthorize]
        [HttpPost("simpan-draft")]
        public async Task<IActionResult> SimpanDraftProsesCpc(
            [FromBody] ProsesPersiapanUangCpcInputDto dto)
        {
            ProsesPersiapanUangCpc proses;

            // ==========================
            // MODE EDIT
            // ==========================
            if (dto.Id.HasValue)
            {
                proses = await _context.ProsesPersiapanUangCpc
                .Include(x => x.DaftarSet)
                    .ThenInclude(x => x.DaftarKotakUang)
                .FirstOrDefaultAsync(x => x.Id == dto.Id.Value);

                if (proses == null)
                    return NotFound(new
                    {
                        metadata = new { code = "404", message = "Data tidak ditemukan" }
                    });

                // =====================
                // UPDATE HEADER
                // =====================
                proses.KodeBank = dto.KodeBank;
                proses.TanggalProses = dto.TanggalProses;
                proses.JamMulai = dto.JamMulai;
                proses.JamSelesai = dto.JamSelesai;
                proses.JenisProses = dto.JenisProses;
                proses.NomorDvr = dto.NomorDvr;
                proses.Meja = dto.Meja;
                proses.NamaPetugas = dto.NamaPetugas;
                proses.JabatanPetugas = dto.JabatanPetugas;
                proses.PathTtdPetugas = dto.PathTtdPetugas;
                proses.Status = StatusProsesCpc.Draft;
                proses.KdCabang = dto.KdCabang;

                // =====================
                // HAPUS CHILD LAMA
                // =====================
                foreach (var set in proses.DaftarSet)
                {
                    _context.ProsesKotakUangCpc.RemoveRange(set.DaftarKotakUang);
                }
                _context.ProsesSetPersiapanUangCpc.RemoveRange(proses.DaftarSet);

                await _context.SaveChangesAsync(); // 🔥 WAJIB

                // =====================
                // TAMBAH CHILD BARU
                // =====================
                var newSets = dto.DaftarSet.Select(s => new ProsesSetPersiapanUangCpc
                {
                    Id = Guid.NewGuid(),
                    ProsesPersiapanUangCpcId = proses.Id,
                    SetKe = s.SetKe,
                    DaftarKotakUang = s.DaftarKotakUang
                        .OrderBy(k => k.UrutanKolom)
                        .Select(k => new ProsesKotakUangCpc
                        {
                            Id = Guid.NewGuid(),
                            UrutanKolom = k.UrutanKolom,
                            NomorKotakUang = k.NomorKotakUang,
                            NomorSeal = k.NomorSeal,
                            JumlahLembar = k.JumlahLembar,
                            JenisUang = k.JenisUang
                        }).ToList()
                }).ToList();

                // 🔥 INI KUNCINYA
                _context.ProsesSetPersiapanUangCpc.AddRange(newSets);

                await _context.SaveChangesAsync();
            }
            // ==========================
            // MODE CREATE
            // ==========================
            else
            {
                proses = new ProsesPersiapanUangCpc
                {
                    Id = Guid.NewGuid(),
                    KodeBank = dto.KodeBank,
                    TanggalProses = dto.TanggalProses,
                    JamMulai = dto.JamMulai,
                    JamSelesai = dto.JamSelesai,
                    JenisProses = dto.JenisProses,

                    NomorDvr = dto.NomorDvr,
                    Meja = dto.Meja,
                    NamaPetugas = dto.NamaPetugas,
                    JabatanPetugas = dto.JabatanPetugas,
                    PathTtdPetugas = dto.PathTtdPetugas,

                    Status = StatusProsesCpc.Draft,
                    TanggalDibuat = DateTime.UtcNow,
                    KdCabang = dto.KdCabang,

                    DaftarSet = dto.DaftarSet.Select(s => new ProsesSetPersiapanUangCpc
                    {
                        Id = Guid.NewGuid(),
                        SetKe = s.SetKe,
                        DaftarKotakUang = s.DaftarKotakUang
                            .OrderBy(k => k.UrutanKolom)
                            .Select(k => new ProsesKotakUangCpc
                            {
                                Id = Guid.NewGuid(),
                                UrutanKolom = k.UrutanKolom,
                                NomorKotakUang = k.NomorKotakUang,
                                NomorSeal = k.NomorSeal,
                                JumlahLembar = k.JumlahLembar,
                                JenisUang = k.JenisUang
                            }).ToList()
                    }).ToList()
                };

                _context.ProsesPersiapanUangCpc.Add(proses);
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                metadata = new { code = "200", message = "Draft berhasil disimpan" },
                response = proses.Id
            });
        }

        // =====================================================
        // FINALISASI PROSES
        // =====================================================
        [ApiKeyAuthorize]
        [HttpPost("finalisasi/{id}")]
        public async Task<IActionResult> FinalisasiProsesCpc(Guid id)
        {
            var proses = await _context.ProsesPersiapanUangCpc
                .Include(x => x.DaftarSet)
                    .ThenInclude(x => x.DaftarKotakUang)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (proses == null)
                return NotFound(new
                {
                    metadata = new { code = "404", message = "Data tidak ditemukan" }
                });

            if (proses.Status == StatusProsesCpc.Final)
                return BadRequest(new
                {
                    metadata = new { code = "400", message = "Proses sudah final" }
                });

            // ================= VALIDASI DASAR =================

            if (!proses.DaftarSet.Any())
                return BadRequest(new
                {
                    metadata = new { code = "400", message = "Minimal 1 set harus diisi" }
                });

            if (proses.DaftarSet.Count > 3)
                return BadRequest(new
                {
                    metadata = new { code = "400", message = "Maksimal 3 set" }
                });

            foreach (var set in proses.DaftarSet)
            {
                if (set.DaftarKotakUang.Count > 10)
                    return BadRequest(new
                    {
                        metadata = new { code = "400", message = $"Set ke-{set.SetKe} melebihi 10 kotak" }
                    });
            }

            // ================= FINALISASI =================
            proses.Status = StatusProsesCpc.Final;
            proses.TanggalFinal = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                metadata = new { code = "200", message = "Proses berhasil difinalisasi" }
            });
        }

        // =====================================================
        // GET BY ID (DETAIL)
        // =====================================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var data = await _context.ProsesPersiapanUangCpc
         .Include(x => x.DaftarSet)
             .ThenInclude(x => x.DaftarKotakUang)
         .FirstOrDefaultAsync(x => x.Id == id);

            if (data == null)
                return NotFound(new
                {
                    metadata = new { code = "404", message = "Data tidak ditemukan" }
                });

            // ===============================
            // 🔥 URUTKAN SET & KOTAK UANG
            // ===============================
            data.DaftarSet = data.DaftarSet
                .OrderBy(s => s.SetKe)
                .Select(s =>
                {
                    s.DaftarKotakUang = s.DaftarKotakUang
                        .OrderBy(k => k.UrutanKolom)
                        .ToList();

                    return s;
                })
                .ToList();

            return Ok(new
            {
                metadata = new { code = "200", message = "OK" },
                response = data
            });
        }


        // =====================================================
        // DELETE DRAFT
        // =====================================================
        [ApiKeyAuthorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDraftProsesCpc(Guid id)
        {
            var proses = await _context.ProsesPersiapanUangCpc
                .FirstOrDefaultAsync(x => x.Id == id);

            if (proses == null)
                return NotFound(new
                {
                    metadata = new { code = "404", message = "Data tidak ditemukan" }
                });

            if (proses.Status == StatusProsesCpc.Final)
                return BadRequest(new
                {
                    metadata = new { code = "400", message = "Data final tidak boleh dihapus" }
                });

            _context.ProsesPersiapanUangCpc.Remove(proses);
            await _context.SaveChangesAsync();
            // 🔥 SET & KOTAK IKUT TERHAPUS (CASCADE)

            return Ok(new
            {
                metadata = new { code = "200", message = "Draft berhasil dihapus" }
            });
        }
    }
}
