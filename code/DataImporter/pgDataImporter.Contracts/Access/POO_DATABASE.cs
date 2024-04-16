﻿using System;

namespace pgDataImporter.Contracts.Access
{
    public class POO_DATABASE

    {
        public string Sample_Number { get; set; }
        public DateTime Date { get; set; }
        public string Pack { get; set; }
        public string Pack_Status { get; set; }
        public string Emergence_Time { get; set; }
        public DateTime Time_in_Freezer { get; set; }
        public string Individual { get; set; }
        public string Time_of_Collection { get; set; }
        public string Parasite_sample_taken { get; set; }
        public string Comment { get; set; }
    }
}