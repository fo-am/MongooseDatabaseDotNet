using System;

namespace DataReciever.Main.Model
{
    public class IndividualDied : ISendable
    {
        public string UniqueId { get; set; }
        public int entity_id { get; set; }
        public string entity_type { get; set; }

        public string Name { get; set; }
        public DateTime DateOfDeath { get; set; }

        public int sent { get; set; }
    }
}