using Microsoft.AspNetCore.Identity;

namespace Rolbazli.Model.Models
{
    public class AppUser : IdentityUser
    {
        //public int Id { get; set; }
        public string FullName { get; set; }
    }
}
