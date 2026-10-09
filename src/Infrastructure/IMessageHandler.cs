namespace PruebaConceptoRabbitMQ.Infrastructure;

public interface IQueueHandler
{
    string QueueName { get; }
    Task HandleAsync(string jsonMessage, CancellationToken cancellationToken);
}