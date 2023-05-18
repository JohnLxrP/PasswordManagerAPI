using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PasswordManagerAPI.Model;
using PasswordManagerAPI.Models;

namespace PasswordManagerAPI.Data
{
    public class apiDBContext : IdentityDbContext<ApplicationUser>
    {
        public apiDBContext()
        {

        }
        public apiDBContext(DbContextOptions<apiDBContext> options):base(options)
        {
        }
        public DbSet<Passwordmngr> passwordmngrs => Set<Passwordmngr>();
    }
}
