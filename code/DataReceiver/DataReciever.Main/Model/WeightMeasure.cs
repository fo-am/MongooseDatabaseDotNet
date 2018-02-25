using System;

namespace DataReciever.Main.Model
{
    public class WeightMeasure : ISendable
    {
        public string UniqueId { get; set; }
        public int entity_id { get; set; }
        public string entity_type { get; set; }

        public string IndividualName { get; set; }
        public string IndividualUniqueId { get; set; }

        public string PackId { get; set; }
        public string PackUniqueId { get; set; }

        public double Weight { get; set; }
        public double? CollarWeight { get; set; }
        public int? Accurate { get; set; }
        public DateTime? Time { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public int sent { get; set; }
        
    }
}