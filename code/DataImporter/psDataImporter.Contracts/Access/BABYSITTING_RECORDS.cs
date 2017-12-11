using System;

namespace psDataImporter.Contracts.Access
{
    public class BABYSITTING_RECORDS
    {
        public DateTime DATE { get; set; }
        public string GROUP { get; set; }
        public string LITTER_CODE { get; set; }
        public string BS { get; set; }
        public string SEX { get; set; }
        public string TYPE { get; set; }
        public string TIME_START { get; set; }
        public string DEN_DIST { get; set; }
        public string TIME_END { get; set; }
        public int? ACCURACY { get; set; }
        public string Edited { get; set; }
        public string COMMENT { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
    }
}