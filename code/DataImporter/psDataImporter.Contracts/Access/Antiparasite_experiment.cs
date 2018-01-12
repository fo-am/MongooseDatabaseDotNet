using System;

namespace psDataImporter.Contracts.Access
{
    public class Antiparasite_experiment
    {
        public string PACK { get; set; }
        public string INDIV { get; set; }
        public DateTime STARTED_EXPERIMENT { get; set; }
        public DateTime? A_FECAL_SAMPLE { get; set; }
        public DateTime FIRST_capture { get; set; }
        public string EXPERIMENT_GROUP { get; set; }
        public DateTime? B_FECAL { get; set; }
        public DateTime? C_FECAL { get; set; }
        public DateTime? SECOND_CAPTURE { get; set; }
        public DateTime? D_FECAL { get; set; }
        public DateTime? E_FECAL { get; set; }
        public DateTime? F_FECAL { get; set; }
        public string notes { get; set; }
    }
}