using System.ComponentModel.DataAnnotations;

namespace Gainly_Auth_API.Dtos
{
    public class ChangeUsernameDto
    {
        [Required]
        [RegularExpression(@"^[a-zA-Z0-9_]{3,20}$", 
            ErrorMessage = "Username может содержать только буквы, цифры и подчёркивания (от 3 до 20 символов)")]
        public string NewUsername { get; set; }
    }
}



