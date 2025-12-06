using System;

namespace ShippingRates.Services.Usps;

internal class UspsResponseException : Exception
{
    public UspsResponseException() { }

    public UspsResponseException(string message)
        : base(message) { }

    public UspsResponseException(string message, Exception innerException)
        : base(message, innerException) { }
}
