using System.ComponentModel.DataAnnotations;
namespace Gainly_Auth_API.Dtos
{
    public class TgLoginDto
    {
        [Required]
        public string TGLogin { get; set; }
    }
    
}
