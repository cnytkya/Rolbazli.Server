namespace Rolbazli.API.DTOs
{
    public class UserDetailDTO
    {
        public string? Id { get; set; }
        public string? FullName { get; set; }
        
        public string? Email { get; set; }
        
        public string[]? Roles { get; set; }
        
        public string? PhoneNumber { get; set; }
        
        public bool TwoFacotorEnabled { get; set; }
        
        public bool PhoneNumberConfirmed { get; set; }
        
        public int AccessFailedCount { get; set; }
    }
}
