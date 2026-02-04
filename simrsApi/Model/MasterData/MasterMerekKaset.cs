using System.ComponentModel.DataAnnotations;

namespace cpcApi.Model.MasterData
{
    public class MasterMerekKaset : BaseModel
    {
        [Key]
        [StringLength(10)]
        public string KdMerek { get; set; } = default!;

        [Required]
        [StringLength(100)]
        public string NmMerek { get; set; } = default!;

        [StringLength(255)]
        public string? Keterangan { get; set; }

        public bool Aktif { get; set; } = true;

        // ================= NAVIGATION =================
        public ICollection<MasterKaset> Kasets { get; set; }
            = new List<MasterKaset>();
    }
}
