using cpcApi.Data;
using cpcApi.Model;
using cpcApi.Model.DTO;
using cpcApi.Model.DTO.konfigurasi;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace cpcApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<ApplicationRole> roleManager;
        private readonly ApplicationDbContext _context;


        public ProfileController(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager,
            ApplicationDbContext context)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            _context = context;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            var user = await userManager.FindByNameAsync(id);

            if (user == null) {
                return BadRequest(ApiResponse<object>.Error("Tidak diberi akses", "404"));
            }
            var userRoles = await userManager.GetRolesAsync(user);

            var result = new
            {
                FullName = user.FullName,
                UserName = user.UserName,
                Photo = user.Photo,
                Email = user.Email,
                Group = string.Join(", ", userRoles), // gabungkan roles menjadi string
                Status = user.Active == true ? "Aktif" : "Non Aktif",
                PhoneNumber = user.PhoneNumber
            };

            return Ok(ApiResponse<object>.Success(result));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutProfile(string id, FormUdateUserDto akun)
        {
            string username = User.FindFirst(ClaimTypes.Name)?.Value ?? "anonymous";

            var user = await userManager.FindByNameAsync(id);
            if (user == null)
            {
                return Ok(ApiResponse<object>.Error("User not found", "201"));
            }

            user.FullName = akun.FullName;
            user.PhoneNumber = akun.PhoneNumber;
            user.Email = akun.Email;
            user.Photo = akun.Photo;

            var result = await userManager.UpdateAsync(user);



            if (result.Succeeded)
            {
                return Ok(ApiResponse<object>.SuccessNoData("Update data berhasil"));
            }
            else
            {
                return Ok(ApiResponse<object>.Error("Error update data", "404"));
            }
        }

        [HttpPost("change-password/{id}")]
        public async Task<IActionResult> ChangePassword(string id, [FromBody] FormGantiPassword request)
        {
            var user = await userManager.FindByNameAsync(id);
            if (user == null)
            {
                return Ok(ApiResponse<object>.Error("User tidak ditemukan", "404"));
            }

            var checkPassword = await userManager.CheckPasswordAsync(user, request.CurrentPassword);
            if (!checkPassword)
            {
                return Ok(ApiResponse<object>.Error("Password lama tidak sesuai", "404"));
            }

            if (request.NewPassword != request.ConfimrNewPassword)
                return Ok(ApiResponse<object>.Error("Konfirmasi password baru tidak cocok", "404"));

            var result = await userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return Ok(ApiResponse<object>.Error("Gagal mengubah password", "404", errors));
            }

            return Ok(ApiResponse<object>.SuccessNoData("Password berhasil diubah"));
        }
    }
}
