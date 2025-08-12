namespace Fitness_App_Auth.API.Interfaces
{
    public interface INotificationPublisher
    {
        public Task PublishAsync(string message, string queue = "notifications");
    }
 }