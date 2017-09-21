using System;
using NLog;

namespace KeepAlive.Sender
{
    public class Program
    {
        private static void Main(string[] args)
        {
            var logger = LogManager.GetLogger("KeepAlive.Sender");
            logger.Info("KeepAlive.Sender starting up");

            var status = new Status
            {
                TimeStampUtc = DateTime.UtcNow,
                AmAlive = true
            };
            var sender = new Sender();
            sender.PublishEntity(status);

            logger.Info("KeepAlive.Sender Exiting");
        }
    }
}