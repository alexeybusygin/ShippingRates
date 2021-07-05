using System;
using System.Collections.Generic;
using System.Text;

namespace ShippingRates
{
    /// <summary>
    /// Documents only package
    /// </summary>
    public class DocumentsPackage : Package
    {
        public DocumentsPackage(decimal weight, decimal insuredValue, bool signatureRequiredOnDelivery = false)
            : base(0, 0, 0, weight, insuredValue, null, signatureRequiredOnDelivery)
        {
        }
    }
}
