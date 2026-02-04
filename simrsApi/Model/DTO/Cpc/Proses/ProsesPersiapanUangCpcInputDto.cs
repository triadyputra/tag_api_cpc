using cpcApi.Model.ENUM;

namespace cpcApi.Model.DTO.Cpc.Proses
{
    public class ProsesPersiapanUangCpcInputDto
    {
        public Guid? Id { get; set; }   // ✅ BUKAN string
        public string KodeBank { get; set; } = default!;
        public DateOnly TanggalProses { get; set; }
        public TimeOnly JamMulai { get; set; }
        public TimeOnly JamSelesai { get; set; }
        public JenisProsesCpc JenisProses { get; set; }

        public string NomorDvr { get; set; } = default!;
        public string Meja { get; set; } = default!;
        public string NamaPetugas { get; set; } = default!;
        public string JabatanPetugas { get; set; } = default!;
        public string? PathTtdPetugas { get; set; }
       
        public string? KdCabang { get; set; }

        public List<ProsesSetPersiapanUangCpcInputDto> DaftarSet { get; set; } = [];
    }
}
