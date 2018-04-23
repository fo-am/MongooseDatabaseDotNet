using System;

namespace DataReceiver.Main.Model
{
    public class InterGroupInteractionEvent : ISendable
    {
        public int sent { get; set; }
        public string UniqueId { get; set; }
        public int entity_id { get; set; }
        public string entity_type { get; set; }

        public string outcome { get; set; }
        public string leaderName { get; set; }
        public string leaderUniqueId { get; set; }

        public string packName { get; set; }
        public string packUniqueId { get; set; }

        public string otherPackName { get; set; }
        public string otherPackUniqueId { get; set; }
        public DateTime time { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public int duration { get; set; }
    }
}