
using System;
using System.Collections.Generic;

namespace DataReciever.Main.Model.PregnancyFocal
{
    public class PregnancyNearest:ISendable
    {
        public int sent { get; set; }
        public string UniqueId { get; set; }
        public int entity_id { get; set; }
        public string entity_type { get; set; }

        public string listClose { get; set; }
        public List<string> CloseListNames { get; set; }
        public string nearestIndividualName { get; set; }
        public string nearestIndividualId { get; set; }
        public DateTime scanTime { get; set; }
    }
}
