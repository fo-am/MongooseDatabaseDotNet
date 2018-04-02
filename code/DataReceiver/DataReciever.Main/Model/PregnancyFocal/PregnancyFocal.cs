using System;
using System.Collections.Generic;

namespace DataReciever.Main.Model.PregnancyFocal
{
    public class PregnancyFocal : ISendable
    {
        public int sent { get; set; }
        public string UniqueId { get; set; }
        public int entity_id { get; set; }
        public string entity_type { get; set; }

        public string packName { get; set; }
        public string packUniqueId { get; set; }
        public string focalIndividualName { get; set; }
        public string focalIndividualId { get; set; }
        public int? visibleIndividuals { get; set; }
        public int? depth { get; set; }
        public int? width { get; set; }
        public string pregnancyNearest { get; set; }
        public List<PregnancyNearest> PregnancyNearestList { get; set; }
        public string pregnancyAffil { get; set; }
        public List<PregnancyAffiliation> PregnancyAffiliationList { get; set; }
        public string pregnancyAggression { get; set; }
        public List<PregnancyAggression> PregnancyAggressionList { get; set; }
        public DateTime time { get; set; }
        public string user { get; set; }
        public double longitude { get; set; }
        public double latitude { get; set; }
       
    }
}
