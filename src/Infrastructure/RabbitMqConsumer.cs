using System.Text;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace PruebaConceptoRabbitMQ.Infrastructure;

public sealed class RabbitMqConsumer : Microsoft.Extensions.Hosting.BackgroundService
{
    private readonly IQueueHandler _handler;
    private readonly IConfiguration _configuration;
    private IConnection? _connection;
    private IChannel? _channel;

    public RabbitMqConsumer(IQueueHandler handler, IConfiguration configuration)
    {
        _handler = handler;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var rabbitConfig = _configuration.GetSection("RabbitMQ");

        var factory = new ConnectionFactory
        {
            HostName = rabbitConfig["HostName"] ?? "localhost",
            Port = int.Parse(rabbitConfig["Port"] ?? "5672"),
            UserName = rabbitConfig["UserName"] ?? "guest",
            Password = rabbitConfig["Password"] ?? "guest"
        };

        var queueName = _handler.QueueName;

        //https://www.rabbitmq.com/tutorials/tutorial-three-dotnet
        _connection = await factory.CreateConnectionAsync(cancellationToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await _channel.QueueDeclareAsync(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);

                await _handler.HandleAsync(json, cancellationToken);

                await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken: cancellationToken);
            }
            catch
            {
                await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true, cancellationToken: cancellationToken);
            }
        };

        await _channel.BasicConsumeAsync(
            queue: queueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: cancellationToken);

        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(1000, cancellationToken);
        }
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }
}
