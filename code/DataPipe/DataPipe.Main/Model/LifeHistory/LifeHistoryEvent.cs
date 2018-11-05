using System;

namespace DataPipe.Main.Model.LifeHistory
{
    public class LifeHistoryEvent : ISendable
    {
        public int sent { get; set; }
        public int entity_id { get; set; }
        public string entity_type { get; set; }

        public string Type { get; set; }
        public string UniqueId { get; set; }
        public string Code { get; set; }
        public string entity_name { get; set; }
        public string associated_pack_name { get; set; }
        public DateTime Date { get; set; }
    }
}
