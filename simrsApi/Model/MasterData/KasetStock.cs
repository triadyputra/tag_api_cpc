using cpcApi.Model.Cpc;
using System.ComponentModel.DataAnnotations;

namespace cpcApi.Model.MasterData
{
    public class KasetStock
    {
        [Key]
        [StringLength(50)]
        public string KdKaset { get; set; } = default!;

        // EMPTY / LOADED / ON_TRIP / INSTALLED / DAMAGED / RESERVED
        [Required]
        [StringLength(20)]
        public string Status { get; set; } = default!;

        // 🔥 PENTING
        public string? ReservedBy { get; set; } // DraftId / OrderId / WOId

        // VAULT / WO / ATM / REPAIR
        [Required]
        [StringLength(20)]
        public string LocationType { get; set; } = default!;

        // NoWO / WSID / dll
        [StringLength(50)]
        public string? LocationId { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // ================= NAVIGATION =================
        public MasterKaset Kaset { get; set; } = default!;

        // 🔥 REKOMENDASI TAMBAHAN
        public ICollection<OrderPengisianKasetDetail> OrderDetails { get; set; }
            = new List<OrderPengisianKasetDetail>();
    }
}
