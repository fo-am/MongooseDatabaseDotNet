using System;
using System.Collections.Generic;
using System.Text;

using DataPipe.Main.Model;

using Newtonsoft.Json;

using NLog;
using NLog.Config;
using RabbitMQ.Client;
using RabbitMQ.Client.Impl;

using BasicProperties = RabbitMQ.Client.Framing.BasicProperties;

namespace DataPipe.Main
{
    internal class Sender
    {
        private readonly AppSettings appSettings;
        private readonly Logger logger;

        public Sender()
        {
            LogManager.Configuration = new XmlLoggingConfiguration("nlog.config");
            logger = LogManager.GetLogger("sender");
            appSettings = GetAppSettings.Get();
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

                        channel.ExchangeDeclare("mongoose.Dead.Letter", "direct",true);

                        var args = new Dictionary<string, object> { { "x-dead-letter-exchange", "mongoose.Dead.Letter" } };

                        channel.QueueDeclare($"mongoose_{typeof(T).Name}",
                            true,
                            false,
                            false,
                            args);

                        var json = JsonConvert.SerializeObject(message);
                        var body = Encoding.UTF8.GetBytes(json);

                        channel.TxSelect();

                        var properties = channel.CreateBasicProperties();
                        properties.Persistent = true;
                        properties.Headers = new Dictionary<string, object>
                        {
                            { "Id", Guid.NewGuid().ToString() }

                        };

                        channel.BasicPublish("",
                            $"mongoose_{typeof(T).Name}",
                            properties,
                            body);
                        Data.MarkAsSent(message);
                        channel.TxCommit();
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