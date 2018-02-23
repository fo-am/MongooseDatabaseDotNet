using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DataPipe.Main.Model;
using DataPipe.Main.Model.LifeHistory;
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
          
            foreach (var lifeHistoryEvent in Data.GetLifeHistoryEvents().Take(numberToSend))
            {
                SendEvent(lifeHistoryEvent);

                logger.Info($"{lifeHistoryEvent} UniqueId: {lifeHistoryEvent.UniqueId}");
            }

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

            //foreach (var entity in Data.GetUnsyncedWeights().Take(numberToSend))
            //{
            //    send.PublishEntity(entity);
            //    logger.Info($"{entity} UniqueId: {entity.UniqueId}");
            //}

            //foreach (var entity in Data.GetUnsynced().Take(numberToSend))
            //{
            //    send.PublishEntity(entity);
            //    logger.Info($"{entity} UniqueId: {entity.UniqueId}");
            //}
            logger.Info("DataPipe end");
            Environment.Exit(0);
        }

        private static void SendEvent(LifeHistoryEvent entity)
        {
            var send = new Sender();
            switch (entity.Code)
            {
                case "endgrp":
                    send.PublishEntity((EndPackEvent) entity);
                    break;
                case "lgrp":
                    send.PublishEntity((LostPackEvent) entity);
                    break;
                case "fgrp":
                    send.PublishEntity((FoundPackEvent) entity);
                    break;
                case "unsuccessful":
                    send.PublishEntity((UnsuccessfulLitterEvent) entity);
                    break;
                case "short-lived":
                    send.PublishEntity((ShortLivedLitterEvent) entity);
                    break;
                case "successful":
                    send.PublishEntity((SuccessfulLitterEvent) entity);
                    break;
                case "adied":
                    send.PublishEntity((IndividualAssumedDeadEvent) entity);
                    break;
                case "died":
                    send.PublishEntity((IndividualDiedEvent) entity);
                    break;
                case "lseen":
                    send.PublishEntity((IndividualLastSeenEvent) entity);
                    break;
                case "fseen":
                    send.PublishEntity((IndividualFirstSeenEvent) entity);
                    break;
                case "stev":
                    send.PublishEntity((IndividualStartEvictionEvent) entity);
                    break;
                case "endev":
                    send.PublishEntity((IndividualEndEvictionEvent) entity);
                    break;
                case "leave":
                    send.PublishEntity((IndividualLeaveEvent) entity);
                    break;
                case "return":
                    send.PublishEntity((IndividualReturnEvent) entity);
                    break;
                case "imm":
                    send.PublishEntity((IndividualImmigrateEvent) entity);
                    break;
                case "emm":
                    send.PublishEntity((IndividualEmmigrateEvent) entity);
                    break;
                case "fpreg":
                    send.PublishEntity((IndividualPregnantEvent) entity);
                    break;
                case "abort":
                    send.PublishEntity((IndividualAbortEvent) entity);
                    break;
                case "birth":
                    send.PublishEntity((IndividualBirthEvent) entity);
                    break;
                default:
                    logger.Error($"Code '{entity.Code}' was not found.");
                    break;
            }
        }
    }

}