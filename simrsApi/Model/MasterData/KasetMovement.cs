using System.ComponentModel.DataAnnotations;

namespace cpcApi.Model.MasterData
{
    public class KasetMovement :BaseModel
    {
        [Key]
        public long IdMovement { get; set; }

        [Required]
        [StringLength(50)]
        public string KdKaset { get; set; } = default!;

        // LOAD / DISPATCH / INSTALL / RETURN / REPAIR
        [Required]
        [StringLength(20)]
        public string Action { get; set; } = default!;

        [StringLength(50)]
        public string? FromLocation { get; set; }

        [StringLength(50)]
        public string? ToLocation { get; set; }

        // ================= KUNCI OPERASIONAL =================
        // Nomor Work Order (pengganti TripId)
        [Required]
        [StringLength(50)]
        public string NoWO { get; set; } = default!;
        // =====================================================

        // ATM tujuan / sumber (jika relevan)
        [StringLength(20)]
        public string? Wsid { get; set; }

        // ================= NAVIGATION =================
        public MasterKaset Kaset { get; set; } = default!;
    }
}
