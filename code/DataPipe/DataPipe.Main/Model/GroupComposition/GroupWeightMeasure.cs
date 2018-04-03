using System;

namespace DataPipe.Main.Model
{
    public class GroupWeightMeasure : ISendable
    {
        public string UniqueId { get; set; }
        public int entity_id { get; set; }
        public string entity_type { get; set; }
        public int sent { get; set; }

        public string individualName { get; set; }
        public string individualId { get; set; }
        public string name { get; set; }
        public double weight { get; set; }
        public double collarWeight { get; set; }
        public int present { get; set; }
        public int accurate { get; set; }
        public DateTime time { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }

    }
}