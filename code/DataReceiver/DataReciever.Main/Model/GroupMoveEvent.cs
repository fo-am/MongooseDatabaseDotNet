using System;

namespace DataReciever.Main.Model
{
    public class GroupMoveEvent : ISendable
    {
        public int sent { get; set; }
        public string UniqueId { get; set; }
        public int entity_id { get; set; }
        public string entity_type { get; set; }

        public string pack { get; set; }
        public string packUniqueId { get; set; }
        public string leader { get; set; }
        public string leaderUniqueId { get; set; }
        public string Destination { get; set; }
        public string Direction { get; set; }
        public DateTime Time { get; set; }
        public string User { get; set; }
        public int? HowMany { get; set; }
        public int? Width { get; set; }
        public int? Depth { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
    }
}