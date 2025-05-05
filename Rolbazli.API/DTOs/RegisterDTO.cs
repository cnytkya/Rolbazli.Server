using System.ComponentModel.DataAnnotations;

namespace Rolbazli.API.DTOs
{
    // Kullanıcının kayıt (register) sırasında göndereceği verileri tutan DTO (Data Transfer Object) sınıfı
    public class RegisterDTO
    {
        // [Required]: Bu alanın boş geçilemeyeceğini belirtir (zorunlu alan)
        // [EmailAddress]: Girilen değerin geçerli bir e-posta formatında olmasını zorunlu kılar
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty; // Email alanı, varsayılan olarak boş string ile başlar.
        public string Fullname { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public List<string>? Roles { get; set; }
    }
}
