using System.Configuration;
using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace DataPipe.Main
{
    internal class Sender
    {
        public Sender()
        {
            RabbitHostName = ConfigurationManager.AppSettings["RabbitHostName"];
            RabbitUsername = ConfigurationManager.AppSettings["RabbitUsername"];
            RabbitPassword = ConfigurationManager.AppSettings["RabbitPassword"];
        }

        public string RabbitHostName { get; }
        public string RabbitUsername { get; }
        public string RabbitPassword { get; }

        public void PublishEntity<T>(T message) where T : class, ISendable
        {
            var factory = new ConnectionFactory
            {
                HostName = RabbitHostName,
                UserName = RabbitUsername,
                Password = RabbitPassword
            };
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true;

                    channel.QueueDeclare($"mongoose_{typeof(T).Name}",
                        true,
                        false,
                        false,
                        null);

                    var json = JsonConvert.SerializeObject(message);
                    var body = Encoding.UTF8.GetBytes(json);

                    channel.TxSelect();
                    channel.BasicPublish("",
                        $"mongoose_{typeof(T).Name}",
                        null,
                        body);
                    Data.MarkAsSent(message);
                    channel.TxCommit();
                }
            }
        }
    }
}