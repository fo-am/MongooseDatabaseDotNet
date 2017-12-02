using System;
using System.Linq;
using System.Threading;
using DataPipe.Main.Model;
using NLog;
using NLog.Config;

namespace DataPipe.Main
{
    internal class Program
    {
        private static Logger logger;

        private static void Main(string[] args)
        {
            LogManager.Configuration = new XmlLoggingConfiguration("nlog.config");
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
            // look for all entities that are not sent
            // make up objects for each entity
            // queue each object.

            // list of objects
            // new pack
            // delete pack (!) 
            // new individual
            // renamed individual
            // update to an individual(eg adding sex)

            // when we send a new object we need to mark in the database which parts have been sent (setting sent = 1 on each row)
            // so we need to gather that information when we construct the object... not sure hwo to do this.

            //foreach (var entity in Data.GetNewPacks().Take(numberToSend))
            //{
            //    send.PublishEntity(entity);
            //    logger.Info($"{entity} UniqueId: {entity.UniqueId}");
            //}

            //foreach (var entity in Data.GetUnsyncedIndividuals().Take(numberToSend))
            //{
            //    send.PublishEntity(entity);
            //    logger.Info($"{entity} UniqueId: {entity.UniqueId}");
            //}

            foreach (var entity in Data.GetUnsyncedWeights().Take(numberToSend))
            {
                send.PublishEntity(entity);
                logger.Info($"{entity} UniqueId: {entity.UniqueId}");
            }

            logger.Info("DataPipe end");
            Environment.Exit(0);
        }
    }
}