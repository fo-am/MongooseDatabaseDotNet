namespace DataPipe.Main.Model
{
    public class PackMove : ISendable
    {
        public int sent { get; set; }
        public string UniqueId { get; set; }
        public int entity_id { get; set; }
        public string entity_type { get; set; }

        public string mongooseId { get; set; }
        public string mongooseName { get; set; }
        public string packDestinationId { get; set; }
        public string packDestintionName { get; set; }
        public string packSourceId { get; set; }
        public string packSourceName { get; set; }
        public string time { get; set; }
        public string user { get; set; }
        public long latitude { get; set; }
        public long longitude { get; set; }
    }
}