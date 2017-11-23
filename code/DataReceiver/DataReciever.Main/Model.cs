using System;

namespace DataReciever.Main
{
    public interface ISendable
    {
        int sent { get; set; }
        string UniqueId { get; set; }
        int entity_id { get; set; }
        string entity_type { get; set; }
    }

    public class IndividualCreated : ISendable
    {
        public string UniqueId { get; set; }
        public int entity_id { get; set; }
        public string entity_type { get; set; }

        public string Name { get; set; }
        public string Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string LitterCode { get; set; }
        public string ChipCode { get; set; }
        public double? CollerWeight { get; set; }
        public int sent { get; set; }
        public string PackCode { get; set; }
        public string PackUniqueId { get; set; }
    }

    public class IndividualUpdate : ISendable
    {
        public string UniqueId { get; set; }
        public int entity_id { get; set; }
        public string entity_type { get; set; }

        public string Name { get; set; }
        public string Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string LitterCode { get; set; }
        public string ChipCode { get; set; }
        public double? CollerWeight { get; set; }
        public int sent { get; set; }
    }

    public class IndividualDied : ISendable
    {
        public string UniqueId { get; set; }
        public int entity_id { get; set; }
        public string entity_type { get; set; }

        public string Name { get; set; }
        public DateTime DateOfDeath { get; set; }

        public int sent { get; set; }
    }

    public class WeightMeasure : ISendable
    {
        public string UniqueId { get; set; }
        public int entity_id { get; set; }
        public string entity_type { get; set; }

        public string IndividualName { get; set; }

        public int Weight { get; set; }
        public int CollarWeight { get; set; }
        public bool Accurate { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int sent { get; set; }
    }
}