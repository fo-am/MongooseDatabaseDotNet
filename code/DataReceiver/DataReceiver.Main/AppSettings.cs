namespace DataReceiver.Main { 
    public class AppSettings
    {
        public string RabbitHostName { get; set; }
        public string RabbitUsername { get; set; }
        public string RabbitPassword { get; set; }
        public string PostgresConnection { get; set; }
    }
}