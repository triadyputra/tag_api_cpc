using System.ComponentModel.DataAnnotations;

namespace cpcApi.Model.Cpc
{
    public class MutasiVault
    {
        [Key]
        public long Id { get; set; }

        [Required]
        [StringLength(10)]
        public string KdCabang { get; set; } = default!;

        [Required]
        [StringLength(10)]
        public string KdBank { get; set; } = default!;  // 🔥 TAMBAHAN

        [Required]
        public int Nominal { get; set; }

        // POSITIVE = masuk
        // NEGATIVE = keluar
        public long QtyLembar { get; set; }

        [Required]
        [StringLength(30)]
        public string TipeMutasi { get; set; } = default!;

        [StringLength(100)]
        public string? ReferenceNo { get; set; }

        public long SaldoSetelah { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
