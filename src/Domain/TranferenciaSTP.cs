using System;
using System.Collections.Generic;

namespace PruebaConceptoRabbitMQ.Domain;

public partial class TransferenciaStp
{
    public int Id { get; set; }

    public decimal? Monto { get; set; }

    public string? ClaveRastreo { get; set; }

    public string? BancoEmisor { get; set; }

    public string? BancoReceptor { get; set; }

    public string? CunetaBeneficiar { get; set; }
}
