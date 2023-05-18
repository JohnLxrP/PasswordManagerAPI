using Microsoft.AspNetCore.Identity;

namespace PasswordManagerAPI.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string  FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? DOB { get; set; }
    }
}
