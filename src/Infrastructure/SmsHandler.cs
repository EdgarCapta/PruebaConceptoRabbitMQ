using System.Text.Json;
using PruebaConceptoRabbitMQ.Domain;

namespace PruebaConceptoRabbitMQ.Infrastructure;

public sealed class SmsHandler : IQueueHandler
{
    public string QueueName => "sms";

    public Task HandleAsync(string jsonMessage, CancellationToken cancellationToken)
    {
        var sms = JsonSerializer.Deserialize<SmsBody>(jsonMessage);

        Console.WriteLine($"[SMS] Enviando sms a {sms?.PhoneNumber}: {sms?.Message}");

        return Task.CompletedTask;
    }
}
