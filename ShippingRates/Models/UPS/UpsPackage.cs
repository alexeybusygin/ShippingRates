namespace ShippingRates.Models.UPS
{
    internal class UpsPackage
    {
        public PackagingType PackagingType { get; set; }
        public UpsPackageWeight PackageWeight { get; set; }
        public UpsDimensions Dimensions { get; set; }
        public PackageServiceOptions PackageServiceOptions  { get; set; }

        public UpsPackage(Package package, UnitsSystem unitsSystem)
        {
            PackagingType = new PackagingType()
            {
                Code = "02"
            };
            PackageWeight = new UpsPackageWeight(package, unitsSystem);
            Dimensions = new UpsDimensions(package, unitsSystem);

            if (package.SignatureRequiredOnDelivery)
            {
                PackageServiceOptions = new PackageServiceOptions()
                {
                    DeliveryConfirmation = new DeliveryConfirmation()
                    {
                        DCISType = "2"
                    }
                };
            }
        }
    }

    internal class UpsDimensions
    {
        public UpsDimensions(Package package, UnitsSystem unitsSystem)
        {
            UnitOfMeasurement = new UnitOfMeasurement()
            {
                Code = unitsSystem == UnitsSystem.Metric ? "CM" : "IN"
            };
            Length = package.GetRoundedLength(unitsSystem).ToString();
            Width = package.GetRoundedWidth(unitsSystem).ToString();
            Height = package.GetRoundedHeight(unitsSystem).ToString();
        }

        public UnitOfMeasurement UnitOfMeasurement { get; set; }
        public string Length { get; set; }
        public string Width { get; set; }
        public string Height { get; set; }
    }

    internal class PackageServiceOptions
    {
        public DeliveryConfirmation DeliveryConfirmation { get; set; }
    }

    internal class DeliveryConfirmation
    {
        public string DCISType { get; set; }
    }

    internal class UpsPackageWeight
    {
        public UpsPackageWeight(Package package, UnitsSystem unitsSystem)
        {
            UnitOfMeasurement = new UnitOfMeasurement()
            {
                Code = unitsSystem == UnitsSystem.Metric ? "KGS" : "LBS"
            };
            Weight = package.GetRoundedWeight(unitsSystem).ToString();
        }

        public UnitOfMeasurement UnitOfMeasurement { get; set; }
        public string Weight { get; set; }
    }

    internal class UnitOfMeasurement
    {
        public string Code { get; set; }
    }

    internal class PackagingType
    {
        public string Code { get; set; }
    }

}
