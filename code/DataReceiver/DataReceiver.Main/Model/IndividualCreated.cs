using System;

namespace DataReceiver.Main.Model
{
    public class IndividualCreated : ISendable
    {
        public string UniqueId { get; set; }
        public int entity_id { get; set; }
        public string entity_type { get; set; }

        public string Name { get; set; }
        public string Gender { get; set; }
        public string DateOfBirthString { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string LitterCode { get; set; }
        public string ChipCode { get; set; }
        public double? CollarWeight { get; set; }
        public int sent { get; set; }
        public string PackCode { get; set; }
        public string PackUniqueId { get; set; }
        public double? Longitude { get; set; }
        public double? Latitude { get; set; }
    }
}