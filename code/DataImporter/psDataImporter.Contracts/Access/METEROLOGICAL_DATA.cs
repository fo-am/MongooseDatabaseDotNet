using System;

namespace pgDataImporter.Contracts.Access
{
    public class METEROLOGICAL_DATA
    {
        public DateTime DATE { get; set; }
        public double? RAIN_MWEYA { get; set; }
        public double? MAX_TEMP { get; set; }
        public double? MIN_TEMP { get; set; }
        public double? TEMP { get; set; }
        public double? MAX_HUMIDITY { get; set; }
        public double? MIN_HUMIDITY { get; set; }
        public double? TIME_TAKEN { get; set; }
        public string OBSERVER { get; set; }
    }
}