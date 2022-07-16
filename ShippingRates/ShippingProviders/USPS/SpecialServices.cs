using System;
using System.Collections.Generic;
using System.Text;

namespace ShippingRates.ShippingProviders.USPS
{
    public enum SpecialServices
    {
        Insurance = 100,
        InsurancePriorityMailExpress = 101,
        ReturnReceipt = 102,
        CollectOnDelivery = 103,
        CertificateOfMailingForm3665 = 104,
        CertifiedMail = 105,
        USPSTracking = 106,
        SignatureConfirmation = 108,
        RegisteredMail = 109,
        ReturnReceiptElectronic = 110,
        RegisteredMailCODCollectionCharge = 112,
        ReturnReceiptPriorityMailExpress = 118,
        AdultSignatureRequired = 119,
        AdultSignatureRestrictedDelivery = 120,
        InsurancePriorityMail = 125,
        USPSTrackingElectronic = 155,
        SignatureConfirmationElectronic = 156,
        CertificateOfMailingForm3817 = 160,
        CertifiedMailRestrictedDelivery = 170,
        CertifiedMailAdultSignatureRequired = 171,
        CertifiedMailAdultSignatureRestrictedDelivery = 172,
        SignatureConfirmRestrictDelivery = 173,
        SignatureConfirmationElectronicRestrictedDelivery = 174,
        CollectOnDeliveryRestrictedDelivery = 175,
        RegisteredMailRestrictedDelivery = 176,
        InsuranceRestrictedDelivery = 177,
        InsuranceRestrictDeliveryPriorityMail = 179,
        InsuranceRestrictDeliveryPriorityMailExpress = 178,
        InsuranceRestrictDeliveryBulkOnly = 180,
        ScanRetention = 181,
        ScanSignatureRetention = 182,
        [Obsolete("Effective July 10, 2022, USPS will eliminate Special Handling - Fragile")]
        SpecialHandlingFragile = 190
    }
}
