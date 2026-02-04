using System.ComponentModel.DataAnnotations;

namespace cpcApi.Model.DTO.konfigurasi
{
    public class FormAkunDto
    {
        public string? Id { get; set; }

        [Required(ErrorMessage = "UserName wajib diisi")]
        public string? UserName { get; set; }

        [Required(ErrorMessage = "FullName wajib diisi")]
        public string? FullName { get; set; }

        public string? Email { get; set; }
        public string? Photo { get; set; }
        public bool Active { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Cabang { get; set; }

        [Required(ErrorMessage = "Group harus diisi")]
        public string[]? Group { get; set; } // <- nullable dulu
    }
}
