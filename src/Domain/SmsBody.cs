using System;
using System.Collections.Generic;

namespace PruebaConceptoRabbitMQ.Domain;

public partial class SmsBody
{
    public string PhoneNumber { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
