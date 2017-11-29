namespace DataReciever.Main.Model
{
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