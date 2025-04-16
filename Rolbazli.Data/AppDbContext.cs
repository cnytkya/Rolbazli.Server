using Microsoft.EntityFrameworkCore;
using Rolbazli.Model.Models;

namespace Rolbazli.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        //dbset
        public DbSet<AppUser> AppUsers { get; set; }
    }
}
