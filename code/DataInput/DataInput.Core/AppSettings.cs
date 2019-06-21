namespace DataInput.Core
{
    public class AppSettings
    {
        public string SourcePostgres { get; set; }
        public string DestinationPostgres { get; set; }

        public string RabbitHostName { get; set; }
        public string RabbitUsername { get; set; }
        public string RabbitPassword { get; set; }
    }
}