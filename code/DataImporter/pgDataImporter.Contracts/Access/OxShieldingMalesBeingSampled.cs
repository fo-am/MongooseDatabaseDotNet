using System;

namespace pgDataImporter.Contracts.Access
{
    public class OxShieldingMalesBeingSampled
    {
        public string PACK { get; set; }
        public string ID { get; set; }
        public string STATUS { get; set; }
        public DateTime DATE_START { get; set; }
        public DateTime DATE_STOP { get; set; }
        public string COMMENT { get; set; }


    }
}