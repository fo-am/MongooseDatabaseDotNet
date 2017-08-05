using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Configuration;
using System.Text;


namespace DataPipe.Main
{
    class Sender
    {
        public string RabbitHostName { get;}
        public string RabbitUsername { get; }
        public string RabbitPassword { get; }

        public Sender()
        {
           
            RabbitHostName = ConfigurationManager.AppSettings["RabbitHostName"];
            RabbitUsername = ConfigurationManager.AppSettings["RabbitUsername"];
            RabbitPassword = ConfigurationManager.AppSettings["RabbitPassword"];
        }
        public void PublishEntity(sync_entity message)
        {
            var factory = new ConnectionFactory() { HostName = RabbitHostName, UserName = RabbitUsername, Password = RabbitPassword };
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true;
                    channel.QueueDeclare(queue: "mongoose_entity",
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);



                    var json = JsonConvert.SerializeObject(message);
                    var body = Encoding.UTF8.GetBytes(json);

                    channel.TxSelect();
                    channel.BasicPublish(exchange: "",
                                         routingKey: "mongoose_entity",
                                         basicProperties: null,
                                         body: body);
                    Data.MarkEntityAsSent(message);
                    channel.TxCommit();

                }

            }
        }

        public void PublishEntityVarchar(sync_value_varchar message)
        {
            var factory = new ConnectionFactory() { HostName = RabbitHostName, UserName = RabbitUsername, Password = RabbitPassword };
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true;

                    channel.QueueDeclare(queue: "mongoose_entity_varchar",
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                    var json = JsonConvert.SerializeObject(message);
                    var body = Encoding.UTF8.GetBytes(json);

                    channel.TxSelect();
                    channel.BasicPublish(exchange: "",
                                         routingKey: "mongoose_entity_varchar",
                                         basicProperties: null,
                                         body: body);
                    Data.MarkVarcharAsSent(message);
                    channel.TxCommit();

                }

            }
        }
    }
}
