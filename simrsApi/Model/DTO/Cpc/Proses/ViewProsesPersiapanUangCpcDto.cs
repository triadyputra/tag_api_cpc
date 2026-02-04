using cpcApi.Model.ENUM;

namespace cpcApi.Model.DTO.Cpc.Order
{
    public class ViewProsesPersiapanUangCpcDto
    {
        public Guid Id { get; set; }

        public string KodeBank { get; set; }
        public DateOnly TanggalProses { get; set; }

        public string NomorDvr { get; set; }
        public string Meja { get; set; }

        public string NamaPetugas { get; set; }
        public string JabatanPetugas { get; set; }

        public JenisProsesCpc JenisProses { get; set; }
        public StatusProsesCpc Status { get; set; }

        public int JumlahSet { get; set; }
        public DateTime TanggalDibuat { get; set; }

        public string? KdCabang { get; set; }
    }
}
