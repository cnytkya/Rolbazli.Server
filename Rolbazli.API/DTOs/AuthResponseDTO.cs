namespace Rolbazli.API.DTOs
{
    public class AuthResponseDTO
    {
        public string? Token { get; set; } = string.Empty;
        public bool? IsSuccess { get; set; }
        public string? Message { get; set; }
    }
}
