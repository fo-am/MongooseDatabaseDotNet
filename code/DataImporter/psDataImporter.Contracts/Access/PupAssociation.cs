using System;

namespace pgDataImporter.Contracts.Access
{
    public class PupAssociation
    {
        public DateTime DATE { get; set; }
        public string SESSION { get; set; }
        public string GROUP { get; set; }
        public string LITTER { get; set; }
        public string PUP { get; set; }
        public string PUP_SEX { get; set; }
        public string ESCORT { get; set; }
        public string ESC_SEX { get; set; }
        public int? STRENGTH { get; set; }
        public int? CONFIDENCE { get; set; }
        public string COMMENT { get; set; }
        public string Editing_comments { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
    }
}