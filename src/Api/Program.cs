using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();

builder.Services.AddDbContext<PruebaConceptoRabbitMQ.Infrastructure.AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    // Create database and tables on first run (demo only)
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<PruebaConceptoRabbitMQ.Infrastructure.AppDbContext>();
    db.Database.EnsureCreated();
}

app.UseHttpsRedirection();

app.MapPost("/webhook-stp", async (PruebaConceptoRabbitMQ.Domain.TransferenciaStp dto, PruebaConceptoRabbitMQ.Infrastructure.AppDbContext db) =>
{
    var entity = new PruebaConceptoRabbitMQ.Domain.TransferenciaStp
    {
        Monto = dto.Monto,
        ClaveRastreo = dto.ClaveRastreo,
        BancoEmisor = dto.BancoEmisor,
        BancoReceptor = dto.BancoReceptor,
        CunetaBeneficiar = dto.CunetaBeneficiar
    };

    db.TransferenciasStp.Add(entity);
    await db.SaveChangesAsync();

    return Results.Created($"/webhook-stp/{entity.Id}", entity);
})
.WithName("CreateTransferenciaStp");

app.Run();
