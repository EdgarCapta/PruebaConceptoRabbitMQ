using Microsoft.EntityFrameworkCore;
using PruebaConceptoRabbitMQ.Domain;
using PruebaConceptoRabbitMQ.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSingleton<RabbitMqPublisher>();

builder.Services.AddSingleton<IQueueHandler, TransferenciaStpHandler>();
builder.Services.AddHostedService(sp =>
    new RabbitMqConsumer(
        sp.GetRequiredService<TransferenciaStpHandler>(),
        sp.GetRequiredService<IConfiguration>()));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.UseHttpsRedirection();

app.MapPost("/webhook-stp", async (TransferenciaStp dto, RabbitMqPublisher publisher) =>
{
    await publisher.PublishAsync(dto);

    var response = new { queued = true };
    return Results.Accepted($"/webhook-stp", response);
});

app.Run();
