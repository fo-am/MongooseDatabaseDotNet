using System;
using System.Collections.Generic;
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

            channel.ExchangeDeclare("mongoose.Dead.Letter", "direct", true);

            var args = new Dictionary<string, object> { { "x-dead-letter-exchange", "mongoose.Dead.Letter" } };

            channel.QueueDeclare($"mongoose_{typeof(T).Name}",
                true,
                false,
                false,
                args);

            var consumer = new EventingBasicConsumer(channel);
            channel.BasicConsume($"mongoose_{typeof(T).Name}", false, consumer);

            consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body);
                var output = JsonConvert.DeserializeObject<T>(message);
                var messageIdBytes =(byte[]) ea.BasicProperties.Headers["Id"];
                var messageId = Encoding.UTF8.GetString(messageIdBytes);
                var logId = PgRepository.StoreMessage(typeof(T).FullName,  message, messageId);

                var handler = new GetHandler();
                try
                {
                    handler.Handle<T>(output);
                    PgRepository.MessageHandledOk(logId);
                }
                catch (Exception ex)
                {
                    PgRepository.FailedToHandleMessage(logId, ex);
                    channel.BasicNack(ea.DeliveryTag, false, true);
                    return;
                }

                // catch
                // store exception.
                Console.WriteLine($"recieved {typeof(T).Name}");

                channel.BasicAck(ea.DeliveryTag, false);
            };
        }
    }
}