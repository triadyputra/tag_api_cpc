using cpcApi.Data;
using cpcApi.Filter;
using cpcApi.Model.Cpc;
using cpcApi.Model.DTO;
using cpcApi.Model.DTO.Cpc.Order;
using cpcApi.Model.DTO.Cpc.Proses;
using cpcApi.Model.ENUM;
using cpcApi.Model.Logistik;
using cpcApi.Report;
using cpcApi.Services.Cpc;
using DevExpress.XtraPrinting.Native;
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
        private readonly CpcReportService _service;

        public ProsesPersiapanUangController(ApplicationDbContext context, CpcReportService service)
        {
            _context = context;
            _service = service;
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
        // GET DETAIL (UNTUK LOAD EDIT / VIEW)
        // =====================================================
        //[ApiKeyAuthorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetail(Guid id)
        {
            var proses = await _context.ProsesPersiapanUangCpc
                .AsNoTracking()
                .Include(x => x.DaftarSet)
                    .ThenInclude(x => x.DaftarKotakUang)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (proses == null)
                return Ok(ApiResponse<object>.Error("Data tidak ditemukan", "404"));

            // ===============================
            // URUTKAN SET & KOTAK
            // ===============================
            proses.DaftarSet = proses.DaftarSet
                .OrderBy(s => s.SetKe)
                .Select(s =>
                {
                    s.DaftarKotakUang = s.DaftarKotakUang
                        .OrderBy(k => k.UrutanKolom)
                        .ToList();
                    return s;
                })
                .ToList();

            // ===============================
            // RESPONSE KHUSUS FORM EDIT
            // ===============================
            var response = new
            {
                proses.Id,
                proses.KodeBank,
                proses.TanggalProses,
                proses.JamMulai,
                proses.JamSelesai,
                proses.JenisProses,
                proses.NomorDvr,
                proses.Meja,
                proses.NamaPetugas,
                proses.JabatanPetugas,
                proses.PathTtdPetugas,
                proses.Status,
                proses.KdCabang,

                DaftarSet = proses.DaftarSet.Select(s => new
                {
                    s.SetKe,
                    DaftarKotakUang = s.DaftarKotakUang.Select(k => new
                    {
                        k.UrutanKolom,
                        NomorKotakUang = k.NomorKotakUang, // 🔥 KdKasetBank
                        k.NomorSeal,
                        k.JumlahLembar,
                        k.JenisUang
                    }).ToList()
                }).ToList()
            };

            return Ok(ApiResponse<object>.Success(response, "OK"));
        }


        // =====================================================
        // SIMPAN DRAFT (CREATE / UPDATE)
        // =====================================================
        [HttpPost("simpan-draft")]
        public async Task<IActionResult> SimpanDraftProsesCpc(
            [FromBody] ProsesPersiapanUangCpcInputDto dto)
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            string? errorMessage = null;
            Guid prosesId = Guid.Empty;

            await strategy.ExecuteAsync(async () =>
            {
                await using var tx = await _context.Database.BeginTransactionAsync();

                try
                {
                    ProsesPersiapanUangCpc proses;
                    List<string> kasetLama = new();

                    // ================= EDIT =================
                    if (dto.Id.HasValue)
                    {
                        proses = await _context.ProsesPersiapanUangCpc
                            .Include(x => x.DaftarSet)
                                .ThenInclude(x => x.DaftarKotakUang)
                            .FirstOrDefaultAsync(x => x.Id == dto.Id.Value);

                        if (proses == null)
                        {
                            errorMessage = "Data tidak ditemukan";
                            await tx.RollbackAsync();
                            return;
                        }

                        if (proses.Status == StatusProsesCpc.Final)
                        {
                            errorMessage = "Data final tidak boleh diubah";
                            await tx.RollbackAsync();
                            return;
                        }

                        kasetLama = ExtractKasetKodeFromEntity(proses);

                        // update header
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
                        proses.KdCabang = dto.KdCabang;
                        proses.Status = StatusProsesCpc.Draft;

                        foreach (var s in proses.DaftarSet)
                            _context.ProsesKotakUangCpc.RemoveRange(s.DaftarKotakUang);

                        _context.ProsesSetPersiapanUangCpc.RemoveRange(proses.DaftarSet);
                        await _context.SaveChangesAsync();
                    }
                    // ================= CREATE =================
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
                            KdCabang = dto.KdCabang
                        };

                        _context.ProsesPersiapanUangCpc.Add(proses);
                        await _context.SaveChangesAsync();
                    }

                    // ================= INSERT SET =================
                    proses.DaftarSet = dto.DaftarSet.Select(s => new ProsesSetPersiapanUangCpc
                    {
                        Id = Guid.NewGuid(),
                        ProsesPersiapanUangCpcId = proses.Id,
                        SetKe = s.SetKe,
                        DaftarKotakUang = s.DaftarKotakUang.Select(k => new ProsesKotakUangCpc
                        {
                            Id = Guid.NewGuid(),
                            UrutanKolom = k.UrutanKolom,
                            NomorKotakUang = k.NomorKotakUang, // KdKasetBank
                            NomorSeal = k.NomorSeal,
                            JumlahLembar = k.JumlahLembar,
                            JenisUang = k.JenisUang,
                            Status = StatusKotakUangCpc.Draft
                        }).ToList()
                    }).ToList();

                    _context.ProsesSetPersiapanUangCpc.AddRange(proses.DaftarSet);
                    await _context.SaveChangesAsync();

                    // ================= VALIDASI SEAL =================
                    var sealError = await ValidateAndReserveSealAsync(
                        proses.Id,
                        ExtractSealNumbersFromDto(dto)
                    );

                    if (sealError != null)
                    {
                        errorMessage = sealError;
                        await tx.RollbackAsync();
                        return;
                    }

                    // ================= VALIDASI & RESERVE KASET =================
                    var kasetKode = ExtractKasetKodeFromDto(dto);

                    var kasetError = await ValidateAndReserveKasetAsync(
                        proses.Id,
                        proses.KodeBank,
                        kasetKode);

                    if (kasetError != null)
                    {
                        errorMessage = kasetError;
                        await tx.RollbackAsync();
                        return;
                    }

                    // ================= RELEASE KASET LAMA =================
                    await ReleaseKasetNotUsedAsync(
                        proses.Id,
                        kasetKode,
                        kasetLama);

                    await _context.SaveChangesAsync();
                    await tx.CommitAsync();

                    prosesId = proses.Id;
                }
                catch
                {
                    await tx.RollbackAsync();
                    throw; // error sistem saja
                }
            });

            if (errorMessage != null)
                return Ok(ApiResponse<object>.Error(errorMessage, "400"));

            return Ok(ApiResponse<object>.Success(prosesId, "Draft berhasil disimpan"));
        }


        // =====================================================
        // FINALISASI
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
                return Ok(ApiResponse<object>.Error("Data tidak ditemukan", "404"));

            if (proses.Status == StatusProsesCpc.Final)
                return Ok(ApiResponse<object>.Error("Proses sudah final", "400"));

            // ===============================
            // 🔑 AMBIL KODE KASET DARI PROSES
            // ===============================
            var kdKasetBanks = ExtractKasetKodeFromEntity(proses);
            var seals = ExtractSealNumbersFromEntity(proses);

            // ===============================
            // 🔥 MAP KdKasetBank → IdKaset
            // ===============================
            var kasetMap = await _context.MasterKaset
                .Where(x => kdKasetBanks.Contains(x.KdKaset!))
                .Select(x => new
                {
                    x.KdKaset,
                    //x.KdKaset
                })
                .ToListAsync();

            if (kasetMap.Count != kdKasetBanks.Count)
                return Ok(ApiResponse<object>.Error(
                    "Sebagian kaset tidak ditemukan",
                    "400"
                ));

            var idKasets = kasetMap.Select(x => x.KdKaset).ToList();

            // ===============================
            // 🔥 VALIDASI STOCK (PAKAI IdKaset)
            // ===============================
            var stocks = await _context.KasetStock
                .Where(x =>
                    idKasets.Contains(x.KdKaset) &&
                    x.Status == KasetStatus.RESERVED &&
                    x.ReservedBy == proses.Id.ToString())
                .ToListAsync();

            if (stocks.Count != idKasets.Count)
                return Ok(ApiResponse<object>.Error(
                    "Kaset tidak valid untuk finalisasi",
                    "400"
                ));

            // ===============================
            // 🔁 UPDATE STATUS KASET
            // ===============================
            foreach (var s in stocks)
            {
                s.Status = KasetStatus.LOADED;
                s.ReservedBy = null;
                s.UpdatedAt = DateTime.UtcNow;
            }

            // ===============================
            // 🔐 FINALISASI SEAL
            // ===============================
            var sealEntities = await _context.RegisterSeal
                .Where(x => seals.Contains(x.NomorSeal!))
                .ToListAsync();

            foreach (var seal in sealEntities)
            {
                seal.Active = false;
                seal.Status = SealStatus.USED;
                seal.ReservedBy = null;
                seal.UpdatedAt = DateTime.UtcNow;
            }


            // ===============================
            // 🔒 FINALISASI KOTAK UANG (INI JAWABANNYA)
            // ===============================
            foreach (var set in proses.DaftarSet)
            {
                foreach (var kotak in set.DaftarKotakUang)
                {
                    kotak.Status = StatusKotakUangCpc.Ready;
                    kotak.UpdatedAt = DateTime.UtcNow;
                }
            }
            // ===============================
            // ✅ FINALISASI PROSES
            // ===============================
            proses.Status = StatusProsesCpc.Final;
            proses.TanggalFinal = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(ApiResponse<object>.SuccessNoData(
                "Proses berhasil difinalisasi"
            ));
        }

        [ApiKeyAuthorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDraftProsesCpc(Guid id)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            string? errorMessage = null;

            await strategy.ExecuteAsync(async () =>
            {
                await using var tx = await _context.Database.BeginTransactionAsync();

                try
                {
                    var proses = await _context.ProsesPersiapanUangCpc
                        .Include(x => x.DaftarSet)
                            .ThenInclude(x => x.DaftarKotakUang)
                        .FirstOrDefaultAsync(x => x.Id == id);

                    if (proses == null)
                    {
                        errorMessage = "Data tidak ditemukan";
                        await tx.RollbackAsync();
                        return;
                    }

                    if (proses.Status == StatusProsesCpc.Final)
                    {
                        errorMessage = "Data final tidak boleh dihapus";
                        await tx.RollbackAsync();
                        return;
                    }

                    // =============================
                    // 🔥 RELEASE KASET
                    // =============================
                    var kasetKode = proses.DaftarSet
                        .SelectMany(s => s.DaftarKotakUang)
                        .Select(k => k.NomorKotakUang!.Trim()) // KdKasetBank
                        .Distinct()
                        .ToList();

                    var stocks = await _context.KasetStock
                        .Include(x => x.Kaset)
                        .Where(x =>
                            x.Status == KasetStatus.RESERVED &&
                            x.ReservedBy == proses.Id.ToString() &&
                            kasetKode.Contains(x.Kaset.KdKaset!)
                        )
                        .ToListAsync();

                    foreach (var stock in stocks)
                    {
                        stock.Status = KasetStatus.EMPTY;
                        stock.ReservedBy = null;
                        stock.UpdatedAt = DateTime.UtcNow;
                    }

                    // =============================
                    // ❌ NOMOR SEAL
                    // =============================
                    var seals = ExtractSealNumbersFromEntity(proses);

                    var sealEntities = await _context.RegisterSeal
                        .Where(x =>
                            seals.Contains(x.NomorSeal) &&
                            x.Status == SealStatus.RESERVED &&
                            x.ReservedBy == proses.Id.ToString())
                        .ToListAsync();

                    foreach (var s in sealEntities)
                    {
                        s.Status = SealStatus.AVAILABLE;
                        s.ReservedBy = null;
                        s.UpdatedAt = DateTime.UtcNow;
                    }

                    _context.ProsesPersiapanUangCpc.Remove(proses);
                    await _context.SaveChangesAsync();

                    await tx.CommitAsync();
                }
                catch
                {
                    await tx.RollbackAsync();
                    throw;
                }
            });

            if (errorMessage != null)
                return Ok(ApiResponse<object>.Error(errorMessage, "400"));

            return Ok(ApiResponse<object>.SuccessNoData("Draft berhasil dihapus"));
        }


        [HttpGet("print/{id}")]
        public async Task<IActionResult> Print(
            Guid id,
            [FromQuery] string? format = "pdf"
        )
        {
            try
            {
                // ===============================
                // AMBIL DATA
                // ===============================
                var data = await _service.GetReportNewAsync(id);

                if (data == null)
                {
                    return Ok(new
                    {
                        response = "",
                        metadata = new
                        {
                            message = "Data tidak ditemukan",
                            code = "201"
                        }
                    });
                }

                var report = new RepProsesCpc();
                report.DataSource = data;

                using var ms = new MemoryStream();

                // ===============================
                // EXPORT FORMAT
                // ===============================
                format = format?.ToLower();

                if (format == "excel" || format == "xlsx")
                {
                    report.ExportToXlsx(ms);
                }
                else
                {
                    report.ExportToPdf(ms);
                    format = "pdf";
                }

                var base64 = Convert.ToBase64String(ms.ToArray());

                return Ok(new
                {
                    response = base64,
                    metadata = new
                    {
                        message = "Berhasil",
                        code = "200",
                        format = format.ToUpper()
                    }
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    response = "",
                    metadata = new
                    {
                        message = ex.InnerException?.Message ?? ex.Message,
                        code = "201"
                    }
                });
            }
        }



        // =====================================================
        // ===================== HELPERS =======================
        // =====================================================
        private async Task<string?> ValidateAndReserveSealAsync(
        Guid prosesId,
        List<string> seals)
        {
            if (!seals.Any()) return null;

            var dbSeals = await _context.RegisterSeal
                .Where(x => seals.Contains(x.NomorSeal))
                .ToListAsync();

            if (dbSeals.Count != seals.Count)
                return "Sebagian nomor seal tidak ditemukan";

            foreach (var seal in dbSeals)
            {
                if (!seal.Active)
                    return $"Seal {seal.NomorSeal} sudah tidak aktif";

                if (seal.Status == SealStatus.AVAILABLE)
                {
                    seal.Status = SealStatus.RESERVED;
                    seal.ReservedBy = prosesId.ToString();
                }
                else if (seal.Status == SealStatus.RESERVED)
                {
                    if (seal.ReservedBy != prosesId.ToString())
                        return $"Seal {seal.NomorSeal} sedang dipakai proses lain";
                }
                else
                {
                    return $"Seal {seal.NomorSeal} sudah digunakan";
                }

                seal.UpdatedAt = DateTime.UtcNow;
            }

            return null;
        }
       
        // 🔥 INI KUNCI UTAMA (KdKasetBank → IdKaset)
        private async Task<string?> ValidateAndReserveKasetAsync(
            Guid prosesId,
            string kdBank,
            List<string> kdKasetBanks)
        {
            if (!kdKasetBanks.Any()) return null;

            var kasets = await _context.MasterKaset
                .Include(x => x.Stock)
                .Where(x => kdKasetBanks.Contains(x.KdKaset!))
                .ToListAsync();

            if (kasets.Count != kdKasetBanks.Count)
                return "Sebagian kaset tidak ditemukan";

            foreach (var kaset in kasets)
            {
                if (kaset.KdBank != kdBank)
                    return $"Kaset {kaset.KdKaset} beda bank";

                var stock = kaset.Stock;
                if (stock == null)
                    return $"Stock kaset {kaset.KdKaset} tidak ditemukan";

                if (stock.Status == KasetStatus.EMPTY)
                {
                    stock.Status = KasetStatus.RESERVED;
                    stock.ReservedBy = prosesId.ToString();
                }
                else if (stock.Status == KasetStatus.RESERVED)
                {
                    if (stock.ReservedBy != prosesId.ToString())
                        return $"Kaset {kaset.KdKaset} sedang dipakai proses lain";
                }
                else
                {
                    return $"Kaset {kaset.KdKaset} tidak tersedia (status {stock.Status})";
                }

                stock.UpdatedAt = DateTime.UtcNow;
            }

            return null;
        }

        private async Task ReleaseKasetNotUsedAsync(
            Guid prosesId,
            List<string> kasetBaru,
            List<string> kasetLama)
        {
            var toRelease = kasetLama
                .Where(x => !kasetBaru.Contains(x))
                .ToList();

            if (!toRelease.Any()) return;

            var stocks = await _context.KasetStock
                .Where(x =>
                    x.Status == KasetStatus.RESERVED &&
                    x.ReservedBy == prosesId.ToString())
                .Include(x => x.Kaset)
                .Where(x => toRelease.Contains(x.Kaset.KdKaset!))
                .ToListAsync();

            foreach (var s in stocks)
            {
                s.Status = KasetStatus.EMPTY;
                s.ReservedBy = null;
                s.UpdatedAt = DateTime.UtcNow;
            }
        }

        private static List<string> ExtractKasetKodeFromDto(ProsesPersiapanUangCpcInputDto dto)
            => dto.DaftarSet
                .SelectMany(s => s.DaftarKotakUang)
                .Select(k => k.NomorKotakUang?.Trim()) // KdKasetBank
                .Where(x => !string.IsNullOrEmpty(x))
                .Distinct()
                .ToList()!;

        private static List<string> ExtractKasetKodeFromEntity(ProsesPersiapanUangCpc proses)
            => proses.DaftarSet
                .SelectMany(s => s.DaftarKotakUang)
                .Select(k => k.NomorKotakUang?.Trim())
                .Where(x => !string.IsNullOrEmpty(x))
                .Distinct()
                .ToList()!;

        private static List<string> ExtractSealNumbersFromDto(ProsesPersiapanUangCpcInputDto dto)
            => dto.DaftarSet
                .SelectMany(s => s.DaftarKotakUang)
                .Select(k => k.NomorSeal?.Trim())
                .Where(x => !string.IsNullOrEmpty(x))
                .Distinct()
                .ToList()!;

        private static List<string> ExtractSealNumbersFromEntity(ProsesPersiapanUangCpc p)
        => p.DaftarSet.SelectMany(s => s.DaftarKotakUang)
            .Select(k => (k.NomorSeal ?? "").Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct()
            .ToList();

    }
}
