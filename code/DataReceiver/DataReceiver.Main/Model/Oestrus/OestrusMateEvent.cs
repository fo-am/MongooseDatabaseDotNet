using System;

namespace DataReceiver.Main.Model.Oestrus
{
    public class OestrusMateEvent : ISendable
    {
        public int sent { get; set; }
        public string UniqueId { get; set; }
        public int entity_id { get; set; }
        public string entity_type { get; set; }

        public string behaviour { get; set; }
        public string femaleResponse { get; set; }
        public string withIndividualName { get; set; }
        public string withIndividualId { get; set; }
        public string maleResponse { get; set; }
        public string success { get; set; }
        public DateTime time { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
    }
}