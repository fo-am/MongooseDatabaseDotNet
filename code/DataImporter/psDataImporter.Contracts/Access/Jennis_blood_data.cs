using System;

namespace pgDataImporter.Contracts.Access
{
   public class Jennis_blood_data

    {
        public DateTime Date { get; set; }
        public string Mongoose { get; set; }
        public DateTime Trap_time { get; set; }
        public string Bleed_time { get; set; }
        public int Weight { get; set; }
        public DateTime? Release_time { get; set; }
        public string Sample { get; set; }
        public DateTime Spinning_time { get; set; }
        public DateTime Freeze_time { get; set; }
        public string Focal { get; set; }
        public int Amount_of_plasma { get; set; }
        public string Comment { get; set; }
    }
}