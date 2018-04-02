using System;
using System.Collections.Generic;

namespace DataPipe.Main.Model.PupFocal
{
    public class PupFocal : ISendable
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
        public List<PupNearest> PupNearestList { get; set; }
        public string pupFeed { get; set; }
        public List<PupFeed> PupFeedList { get; set; }
        public string pupFind { get; set; }
        public List<PupFind> PupFindList { get; set; }
        public string pupCare { get; set; }
        public List<PupCare> PupCareList { get; set; }
        public string pupAggression { get; set; }
        public List<PupAggressionEvent> PupAggressionList { get; set; }
       
    }
}