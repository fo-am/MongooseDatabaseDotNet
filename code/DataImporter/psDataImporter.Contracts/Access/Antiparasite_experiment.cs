using System;

namespace psDataImporter.Contracts.Access
{
    public class Antiparasite_experiment
    {
        public string PACK { get; set; }
        public string INDIV { get; set; }
        public DateTime STARTED_EXPERIMENT { get; set; }
        public DateTime A_FECAL_SAMPLE { get; set; }
        public DateTime FIRST_capture { get; set; }
        public string EXPERIMENT_GROUP { get; set; }
        public string B_FECAL { get; set; }
        public string C_FECAL { get; set; }
        public string SECOND_CAPTURE { get; set; }
        public string D_FECAL { get; set; }
        public string E_FECAL { get; set; }
        public string F_FECAL { get; set; }
        public string notes { get; set; }
    }
}