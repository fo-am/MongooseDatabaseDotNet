using System;

namespace pgDataImporter.Contracts.Access
{
    public class ProvisioningData
    {
        public DateTime Date { get; set; }
        public string Visit_time { get; set; }
        public string Pack { get; set; }
        public string Litter { get; set; }
        public string Female_ID { get; set; }
        public string Amount_of_egg { get; set; }
        public string Comments { get; set; }
    }
}