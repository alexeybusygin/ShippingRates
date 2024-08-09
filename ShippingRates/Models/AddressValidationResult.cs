using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShippingRates.Models
{
    public class AddressValidationResult
    {
        public bool IsValid { get; set; }
        public AddressDetails AddressDetails { get; set; }
        /// <summary>
        ///     Errors returned by service provider (e.g. 'Wrong postal code')
        /// </summary>
        public List<Error> Errors { get; } = new List<Error>();
        /// <summary>
        ///     Internal errors during interaction with service provider
        /// </summary>
        public List<string> InternalErrors { get; } = new List<string>();
    }
}
