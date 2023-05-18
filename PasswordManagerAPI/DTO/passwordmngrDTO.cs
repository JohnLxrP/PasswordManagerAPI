using System.ComponentModel.DataAnnotations;

namespace PasswordManagerAPI.DTO
{
    public class passwordmngrDTO
    {
        [Required]
        public string Accountr { get; set; }
        public string? Description { get; set; }
        public string Passwordr { get; set; }

        public passwordmngrDTO()
        {
        }
        public passwordmngrDTO( string accountr, string description, string passwordr)
        {
            Accountr = accountr;
            Passwordr = passwordr;
            Description = description;
            
        }
    }
}
