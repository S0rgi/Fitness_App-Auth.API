namespace Fitness_App_Auth.API.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Username { get; set; }
        public DateTime RegistrationDate { get; set; }
        public List<Friendship> Friendships { get; set; }
    }

    public class Friendship
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; }
        public Guid FriendId { get; set; }
        public User Friend { get; set; }
        public FriendshipStatus Status { get; set; } = FriendshipStatus.Pending;
    }

    public enum FriendshipStatus
    {
        Pending,
        Accepted,
        Rejected
    }
}
