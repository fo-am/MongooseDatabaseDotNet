using System;
using System.Collections.Generic;

namespace DataReceiver.Main.Model.PupFocal
{
    public class PupNearest : ISendable
    {
        public int entity_id { get; set; }
        public int sent { get; set; }
        public string entity_type { get; set; }
        public string UniqueId { get; set; }
        public string listClose { get; set; }
        public List<string> CloseListNames { get; set; }
        public string nearestIndividualName { get; set; }
        public string nearestIndividualId { get; set; }
        public DateTime scanTime { get; set; }
      
    }
}