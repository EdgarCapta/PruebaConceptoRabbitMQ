using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using PruebaConceptoRabbitMQ.Domain;

namespace PruebaConceptoRabbitMQ.Infrastructure;

public sealed class TransferenciaStpHandler : IQueueHandler
{
    public string QueueName => "transferencias-stp";

    private readonly IServiceScopeFactory _scopeFactory;

    public TransferenciaStpHandler(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task HandleAsync(string jsonMessage, CancellationToken cancellationToken)
    {
        var transferencia = JsonSerializer.Deserialize<TransferenciaStp>(jsonMessage);
        if (transferencia is null) return;

        Console.WriteLine($"[TRANSFERENCIAS-STP] Corriendo handlers");

        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var entity = new TransferenciaStp
        {
            Monto = transferencia.Monto,
            ClaveRastreo = transferencia.ClaveRastreo,
            BancoEmisor = transferencia.BancoEmisor,
            BancoReceptor = transferencia.BancoReceptor,
            CunetaBeneficiar = transferencia.CunetaBeneficiar
        };

        db.TransferenciasStp.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
    }
}
