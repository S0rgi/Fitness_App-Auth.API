using System.ComponentModel.DataAnnotations;
namespace Fitness_App_Auth.API.Dtos
{

    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }

}
