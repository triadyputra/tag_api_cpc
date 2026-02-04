using cpcApi.Model.ENUM;
using System.ComponentModel.DataAnnotations;

namespace cpcApi.Model.Cpc
{
    public class ProsesPersiapanUangCpc
    {
        public Guid Id { get; set; }

        // ======================
        // INFORMASI BANK & JADWAL
        // ======================
        public string KodeBank { get; set; } = default!;
        public DateOnly TanggalProses { get; set; }
        public TimeOnly JamMulai { get; set; }

        public JenisProsesCpc JenisProses { get; set; } // Rekonsiliasi / CIT

        // ======================
        // INFORMASI DVR & PETUGAS
        // ======================
        public string NomorDvr { get; set; } = default!;
        public string Meja { get; set; } = default!;
        public TimeOnly JamSelesai { get; set; }

        public string NamaPetugas { get; set; } = default!;
        public string JabatanPetugas { get; set; } = default!;
        public string? PathTtdPetugas { get; set; }

        // ======================
        // STATUS PROSES
        // ======================
        public StatusProsesCpc Status { get; set; } = StatusProsesCpc.Draft;

        public DateTime TanggalDibuat { get; set; } = DateTime.UtcNow;
        public DateTime? TanggalFinal { get; set; }

        [StringLength(10)]
        public string? KdCabang { get; set; }

        // ======================
        // RELASI
        // ======================
        public ICollection<ProsesSetPersiapanUangCpc> DaftarSet { get; set; }
            = new List<ProsesSetPersiapanUangCpc>();
    }
}
