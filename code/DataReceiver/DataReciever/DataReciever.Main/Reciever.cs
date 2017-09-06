using System;
using System.Configuration;
using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace DataReciever.Main
{
    internal class Reciever
    {
        public Reciever()
        {
            RabbitHostName = ConfigurationManager.AppSettings["RabbitHostName"];
            RabbitUsername = ConfigurationManager.AppSettings["RabbitUsername"];
            RabbitPassword = ConfigurationManager.AppSettings["RabbitPassword"];
        }

        public string RabbitHostName { get; }
        public string RabbitUsername { get; }
        public string RabbitPassword { get; }

        public void Recieve<T>()
        {
            var factory = new ConnectionFactory
            {
                HostName = RabbitHostName,
                UserName = RabbitUsername,
                Password = RabbitPassword
            };

            var connection = factory.CreateConnection();

            Console.WriteLine($"Setting up reciever for: {typeof(T).Name}");
            var channel = connection.CreateModel();

            channel.QueueDeclare($"mongoose_{typeof(T).Name}",
                true,
                false,
                false,
                null);
            var consumer = new EventingBasicConsumer(channel);
            channel.BasicConsume($"mongoose_{typeof(T).Name}", false, consumer);

            consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body);
                var output = JsonConvert.DeserializeObject<T>(message);

                Data.StoreEntity(output);
                Console.WriteLine($"recieved {typeof(T).Name}");

                channel.BasicAck(ea.DeliveryTag, false);
            };
        }
    }
}