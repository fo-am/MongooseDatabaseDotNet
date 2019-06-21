using System;

namespace DataPipe.Main.Model
{
    public class PupAssociation : ISendable
    {
        public string UniqueId { get; set; }
        public int entity_id { get; set; }
        public string entity_type { get; set; }
        public int sent { get; set; }

        public string pupName { get; set; }
        public string pupIndividualId { get; set; }
        public string escortName { get; set; }
        public string escortIndividualId { get; set; }
        public string strength { get; set; }
        public string accurate { get; set; }
        public DateTime time { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }

    }
}