using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DataPipe.Main.Model;

using Newtonsoft.Json;

using NLog;
using NLog.Config;
using RabbitMQ.Client;
using RabbitMQ.Client.Impl;

using BasicProperties = RabbitMQ.Client.Framing.BasicProperties;

namespace DataInput.Core
{
    public class Sender
    {
        private readonly AppSettings appSettings;
        private readonly Logger logger;

        public Sender()
        {
            LogManager.Configuration = new XmlLoggingConfiguration("nlog.config");
            logger = LogManager.GetLogger("sender");
            appSettings = GetAppSettings.Get();
        }

        public void PublishEntity(string type, string messageString)
        {
            dynamic message = JsonConvert.DeserializeObject(messageString);
            logger.Info($"publishing {message.UniqueId}");

            string typeName = type.Split(".").Last();

            var factory = new ConnectionFactory
            {
                HostName = appSettings.RabbitHostName,
                UserName = appSettings.RabbitUsername,
                Password = appSettings.RabbitPassword
            };
            try
            {
                using (var connection = factory.CreateConnection())
                {
                    using (var channel = connection.CreateModel())
                    {
                        Console.WriteLine($"Setting up queues for: {typeName}");
                        // Declair dead letter queue for this type
                        channel.ExchangeDeclare("mongoose.Dead.Letter", "direct", true);
                        var queueArgs = new Dictionary<string, object>
                        {
                            { "x-dead-letter-exchange", "mongoose" },
                            {
                                "x-dead-letter-routing-key", $"mongoose_{typeName}"
                            }
                            ,{ "x-message-ttl", 30000 }
                        };

                        channel.QueueDeclare($"DLQ.mongoose_{typeName}",
                            true,
                            false,
                            false,
                            queueArgs);
                        channel.QueueBind($"DLQ.mongoose_{typeName}", "mongoose.Dead.Letter", $"DLQ.mongoose_{typeName}", null);

                        // declair queue for this type
                        channel.ExchangeDeclare("mongoose", "direct", true);
                        var args = new Dictionary<string, object>
                        {
                            { "x-dead-letter-exchange", "mongoose.Dead.Letter" },
                            {
                                "x-dead-letter-routing-key", $"DLQ.mongoose_{typeName}"
                            }
                        };

                        channel.QueueDeclare($"mongoose_{typeName}",
                            true,
                            false,
                            false,
                            args);
                        channel.QueueBind($"mongoose_{typeName}", "mongoose", $"mongoose_{typeName}", null);

                        // use the queue

                        var body = Encoding.UTF8.GetBytes(messageString);

                        channel.TxSelect();

                        var properties = channel.CreateBasicProperties();
                        properties.Persistent = true;
                        var messageId = Guid.NewGuid().ToString();
                        properties.Headers = new Dictionary<string, object>
                        {
                            { "Id", messageId }
                        };

                        channel.BasicPublish("mongoose",
                            $"mongoose_{typeName}",
                            properties,
                            body);

                        channel.TxCommit();

                        logger.Info($"Sent message with id '{messageId}' to 'mongoose_{typeName}'.");
                    }
                }
            }
            catch (Exception exception)
            {
                logger.Error(exception,
                    $"An error occured with queue '{appSettings.RabbitHostName}' and message type '{typeName}'");
                throw;
            }
        }

        public void PublishEntity<T>(T message) where T : class, ISendable
        {
            logger.Info($"publishing {message.UniqueId}");

            var factory = new ConnectionFactory
            {
                HostName = appSettings.RabbitHostName,
                UserName = appSettings.RabbitUsername,
                Password = appSettings.RabbitPassword
            };
            try
            {
                using (var connection = factory.CreateConnection())
                {
                    using (var channel = connection.CreateModel())
                    {
                        Console.WriteLine($"Setting up queues for: {typeof(T).Name}");
                        // Declair dead letter queue for this type
                        channel.ExchangeDeclare("mongoose.Dead.Letter", "direct", true);
                        var queueArgs = new Dictionary<string, object>
                        {
                            { "x-dead-letter-exchange", "mongoose" },
                            {
                                "x-dead-letter-routing-key", $"mongoose_{typeof(T).Name}"
                            }
                            ,{ "x-message-ttl", 30000 }
                        };

                        channel.QueueDeclare($"DLQ.mongoose_{typeof(T).Name}",
                            true,
                            false,
                            false,
                            queueArgs);
                        channel.QueueBind($"DLQ.mongoose_{typeof(T).Name}", "mongoose.Dead.Letter", $"DLQ.mongoose_{typeof(T).Name}", null);

                        // declair queue for this type
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

                        // use the queue
                        var json = JsonConvert.SerializeObject(message);
                        var body = Encoding.UTF8.GetBytes(json);

                        channel.TxSelect();

                        var properties = channel.CreateBasicProperties();
                        properties.Persistent = true;
                        var messageId = Guid.NewGuid().ToString();
                        properties.Headers = new Dictionary<string, object>
                        {
                            { "Id", messageId }
                        };

                        channel.BasicPublish("mongoose",
                            $"mongoose_{typeof(T).Name}",
                            properties,
                            body);
                        //     Data.MarkAsSent(message);
                        channel.TxCommit();

                        logger.Info($"Sent message with id '{messageId}' to 'mongoose_{typeof(T).Name}'.");
                    }
                }
            }
            catch (Exception exception)
            {
                logger.Error(exception,
                    $"An error occured with queue '{appSettings.RabbitHostName}' and message type '{typeof(T)}'");
                throw;
            }
        }
    }
}