using System;
using System.Collections.Generic;
using System.Text;

using DataReciever.Main.Data;
using DataReciever.Main.Handlers;

using Newtonsoft.Json;

using NLog;

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

            var args = new Dictionary<string, object>
            {
                { "x-dead-letter-exchange", "mongoose.Dead.Letter" },
                {
                    "x-dead-letter-routing-key", $"DLQ.mongoose_{typeof(T).Name}"
                }
            };

            channel.QueueDeclare($"mongoose_{typeof(T).Name}",
                true,
                false,
                false,
                args);

            channel.QueueDeclare($"DLQ.mongoose_{typeof(T).Name}",
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
                var messageIdBytes =(byte[]) ea.BasicProperties.Headers["Id"];
                var messageId = Encoding.UTF8.GetString(messageIdBytes);
                int logId;
                try
                {
                    logId = PgRepository.StoreMessage(typeof(T).FullName, message, messageId);
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                  
                    throw;
                }
               

                var handler = new GetHandler();
                try
                {
                    handler.Handle<T>(output);
                    PgRepository.MessageHandledOk(logId);
                }
                catch (Exception ex)
                {
                    var attemptsToHandle = PgRepository.FailedToHandleMessage(logId, ex);
                    if (attemptsToHandle > 5)
                    {
                        //If we have seen this message many times then don't re-que.
                        channel.BasicNack(ea.DeliveryTag, false, false);
                        return;
                    }

                    // re-que so we can re-try later.
                    channel.BasicNack(ea.DeliveryTag, false, true);
                    return;
                }

                Console.WriteLine($"recieved {typeof(T).Name}");

                channel.BasicAck(ea.DeliveryTag, false);
            };
        }
    }
}