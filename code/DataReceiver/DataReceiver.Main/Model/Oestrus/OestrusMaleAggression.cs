using System;

namespace DataReceiver.Main.Model.Oestrus
{
    public class OestrusMaleAggression : ISendable
    {
        public int sent { get; set; }
        public string UniqueId { get; set; }
        public int entity_id { get; set; }
        public string entity_type { get; set; }

        public string initiatorIndividualName { get; set; }
        public string initiatorIndividualId { get; set; }
        public string receiverIndividualName { get; set; }
        public string receiverIndividualId { get; set; }
        public string level { get; set; }
        public string winner { get; set; }
        public string owner { get; set; }
        public DateTime time { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
    }
}