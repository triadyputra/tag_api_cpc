using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace cpcApi.Model.MasterData
{
    public class MasterKaset :BaseModel
    {
        //[Key]
        //[StringLength(50)]
        //public string IdKaset { get; set; } = Guid.NewGuid().ToString();

        // kode kaset versi bank (opsional)
        [Key]
        [StringLength(50)]
        public string? KdKaset { get; set; }

        // reference ke master bank (DB lain)
        [Required]
        [StringLength(10)]
        public string KdBank { get; set; } = default!;

        // snapshot nama bank (UNTUK DISPLAY)
        [Required]
        [StringLength(100)]
        public string NmBank { get; set; } = default!;
        // ========================================

        [Required]
        [StringLength(10)]
        public string KdMerek { get; set; } = default!;

        public MasterMerekKaset? Merek { get; set; }

        [Required]
        [StringLength(10)]
        public string Tipe { get; set; } = default!;

        // DISPENSE / REJECT / RETRACT
        [Required]
        [StringLength(10)]
        public string Jenis { get; set; } = default!;

        [Required]
        [StringLength(100)]
        public string NoSerial { get; set; } = default!;

        // GOOD / DAMAGED / REPAIR
        [Required]
        [StringLength(10)]
        public string StatusFisik { get; set; } = default!;

        [Required]
        [StringLength(10)]
        public string KdCabang { get; set; } = default!;

        // ================= NAVIGATION =================
        public KasetStock? Stock { get; set; }
        public ICollection<KasetMovement> Movements { get; set; }
            = new List<KasetMovement>();
    }
}
