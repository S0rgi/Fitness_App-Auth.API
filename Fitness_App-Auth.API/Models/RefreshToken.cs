
namespace Fitness_App_Auth.API.Models
{public class RefreshToken
{
    public int Id { get; set; }
    public string Token { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; } = false;
    public string UserId { get; set; } = null!; // Или Guid, если ты так используешь
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
}