using System;
using System.Collections.Generic;

namespace DataReceiver.Main.Model.Oestrus
{
    public class OestrusEvent : ISendable
    {
        public int sent { get; set; }
        public string UniqueId { get; set; }
        public int entity_id { get; set; }
        public string entity_type { get; set; }

        public string focalIndividualName { get; set; }
        public string focalIndividualId { get; set; }
        public DateTime time { get; set; }
        public string user { get; set; }
        public double longitude { get; set; }
        public double latitude { get; set; }
        public int? visibleIndividuals { get; set; }
        public int? depth { get; set; }
        public int? width { get; set; }
        public string packName { get; set; }
        public string packUniqueId { get; set; }
        public string nearest { get; set; }
        public List<OestrusNearest> NearestList { get; set; }
        public string maleaggr { get; set; }
        public List<OestrusMaleAggression> MaleAggressionList { get; set; }
        public string mate { get; set; }
        public List<OestrusMateEvent> MateEventList { get; set; }
        public string aggr { get; set; }
        public List<OestrusAggressionEvent> AggressionEventList { get; set; }
        public string affil { get; set; }
        public List<OestrusAffiliationEvent> AffiliationEventList { get; set; }
    }
}