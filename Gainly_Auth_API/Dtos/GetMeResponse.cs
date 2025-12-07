namespace Gainly_Auth_API.Dtos
{

    public class GetMeResponse
    {
        public required string nickname { get; set; }

        public string? Email {get;set;}

        public string? TGUsername {get;set;}

        public DateTime RegistrationDate { get; set; }
    }

}



