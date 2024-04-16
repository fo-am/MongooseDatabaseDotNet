using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace KeepAlive.Receiver

{
    internal class Receiver
    {
        private AppSettings settings;
        public Receiver() { settings = GetAppSettings.Get(); }

        public void Receive<T>()
        {
            var factory = new ConnectionFactory
            {
                HostName = settings.RabbitHostName,
                UserName = settings.RabbitUsername,
                Password = settings.RabbitPassword
            };

            var connection = factory.CreateConnection();

            Console.WriteLine($"Setting up receiver for: {typeof(T).Name}");
            var channel = connection.CreateModel();

            channel.QueueDeclare($"keepalive_{typeof(T).Name}", true, false, false, null);
            var consumer = new EventingBasicConsumer(channel);
            channel.BasicConsume($"keepalive_{typeof(T).Name}", false, consumer);

            consumer.Received += (model, ea) =>
            {
                ReadOnlyMemory<byte> body = ea.Body;
                var message = Encoding.UTF8.GetString(body.ToArray());
                var output = JsonConvert.DeserializeObject<T>(message);

                // Log receipt
                Console.WriteLine($"received {typeof(T).Name}");

                channel.BasicAck(ea.DeliveryTag, false);
            };
        }
    }
}