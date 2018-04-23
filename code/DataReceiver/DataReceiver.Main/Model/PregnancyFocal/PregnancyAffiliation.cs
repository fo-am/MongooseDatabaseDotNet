

using System;

namespace DataReceiver.Main.Model.PregnancyFocal
{
  public  class PregnancyAffiliation:ISendable
    {
        public int sent { get; set; }
        public string UniqueId { get; set; }
        public int entity_id { get; set; }
        public string entity_type { get; set; }

        public string withIndividualName { get; set; }
        public string withIndividualId { get; set; }
        public string initiate { get; set; }
        public string over { get; set; }
        public DateTime time { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
    }
}
