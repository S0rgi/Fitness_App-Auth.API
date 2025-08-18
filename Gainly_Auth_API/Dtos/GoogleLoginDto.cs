using System.ComponentModel.DataAnnotations;
namespace Gainly_Auth_API.Dtos
{
    public class GoogleLoginDto
    {
        [Required]
        public string GoogleIdToken { get; set; }
    }
    
}



