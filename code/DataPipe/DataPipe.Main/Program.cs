using System;
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
            logger.Info("DataPipe started");
            var send = new Sender();

            foreach (var entity in Data.GetUnsyncedStreamAttribute())
            {
                send.PublishEntity(entity);
                logger.Info($"{entity} {entity.id}");
            }

            foreach (var entity in Data.GetUnsyncedStreamEntity())
            {
                send.PublishEntity(entity);
                logger.Info($"{entity} {entity.entity_id}");
            }

            foreach (var entity in Data.GetUnsyncedStreamValueFile())
            {
                send.PublishEntity(entity);
                logger.Info($"{entity} {entity.entity_id}");
            }

            foreach (var entity in Data.GetUnsyncedStreamValueInt())
            {
                send.PublishEntity(entity);
                logger.Info($"{entity} {entity.entity_id}");
            }

            foreach (var entity in Data.GetUnsyncedStreamValueReal())
            {
                send.PublishEntity(entity);
                logger.Info($"{entity} {entity.entity_id}");
            }

            foreach (var entity in Data.GetUnsyncedStreamValueVarchar())
            {
                send.PublishEntity(entity);
                logger.Info($"{entity} {entity.entity_id}");
            }

            foreach (var entity in Data.GetUnsyncedSyncAttribute())
            {
                send.PublishEntity(entity);
                logger.Info($"{entity} {entity.id}");
            }

            foreach (var entity in Data.GetUnsyncedSyncEntity())
            {
                send.PublishEntity(entity);
                logger.Info($"{entity} {entity.entity_id}");
            }

            foreach (var entity in Data.GetUnsyncedSyncValueFile())
            {
                send.PublishEntity(entity);
                logger.Info($"{entity} {entity.entity_id}");
            }

            foreach (var entity in Data.GetUnsyncedSyncValueInt())
            {
                send.PublishEntity(entity);
                logger.Info($"{entity} {entity.entity_id}");
            }

            foreach (var entity in Data.GetUnsyncedSyncValueReal())
            {
                send.PublishEntity(entity);
                logger.Info($"{entity} {entity.entity_id}");
            }

            foreach (var entity in Data.GetUnsyncedSyncValueVarchar())
            {
                send.PublishEntity(entity);
                logger.Info($"{entity} {entity.entity_id}");
            }

            logger.Info("DataPipe end");
            Environment.Exit(0);
        }
    }
}