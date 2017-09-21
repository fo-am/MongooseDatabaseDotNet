using System;

using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace KeepAlive.Reciever

{
    internal class Reciever
    {
        private AppSettings settings;
        public Reciever()
        {
            settings = GetAppSettings.Get();
        }

        public void Recieve<T>()
        {
            var factory = new ConnectionFactory
            {
                HostName = settings.RabbitHostName,
                UserName = settings.RabbitUsername,
                Password = settings.RabbitPassword
            };

            var connection = factory.CreateConnection();

            Console.WriteLine($"Setting up reciever for: {typeof(T).Name}");
            var channel = connection.CreateModel();

            channel.QueueDeclare($"keepalive_{typeof(T).Name}",
                true,
                false,
                false,
                null);
            var consumer = new EventingBasicConsumer(channel);
            channel.BasicConsume($"keepalive_{typeof(T).Name}", false, consumer);

            consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body);
                var output = JsonConvert.DeserializeObject<T>(message);

              // Log receipt
                Console.WriteLine($"recieved {typeof(T).Name}");

                channel.BasicAck(ea.DeliveryTag, false);
            };
        }
    }
}