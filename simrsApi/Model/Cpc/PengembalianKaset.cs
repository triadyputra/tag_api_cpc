using System.ComponentModel.DataAnnotations;

namespace cpcApi.Model.Cpc
{
    public class PengembalianKaset:BaseModel
    {
        [Key]
        [StringLength(100)]
        public string Id { get; set; } = default!;
        // contoh: RET-01RPL01260155|BCA-Z7D5

        [Required]
        [StringLength(100)]
        public string OrderPengisianId { get; set; } = default!;
        // referensi ke order pengisian (opsional tapi recommended)

        [Required]
        [StringLength(50)]
        public string NomorMesin { get; set; } = default!;

        [Required]
        [StringLength(200)]
        public string Lokasi { get; set; } = default!;

        [Required]
        [StringLength(50)]
        public string MerekMesin { get; set; } = default!;

        [StringLength(10)]
        public string KDBANK { get; set; } = default!;

        [StringLength(10)]
        public string KDCABANG { get; set; } = default!;

        public decimal Jumlah { get; set; } // total uang dikembalikan

        // info tambahan penerimaan
        public DateTime TanggalTerima { get; set; } = DateTime.Now;

        [StringLength(100)]
        public string? DiterimaOleh { get; set; }

        [StringLength(200)]
        public string? Catatan { get; set; }

        // ================= NAVIGATION =================
        public ICollection<PengembalianKasetDetail> Details { get; set; }
            = new List<PengembalianKasetDetail>();
    }
}
