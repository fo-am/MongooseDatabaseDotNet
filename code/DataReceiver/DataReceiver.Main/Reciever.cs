using System;
using System.Collections.Generic;
using System.Text;

using Autofac.Extras.NLog;

using DataReceiver.Main.Interfaces;

using Newtonsoft.Json;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace DataReceiver.Main
{
    internal class Receiver : IReceiver
    {
        private ILogger _logger;
        private IConnection _connection;
        private IPgRepository _data;
        private IGetHandler _handler;

        public Receiver(ILogger logger, IConnection connection, IPgRepository data, IGetHandler handler)
        {
            _logger = logger;
            _connection = connection;
            _data = data;
            _handler = handler;
        }

        public void Receive<T>()
        {
            _logger.Info($"Setting up receiver for: {typeof(T).Name}");
            var channel = _connection.CreateModel();

            channel.ExchangeDeclare("mongoose.Dead.Letter", "direct", true);
            var queueArgs = new Dictionary<string, object>
            {
                { "x-dead-letter-exchange", "mongoose" },
                {
                    "x-dead-letter-routing-key", $"mongoose_{typeof(T).Name}"
                }
                ,{ "x-message-ttl",  30000 }
            };

            channel.QueueDeclare($"DLQ.mongoose_{typeof(T).Name}",
                true,
                false,
                false,
                queueArgs);
            channel.QueueBind($"DLQ.mongoose_{typeof(T).Name}", "mongoose.Dead.Letter", $"DLQ.mongoose_{typeof(T).Name}", null);



            channel.ExchangeDeclare("mongoose", "direct", true);
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
            channel.QueueBind($"mongoose_{typeof(T).Name}", "mongoose", $"mongoose_{typeof(T).Name}", null);

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
                    logId = _data.StoreMessage(typeof(T).FullName, message, messageId);
                }
                catch (Exception exception)
                {
                    _logger.Error(exception.Message);
                    throw;
                }
               
                try
                {
                    _handler.Handle<T>(output);
                    _data.MessageHandledOk(logId);
                }
                catch (Exception ex)
                {
                    var attemptsToHandle = _data.FailedToHandleMessage(logId, ex);
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

                _logger.Info($"Received {typeof(T).Name}");

                channel.BasicAck(ea.DeliveryTag, false);
            };
        }
    }
}