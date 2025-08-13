namespace Gainly_Auth_API.Interfaces
{
    public interface INotificationPublisher
    {
        public Task PublishAsync(string message, string queue = "notifications");
    }
 }


