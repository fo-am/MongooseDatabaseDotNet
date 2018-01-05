using System;

namespace psDataImporter.Contracts.Access
{
    public class DNA_SAMPLES
    {
        public DateTime DATE { get; set; }
        public string SAMPLE_TYPE { get; set; }
        public string TISSUE { get; set; }
        public string STORAGE_ID { get; set; }
        public string TUBE_ID { get; set; }
        public string AGE { get; set; }
        public string SEX { get; set; }
        public string DISPERSAL { get; set; }
        public string PACK { get; set; }
        public string LITTER { get; set; }
        public string CODE { get; set; }
        public string COMMENT { get; set; }
        public string box_slot { get; set; }
    }
}