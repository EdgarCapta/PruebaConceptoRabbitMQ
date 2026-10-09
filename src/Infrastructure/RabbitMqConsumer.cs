using System;
using System.Collections.Generic;
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

    // Al constructor le tienes que pasar un handler!!
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

        // Quality of service, 10 tareas a la vez
        await _channel.BasicQosAsync(0, prefetchCount: 10, global: false, cancellationToken: cancellationToken);

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
                //  RABBIT MQ NO TIENE RETRY??
                var retryCount = 0;
                if (ea.BasicProperties?.Headers?.TryGetValue("x-retry-count", out var retryObj) == true && retryObj is not null)
                {
                    retryCount = Convert.ToInt32(retryObj);
                }

                if (retryCount < 3)
                {
                    var newProps = new BasicProperties
                    {
                        Persistent = true,
                        ContentType = "application/json",
                        Headers = new Dictionary<string, object?>
                        {
                            ["x-retry-count"] = retryCount + 1
                        }
                    };

                    await _channel.BasicPublishAsync(
                        exchange: "",
                        routingKey: queueName,
                        mandatory: false,
                        basicProperties: newProps,
                        body: ea.Body,
                        cancellationToken: cancellationToken);

                    await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false, cancellationToken: cancellationToken);
                }
                else
                {
                    await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false, cancellationToken: cancellationToken);
                }
            }
        };

        await _channel.BasicConsumeAsync(
            queue: queueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: cancellationToken);

        // El servicio se queda vivo hasta que llega cancellationtoken
        try
        {
            await Task.Delay(Timeout.Infinite, cancellationToken);
        }
        catch (TaskCanceledException)
        { }
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }
}
