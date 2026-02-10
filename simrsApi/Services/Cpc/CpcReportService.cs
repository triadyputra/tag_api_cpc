using cpcApi.Data;
using cpcApi.Model.Cpc;
using cpcApi.Model.DTO.Cpc.Report;
using cpcApi.Model.ENUM;
using Microsoft.EntityFrameworkCore;

namespace cpcApi.Services.Cpc
{
    public class CpcReportService
    {
        private readonly ApplicationDbContext _context;

        public CpcReportService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CpcReportHeaderDto> GetReportAsync(Guid prosesId)
        {
            var header = await _context.ProsesPersiapanUangCpc
                .AsNoTracking()
                .FirstAsync(x => x.Id == prosesId);

            var sets = await _context.ProsesSetPersiapanUangCpc
                .Where(x => x.ProsesPersiapanUangCpcId == prosesId)
                .OrderBy(x => x.SetKe)
                .Select(s => new CpcReportSetDto
                {
                    SetKe = s.SetKe,
                    Kotak = _context.ProsesKotakUangCpc
                        .Where(k => k.ProsesSetPersiapanUangCpcId == s.Id)
                        .OrderBy(k => k.UrutanKolom)
                        .Select(k => new CpcReportKotakDto
                        {
                            UrutanKolom = k.UrutanKolom,
                            NomorKotakUang = k.NomorKotakUang,
                            NomorSeal = k.NomorSeal,
                            JumlahLembar = k.JumlahLembar,
                            JenisUang = k.JenisUang
                        }).ToList()
                }).ToListAsync();

            // 🔥 WAJIB: PASTIKAN 12 KOLOM
            foreach (var set in sets)
            {
                for (int i = 1; i <= 10; i++)
                {
                    if (!set.Kotak.Any(x => x.UrutanKolom == i))
                    {
                        set.Kotak.Add(new CpcReportKotakDto
                        {
                            UrutanKolom = i
                        });
                    }
                }
                set.Kotak = set.Kotak.OrderBy(x => x.UrutanKolom).ToList();
            }

            return new CpcReportHeaderDto
            {
                KodeBank = header.KodeBank,
                TanggalProses = header.TanggalProses,
                NomorDvr = header.NomorDvr,
                Meja = header.Meja,
                JamMulai = header.JamMulai,
                JamSelesai = header.JamSelesai,
                NamaPetugas = header.NamaPetugas,
                JabatanPetugas = header.JabatanPetugas,
                PathTtdPetugas = header.PathTtdPetugas,
                Sets = sets
            };
        }

        public async Task<List<CpcReportRowDto>> GetReportNewAsync(Guid prosesId)
        {
            const int MIN_SET = 3;

            var header = await _context.ProsesPersiapanUangCpc
                .AsNoTracking()
                .FirstAsync(x => x.Id == prosesId);

            var sets = await _context.ProsesSetPersiapanUangCpc
                .Where(x => x.ProsesPersiapanUangCpcId == prosesId)
                .OrderBy(x => x.SetKe)
                .ToListAsync();

            int totalSet = Math.Max(sets.Count, MIN_SET);

            var result = new List<CpcReportRowDto>();

            for (int index = 0; index < totalSet; index++)
            {
                var set = index < sets.Count ? sets[index] : null;

                var kotak = set == null
                    ? new List<ProsesKotakUangCpc>()
                    : await _context.ProsesKotakUangCpc
                        .Where(k => k.ProsesSetPersiapanUangCpcId == set.Id)
                        .OrderBy(k => k.UrutanKolom)
                        .ToListAsync();

                var map = kotak.ToDictionary(x => x.UrutanKolom, x => x);

                CpcReportRowDto row = new()
                {
                    KodeBank = header.KodeBank,
                    TanggalProses = header.TanggalProses,
                    NomorDvr = header.NomorDvr,
                    Meja = header.Meja,
                    JamMulai = header.JamMulai,
                    JamSelesai = header.JamSelesai,
                    NamaPetugas = header.NamaPetugas,
                    JabatanPetugas = header.JabatanPetugas,
                    JenisProses = header.JenisProses == JenisProsesCpc.Cit
                        ? "CIT"
                        : "Rekonsiliasi",
                    SetKe = index + 1   // 🔥 SET KE DEFAULT
                };

                for (int i = 1; i <= 10; i++)
                {
                    map.TryGetValue(i, out var k);

                    typeof(CpcReportRowDto)
                        .GetProperty($"Kotak{i}")?.SetValue(row, k?.NomorKotakUang);

                    typeof(CpcReportRowDto)
                        .GetProperty($"Seal{i}")?.SetValue(row, k?.NomorSeal);

                    typeof(CpcReportRowDto)
                        .GetProperty($"Lembar{i}")?.SetValue(row, k?.JumlahLembar);

                    typeof(CpcReportRowDto)
                        .GetProperty($"Jenis{i}")?.SetValue(row, k?.JenisUang);
                }

                result.Add(row);
            }

            return result;
        }

    }
}
