using System;

namespace DataReceiver.Main.Model
{
    public class LitterCreated : ISendable
    {
        public int sent { get; set; }
        public string UniqueId { get; set; }
        public int entity_id { get; set; }
        public string entity_type { get; set; }

        public DateTime Date { get; set; }
        public string LitterName { get; set; }
        public string PackName { get; set; }
        public string PackUniqueId { get; set; }
    }
}