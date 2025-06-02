using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Rolbazli.API.DTOs;
using Rolbazli.Model.Models;

namespace Rolbazli.API.Controllers
{
    [Authorize] //Authorize=> Bu metoda/metotlara erişmeden önce kimlik doğrulama yapılmış mı kontrol et. Bazı metotları bundan muaf tutmak için [AllowAnonymous] attribut'ünü kullanırız. Ör: login metoduna [AllowAnonymous] attribut'ünü koyabiliriz. Eğer bu attribute u koymazsa kullanıcı giriş yapamayacak.
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _config;

        public AccountController(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration config)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _config = config;
        }
        [AllowAnonymous]//Bu attribute kullanıcı giriş yapmazsa bile yeni kayıt oluşturabilsin diye var.
        [HttpPost("register")]
        // POST isteği: api/account/register
        public async Task<ActionResult<string>> Register(RegisterDTO registerDTO)
        {
            // Gelen modelin geçerli olup olmadığını kontrol eder.
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);// Geçersizse 400 hatası döner.
            }

            var user = new AppUser
            {
                Email = registerDTO.Email,
                Fullname = registerDTO.Fullname,
                UserName = registerDTO.Email// Genelde kullanıcı adı olarak e-posta atanır.
            };
            // UserManager yardımıyla kullanıcı oluşturulur ve şifre atanır.
            var result = await _userManager.CreateAsync(user, registerDTO.Password);
            // Eğer kullanıcı oluşturulamazsa, hatalar döndürülür.
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }
            // Eğer herhangi bir rol belirtilmemişse, varsayılan olarak "User" rolü atanır.
            if (registerDTO.Roles is null)
            {
                await _userManager.AddToRoleAsync(user, "User");
            }
            else
            {
                // Eğer roller varsa, her biri kullanıcıya atanabilir. Kullanıcı istediğini seçebilecek.
                foreach (var role in registerDTO.Roles)
                {
                    await _userManager.AddToRoleAsync(user, role);
                }
            }
            // Başarılı şekilde kullanıcı oluşturulduğunda cevap olarak bu DTO döner.
            return Ok(new AuthResponseDTO
            {
                IsSuccess = true,
                Message = "Hesabınız başarıyla oluşturuldu."
            });
        }

        [AllowAnonymous]
        [HttpPost("login")]
        // api/account/login
        public async Task<ActionResult<AuthResponseDTO>> Login(LoginDTO loginDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByEmailAsync(loginDTO.Email);
            //eğer kullanıcı bulunamadıysa
            if (user is null)
            {
                return Unauthorized(new AuthResponseDTO
                {
                    IsSuccess = false,
                    Message = "Bu e-posta ile kayıt bulunamadı!"
                });
            }

            var result = await _userManager.CheckPasswordAsync(user, loginDTO.Password);

            if (!result)
            {
                return Unauthorized(new AuthResponseDTO
                {
                    IsSuccess = false,
                    Message = "Şifre hatalı!"
                });
            }
            var token = GenerateToken(user);
            return Ok(new AuthResponseDTO
            {
                Token = token,
                IsSuccess = true,
                Message = "Giriş başarılı."
            });
        }

        //GenerateToken: token üretme
        private string GenerateToken(AppUser user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_config.GetSection("JwtSetting").GetSection("securityKey").Value!);
            var roles = _userManager.GetRolesAsync(user).Result;

            List<Claim> claims =
            [
                new Claim(JwtRegisteredClaimNames.Email, user.Email!),
                new Claim(JwtRegisteredClaimNames.Name, user.Fullname!),
                        new Claim(JwtRegisteredClaimNames.NameId, user.Id.ToString()!),
                new Claim(JwtRegisteredClaimNames.Aud, _config.GetSection("JwtSetting").GetSection        ("validAudience").Value!),
                new Claim(JwtRegisteredClaimNames.Iss, _config.GetSection("JwtSetting").GetSection        ("validIssuer").Value!)
            ];

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        //[Authorize]
        [HttpGet("user-detail")]//api/account/user-detail
        public async Task<ActionResult<UserDetailDTO>> GetUserDetail()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(currentUserId);

            if (user == null)
            {
                return NotFound(new AuthResponseDTO
                {
                    IsSuccess = false,
                    Message = "User not found"
                });
            }

            return Ok(new UserDetailDTO
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.Fullname,
                Roles = [.. await _userManager.GetRolesAsync(user)],
                PhoneNumber = user.PhoneNumber,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                AccessFailedCount = user.AccessFailedCount
            });
        }
        //[Authorize]
        [HttpGet("get-users")]
        public async Task<ActionResult<IEnumerable<UserDetailDTO>>> GetAllUsers()
        {
            // 1. Adım: Önce tüm kullanıcıları çek
            var users = await _userManager.Users.ToListAsync();

            // 2. Adım: Kullanıcı DTO'larını oluştur
            var userDtos = new List<UserDetailDTO>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                userDtos.Add(new UserDetailDTO
                {
                    Id = user.Id,
                    Email = user.Email,
                    FullName = user.Fullname,
                    Roles = roles.ToArray()
                });
            }

            return Ok(userDtos);
        }

    }

}
