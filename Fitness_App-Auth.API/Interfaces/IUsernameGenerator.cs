namespace Fitness_App_Auth.API.Interfaces
{
    public interface IUsernameGenerator
    {
        Task<string> GenerateAsync(string email);
    }

}