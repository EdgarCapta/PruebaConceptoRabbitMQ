## TLDR
docker compose up -d


### Paso 1: Levantar contenedores
docker compose up -d
Los contenedores contienen base de datos SQL local y rabbitMQ local
Esto es solo para la prueba de concepto, en producción de preferencia se debería correr ambos servicios "bare metal" sin contenedores.

## Paso 2: Crear la tabla en base de datos
create database capta;
go;
use capta;
go;
CREATE TABLE dbo.transferenciaStp (
    id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    monto MONEY NULL,
    claveRastreo NVARCHAR(50) NULL,
    bancoEmisor NVARCHAR(100) NULL,
    bancoReceptor NVARCHAR(100) NULL,
    cunetaBeneficiar NVARCHAR(50) NULL
);

## Paso 3: Correr la api
dotnet run --project src/Api

## Paso 4: Enviar una request
curl --request POST \
  --url http://localhost:5008/webhook-stp \
  --header 'Content-Type: application/json' \
  --data '{
  "monto": 1500.75,
  "claveRastreo": "CAPTA-1234",
  "bancoEmisor": "BBVA",
  "bancoReceptor": "Banorte",
  "cunetaBeneficiar": "0123456789"
}'

### Database first introspection, una utilidad que pueds hacer
dotnet ef dbcontext scaffold "Server=localhost,1433;Database=capta;User Id=sa;Password=Captavale123!;TrustServerCertificate=True;" Microsoft.EntityFrameworkCore.SqlServer --project src/Infrastructure/PruebaConceptoRabbitMQ.Infrastructure.csproj --startup-project src/Api/PruebaConceptoRabbitMQ.Api.csproj --output-dir ../Domain/Generated --context AppDbContext --context-dir . --force

Usando dbcontext se puede generar los dto de domain