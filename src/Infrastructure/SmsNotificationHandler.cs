using System.Text.Json;

namespace PruebaConceptoRabbitMQ.Infrastructure;

public sealed class SmsNotificationHandler : IQueueHandler
{
    public string QueueName => "sms-notifications";

    public Task HandleAsync(string jsonMessage, CancellationToken cancellationToken)
    {
        var sms = JsonSerializer.Deserialize<SmsMessage>(jsonMessage);

        Console.WriteLine($"[SMS] Enviando sms a {sms?.PhoneNumber}: {sms?.Message}");

        return Task.CompletedTask;
    }
}

public sealed record SmsMessage(string PhoneNumber, string Message);
