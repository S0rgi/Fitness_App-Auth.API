namespace Gainly_Auth_API.Models
{
    public class EmailConfirmation
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string Code { get; set; }
        public DateTime Expiration { get; set; }
        public bool IsConfirmed { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}