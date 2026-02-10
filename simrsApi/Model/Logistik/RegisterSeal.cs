using System.ComponentModel.DataAnnotations;

namespace cpcApi.Model.Logistik
{
    public class RegisterSeal
    {
        [Key]
        [StringLength(50)]
        public string? NomorSeal { get; set; }
        public bool Active { get; set; }

        // 🔥 TAMBAHAN
        [StringLength(20)]
        public string Status { get; set; } = SealStatus.AVAILABLE;

        [StringLength(50)]
        public string? ReservedBy { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
