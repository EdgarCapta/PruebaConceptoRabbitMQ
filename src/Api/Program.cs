using Microsoft.EntityFrameworkCore;
using PruebaConceptoRabbitMQ.Domain;
using PruebaConceptoRabbitMQ.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();
builder.Services.AddHostedService<RabbitMqConsumer>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    // Create database and tables on first run (demo only)
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.UseHttpsRedirection();

app.MapPost("/webhook-stp", async (TransferenciaStp dto, IRabbitMqPublisher publisher) =>
{
    // Only publish to queue - the consumer will perform the actual insert
    await publisher.PublishAsync(dto);

    return Results.Accepted($"/webhook-stp", dto);
})
.WithName("CreateTransferenciaStp");

app.Run();
