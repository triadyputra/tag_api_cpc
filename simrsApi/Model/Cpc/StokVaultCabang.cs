using System.ComponentModel.DataAnnotations;

namespace cpcApi.Model.Cpc
{
    public class StokVaultCabang
    {
        [Key]
        public long Id { get; set; }

        [Required]
        [StringLength(10)]
        public string KdCabang { get; set; } = default!;

        [Required]
        [StringLength(10)]
        public string KdBank { get; set; } = default!;   // 🔥 TAMBAHAN


        [Required]
        public int Nominal { get; set; }

        public long SaldoLembar { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // 🔥 WAJIB UNTUK ANTI RACE CONDITION
        [Timestamp]
        public byte[] RowVersion { get; set; } = default!;
    }
}
