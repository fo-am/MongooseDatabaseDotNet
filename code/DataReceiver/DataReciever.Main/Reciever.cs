using System;
using System.Text;

using DataReciever.Main.Data;
using DataReciever.Main.Handlers;

using Newtonsoft.Json;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace DataReciever.Main
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

                var logId = PgRepository.StoreMessage(typeof(T).FullName, message);

                var handler = new GetHandler();
                try
                {
                    handler.Handle<T>(output);
                    PgRepository.MessageHandledOk(logId);
                }
                catch (Exception ex)
                {
                    PgRepository.FailedToHandleMessage(logId, ex);
                }

                // catch
                // store exception.
                Console.WriteLine($"recieved {typeof(T).Name}");

                channel.BasicAck(ea.DeliveryTag, false);
            };
        }
    }
}