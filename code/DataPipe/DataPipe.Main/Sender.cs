using System;
using System.Text;

using Newtonsoft.Json;

using NLog;

using RabbitMQ.Client;

namespace DataPipe.Main
{
    internal class Sender
    {
        private readonly AppSettings appSettings;
        private readonly Logger logger;

        public Sender()
        {
            logger = LogManager.GetLogger("sender");
            appSettings = GetAppSettings.Get();
        }

        public void PublishEntity<T>(T message) where T : class, ISendable
        {
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
                        var properties = channel.CreateBasicProperties();
                        properties.Persistent = true;

                        channel.QueueDeclare($"mongoose_{typeof(T).Name}",
                            true,
                            false,
                            false,
                            null);

                        var json = JsonConvert.SerializeObject(message);
                        var body = Encoding.UTF8.GetBytes(json);

                        channel.TxSelect();
                        channel.BasicPublish("",
                            $"mongoose_{typeof(T).Name}",
                            null,
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