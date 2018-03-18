namespace DataPipe.Main.Model
{
    public class PackMove : ISendable
    {
        public int sent { get; set; }
        public string UniqueId { get; set; }
        public int entity_id { get; set; }
        public string entity_type { get; set; }

        public string MongooseId { get; set; }
        public string MongooseName { get; set; }
        public string PackDestinationId { get; set; }
        public string PackDestintionName { get; set; }
        public string PackSourceId { get; set; }
        public string PackSourceName { get; set; }
        public string Time { get; set; }
        public string User { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}