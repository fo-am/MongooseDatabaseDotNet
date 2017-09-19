using System;
using System.Linq;
using System.Threading;

using NLog;

namespace DataPipe.Main
{
    internal class Program
    {
        private static Logger logger;

        private static void Main(string[] args)
        {
            logger = LogManager.GetLogger("console");
            logger.Info("DataPipe trying to start");

            const string mutexId = @"Global\{{7588B7D1-9AC3-4CEF-A199-339EBA4D2571}}";

            bool createdNew;
            using (var mutex = new Mutex(false, mutexId, out createdNew))
            {
                var hasHandle = false;
                try
                {
                    try
                    {
                        hasHandle = mutex.WaitOne(5000, false);
                        if (!hasHandle)
                        {
                            logger.Error("Timeout waiting for exclusive access");
                            throw new TimeoutException("Timeout waiting for exclusive access");
                        }
                    }
                    catch (AbandonedMutexException)
                    {
                        hasHandle = true;
                    }

                    // Perform your work here.
                    PublishData();
                }
                finally
                {
                    if (hasHandle)
                    {
                        mutex.ReleaseMutex();
                    }
                }
            }
        }

        private static void PublishData()
        {
            var numberToSend = 20;
            logger.Info("DataPipe started");
            var send = new Sender();

            foreach (var entity in Data.GetUnsyncedStreamAttribute().Take(numberToSend))
            {
                send.PublishEntity(entity);
                logger.Info($"{entity} {entity.id}");
            }

            foreach (var entity in Data.GetUnsyncedStreamEntity().Take(numberToSend))
            {
                send.PublishEntity(entity);
                logger.Info($"{entity} {entity.entity_id}");
            }

            foreach (var entity in Data.GetUnsyncedStreamValueFile().Take(numberToSend))
            {
                send.PublishEntity(entity);
                logger.Info($"{entity} {entity.entity_id}");
            }

            foreach (var entity in Data.GetUnsyncedStreamValueInt().Take(numberToSend))
            {
                send.PublishEntity(entity);
                logger.Info($"{entity} {entity.entity_id}");
            }

            foreach (var entity in Data.GetUnsyncedStreamValueReal().Take(numberToSend))
            {
                send.PublishEntity(entity);
                logger.Info($"{entity} {entity.entity_id}");
            }

            foreach (var entity in Data.GetUnsyncedStreamValueVarchar().Take(numberToSend))
            {
                send.PublishEntity(entity);
                logger.Info($"{entity} {entity.entity_id}");
            }

            foreach (var entity in Data.GetUnsyncedSyncAttribute().Take(numberToSend))
            {
                send.PublishEntity(entity);
                logger.Info($"{entity} {entity.id}");
            }

            foreach (var entity in Data.GetUnsyncedSyncEntity().Take(numberToSend))
            {
                send.PublishEntity(entity);
                logger.Info($"{entity} {entity.entity_id}");
            }

            foreach (var entity in Data.GetUnsyncedSyncValueFile().Take(numberToSend))
            {
                send.PublishEntity(entity);
                logger.Info($"{entity} {entity.entity_id}");
            }

            foreach (var entity in Data.GetUnsyncedSyncValueInt().Take(numberToSend))
            {
                send.PublishEntity(entity);
                logger.Info($"{entity} {entity.entity_id}");
            }

            foreach (var entity in Data.GetUnsyncedSyncValueReal().Take(numberToSend))
            {
                send.PublishEntity(entity);
                logger.Info($"{entity} {entity.entity_id}");
            }

            foreach (var entity in Data.GetUnsyncedSyncValueVarchar().Take(numberToSend))
            {
                send.PublishEntity(entity);
                logger.Info($"{entity} {entity.entity_id}");
            }

            logger.Info("DataPipe end");
            Environment.Exit(0);
        }
    }
}