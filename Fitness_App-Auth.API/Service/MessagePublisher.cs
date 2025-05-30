using RabbitMQ.Client;
using System.Text;
namespace Fitness_App_Auth.API.Service;
public class MessagePublisher
{
    private readonly string _connectionString;

    public MessagePublisher(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task PublishAsync(string message, string queue = "notifications")
    {
        var factory = new ConnectionFactory
        {
            Uri = new Uri(_connectionString)
        };

        await using var connection = await factory.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(
            queue: "notifications",
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

        var body = Encoding.UTF8.GetBytes(message);

        await channel.BasicPublishAsync(exchange: string.Empty, routingKey:queue, body: body);
    }



}
