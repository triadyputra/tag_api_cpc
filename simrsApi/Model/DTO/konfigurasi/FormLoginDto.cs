using System.ComponentModel.DataAnnotations;

namespace cpcApi.Model.DTO.konfigurasi
{
    public class FormLoginDto
    {
        [Required(ErrorMessage = "Username Harus diisi")]
        public string username { get; set; }

        [Required(ErrorMessage = "Password Harus diisi")]
        public string password { get; set; }
    }
}
