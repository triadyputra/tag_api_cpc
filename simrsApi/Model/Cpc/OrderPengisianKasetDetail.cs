using cpcApi.Model.MasterData;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace cpcApi.Model.Cpc
{
    public class OrderPengisianKasetDetail
    {
        [Key]
        public long Id { get; set; }

        [Required]
        [StringLength(100)]
        public string OrderId { get; set; } = default!;

        [Required]
        public int Kaset { get; set; } // 1–5

        [Required]
        [StringLength(50)]
        public string KodeKaset { get; set; } = default!;
        // ini = IdKaset (FK ke KasetStock)

        [StringLength(50)]
        public string? NoSeal { get; set; }

        public decimal Denom { get; set; }
        public int Lembar { get; set; }

        [NotMapped]
        public decimal Total => Denom * Lembar;

        // ================= NAVIGATION =================
        [ForeignKey(nameof(OrderId))]
        public OrderPengisianKaset Order { get; set; } = default!;

        [ForeignKey(nameof(KodeKaset))]
        public KasetStock KasetStock { get; set; } = default!;
    }
}
