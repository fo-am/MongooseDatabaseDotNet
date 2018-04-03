using System;

namespace DataReciever.Main.Model
{
    public class GroupCompositionMateGuard : ISendable
    {
        public string UniqueId { get; set; }
        public int entity_id { get; set; }
        public string entity_type { get; set; }
        public int sent { get; set; }

        public string femaleName { get; set; }
        public string femaleIndividualId { get; set; }
        public string guardName { get; set; }
        public string guardIndividualId { get; set; }
        public string strength { get; set; }
        public int pester { get; set; }
        public DateTime time { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }

    }
}