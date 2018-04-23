using System;

namespace DataReceiver.Main.Model
{
    public class PackCreated : ISendable
    {
        public int sent { get; set; }
        public string UniqueId { get; set; }
        public int entity_id { get; set; }
        public string entity_type { get; set; }

        public string Name { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
}