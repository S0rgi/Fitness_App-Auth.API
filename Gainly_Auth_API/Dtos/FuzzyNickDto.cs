using System.ComponentModel.DataAnnotations;
namespace Gainly_Auth_API.Dtos
{

    public class FuzzynickRequest
    {
        [Required]
        public string nickname { get; set; }
    }
    public class FuzzynickResponse
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public DateTime RegistrationDate { get; set; }
        
    }

}



