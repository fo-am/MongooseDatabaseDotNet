using System;

namespace DataReciever.Main.Model
{
    public class GroupAlarmEvent : ISendable
    {
        public int sent { get; set; }
        public string UniqueId { get; set; }
        public int entity_id { get; set; }
        public string entity_type { get; set; }

        public string cause { get; set; }
        public string othersJoin { get; set; }
        public DateTime time { get; set; }
        public string user { get; set; }
        public string packName { get; set; }
        public string packUniqueId { get; set; }
        public string callerName { get; set; }
        public string callerUniqueId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}