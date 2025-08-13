using System.ComponentModel.DataAnnotations;
namespace Gainly_Auth_API.Dtos
{
        public class RegisterDto
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [StringLength(100, MinimumLength = 6)]
            public string Password { get; set; }
        }
    
}



