using System;

namespace DataReciever.Main.Model.Oestrus
{
    public class OestrusNearest : ISendable
    {
        public int entity_id { get; set; }
        public int sent { get; set; }
        public string entity_type { get; set; }
        public string UniqueId { get; set; }
        public string listClose { get; set; }
        public string nearestIndividualName { get; set; }
        public string nearestIndividualId { get; set; }
        public DateTime scanTime { get; set; }
    }
}