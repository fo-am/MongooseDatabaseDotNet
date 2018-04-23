using System;
using System.Collections.Generic;

namespace DataReceiver.Main.Model
{
    public class GroupComposition : ISendable
    {

        public int sent { get; set; }
        public string UniqueId { get; set; }
        public int entity_id { get; set; }
        public string entity_type { get; set; }

        public string pupList { get; set; }
        public List<string> PupNames { get; set; }
        public string groupCompCode { get; set; }
        public string observer { get; set; }
        public string packName { get; set; }
        public string packUniqueId { get; set; }
        public string pregnantList { get; set; }
        public List<string> PregnantNames { get; set; }
        public string user { get; set; }
        public string weightIds { get; set; }
        public List<GroupWeightMeasure> WeightsList { get; set; }
        public string mateGuardIds { get; set; }
        public List<GroupCompositionMateGuard> MateGuardsList { get; set; }
        public string pupAssociationIds { get; set; }
        public List<PupAssociation> PupAssociationsList { get; set; }
        public DateTime time { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}