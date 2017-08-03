using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataPipe.Main
{
    class Sender
    {
        public void Publish(string message)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: "mongoose",
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);


                    var body = Encoding.UTF8.GetBytes(message);

                    channel.BasicPublish(exchange: "",
                                         routingKey: "mongoose",
                                         basicProperties: null,
                                         body: body);

                }

            }
        }

    }
}
