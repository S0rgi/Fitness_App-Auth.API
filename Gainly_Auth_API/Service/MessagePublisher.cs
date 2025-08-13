using Gainly_Auth_API.Interfaces;
using RabbitMQ.Client;
using System.Text;
namespace Gainly_Auth_API.Service;
public class MessagePublisher:INotificationPublisher
{
    private readonly string _connectionString;
    private readonly string _pingUrl;
    private readonly HttpClient _httpClient = new();
    public MessagePublisher(string connectionString, string pingUrl)
    {
        _connectionString = connectionString;
        _pingUrl = pingUrl;
    }

    public async Task PublishAsync(string message, string queue = "notifications")
    {
        // Пробуждаем NotificationService
        try
        {
            await _httpClient.GetAsync(_pingUrl);
        }
        catch (Exception ex)
        {
            Console.WriteLine(_pingUrl);
            Console.WriteLine($"[Wake-up] Ошибка при вызове ping: {ex.Message}");
        }

        var factory = new ConnectionFactory
        {
            Uri = new Uri(_connectionString)
        };

        await using var connection = await factory.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(
            queue: queue,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

        var body = Encoding.UTF8.GetBytes(message);
        await channel.BasicPublishAsync(exchange: string.Empty, routingKey: queue, body: body);
    }
}



