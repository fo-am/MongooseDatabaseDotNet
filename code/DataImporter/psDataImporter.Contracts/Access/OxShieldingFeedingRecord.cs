using System;

namespace psDataImporter.Contracts.Access
{
    public class OxShieldingFeedingRecord
    {
        public DateTime Date { get; set; }
        public string Female_ID { get; set; }
        public string Pack { get; set; }
        public string AMPM { get; set; }
        public int Amount_of_egg { get; set; }
        public string Comments { get; set; }
    }
}