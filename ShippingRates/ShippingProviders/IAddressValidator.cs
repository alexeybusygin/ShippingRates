using ShippingRates.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShippingRates.ShippingProviders
{
    internal interface IAddressValidator
    {
        Task<AddressValidationResult> ValidateAddressAsync(Address address);
    }
}
