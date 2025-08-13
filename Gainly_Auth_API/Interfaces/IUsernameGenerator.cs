namespace Gainly_Auth_API.Interfaces
{
    public interface IUsernameGenerator
    {
        Task<string> GenerateAsync(string email);
    }

}


