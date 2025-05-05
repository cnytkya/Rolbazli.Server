using System.ComponentModel.DataAnnotations;

namespace Rolbazli.API.DTOs
{
    public class LoginDTO
    {
        [Required]
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
