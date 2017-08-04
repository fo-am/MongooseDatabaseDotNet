using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;


namespace DataPipe.Main
{
    class Sender
    {
        public void PublishEntity(sync_entity message)
        {
            var factory = new ConnectionFactory() { HostName = "192.168.1.72", UserName = "aw", Password = "aw" };
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {

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
            var factory = new ConnectionFactory() { HostName = "192.168.1.72", UserName = "aw", Password = "aw" };
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {

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
