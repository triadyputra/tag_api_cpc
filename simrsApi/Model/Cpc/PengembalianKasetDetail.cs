using cpcApi.Model.MasterData;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace cpcApi.Model.Cpc
{
    public class PengembalianKasetDetail
    {
        [Key]
        public long Id { get; set; }

        [Required]
        [StringLength(100)]
        public string PengembalianId { get; set; } = default!;

        [Required]
        public int Kaset { get; set; } // 1–5

        [Required]
        [StringLength(50)]
        public string KodeKaset { get; set; } = default!;
        // FK ke KasetStock

        [StringLength(50)]
        public string? NoSeal { get; set; } // seal saat dikembalikan

        public decimal Denom { get; set; }
        public int Lembar { get; set; }

        [NotMapped]
        public decimal Total => Denom * Lembar;

        // ================= NAVIGATION =================
        public PengembalianKaset Pengembalian { get; set; } = default!;
        public KasetStock KasetStock { get; set; } = default!;
    }
}
