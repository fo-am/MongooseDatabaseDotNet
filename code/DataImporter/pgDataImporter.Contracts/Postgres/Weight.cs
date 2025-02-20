﻿using System;

namespace pgDataImporter.Contracts.Postgres
{
    public class Weight
    {
        public int WeightId { get; set; }
        public int PackHistoryId { get; set; }
        public int WeightGrams { get; set; }
        public DateTime Time { get; set; }
        public int Accuracy { get; set; }
        public string Session { get; set; }
        public int CollarWeight { get; set; }
        public string Location { get; set; } //Geography 
        public string Comment { get; set; }
    }
}