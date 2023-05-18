namespace PasswordManagerAPI.Model
{
    public class Passwordmngr
    {
        public int UserId { get; set; }
        public int Id { get; set; }
        public string Accountr { get; set; } = default!;
        public string Passwordr { get; set; }

        public string Description { get; set; }
    }
}
