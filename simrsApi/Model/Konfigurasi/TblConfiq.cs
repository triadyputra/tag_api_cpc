using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace cpcApi.Model.Konfigurasi
{
    [Table("TblConfiq")]
    public class TblConfiq : BaseModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [StringLength(50)]
        public string? Id { get; set; }

        [Required]
        public string Parameter { get; set; } = string.Empty;

        [Required]
        public string Nilai { get; set; } = string.Empty;
    }
}
