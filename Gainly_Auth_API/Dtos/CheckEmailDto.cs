using System.ComponentModel.DataAnnotations;
namespace Gainly_Auth_API.Dtos
{
    public record CheckEmailDto
    {
        [Required]
        public string email { get; set; }
        [Required]
        public int code {get; set;}
    }
    
}



