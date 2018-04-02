using System;
using System.Linq;
using System.Threading;

using AutoMapper;

using DataPipe.Main.Model;
using DataPipe.Main.Model.LifeHistory;

using NLog;
using NLog.Config;

namespace DataPipe.Main
{
    public class Program
    {
        private static Logger logger;

        private static void Main(string[] args)
        {
            LogManager.Configuration = new XmlLoggingConfiguration("nlog.config");
            logger = LogManager.GetLogger("console");
            logger.Info("DataPipe trying to start v1.91");

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
                    try
                    {
                        PublishData();
                    }
                    catch (Exception e)
                    {
                        logger.Error(e, "Exception trying to publish data");
                        throw;
                    }
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
            var numberToSend = 2000;
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
            logger.Info($"Number to send '{numberToSend}'");
            var send = new Sender();

            foreach (var entity in Data.GetNewPacks().Take(numberToSend))
            {
                send.PublishEntity(entity);
                logger.Info($"{entity} UniqueId: {entity.UniqueId}");
            }

            foreach (var entity in Data.GetUnsyncedIndividuals().Take(numberToSend))
            {
                send.PublishEntity(entity);
                logger.Info($"{entity} UniqueId: {entity.UniqueId}");
            }

            foreach (var entity in Data.GetUnsynchedLitters().Take(numberToSend))
            {
                send.PublishEntity(entity);
                logger.Info($"{entity} UniqueId: {entity.UniqueId}");
            }

            foreach (var entity in Data.GetUnsyncedWeights().Take(numberToSend))
            {
                send.PublishEntity(entity);
                logger.Info($"{entity} UniqueId: {entity.UniqueId}");
            }

            foreach (var lifeHistoryEvent in Data.GetLifeHistoryEvents().Take(numberToSend))
            {
                SendEvent(lifeHistoryEvent);

                logger.Info($"{lifeHistoryEvent} UniqueId: {lifeHistoryEvent.UniqueId}");
            }

            foreach (var entity in Data.GetUnsynchedPackMoves().Take(numberToSend))
            {
                send.PublishEntity(entity);
                logger.Info($"{entity} UniqueId: {entity.UniqueId}");
            }

            foreach (var entity in Data.GetUnsynchedInterGroupInteractions().Take(numberToSend))
            {
                send.PublishEntity(entity);
                logger.Info($"{entity} UniqueId: {entity.UniqueId}");
            }

            foreach (var entity in Data.GetUnsynchedGroupAlarms().Take(numberToSend))
            {
                send.PublishEntity(entity);
                logger.Info($"{entity} UniqueId: {entity.UniqueId}");
            }

            foreach (var entity in Data.GetUnsynchedGroupMoves().Take(numberToSend))
            {
                send.PublishEntity(entity);
                logger.Info($"{entity} UniqueId: {entity.UniqueId}");
            }

            foreach (var entity in Data.GetUnsynchedOesturus().Take(numberToSend))
            {
                send.PublishEntity(entity);
                logger.Info($"{entity} UniqueId: {entity.UniqueId}");
            }

            logger.Info("DataPipe end");
            
            Environment.Exit(0);
        }

        private static void SendEvent(LifeHistoryEvent entity)
        {
            LogManager.Configuration = new XmlLoggingConfiguration("nlog.config");
            var logger = LogManager.GetLogger("Data");

            logger.Info($"Sending {entity.Code} for {entity.entity_name}");
            var send = new Sender();

            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<LifeHistoryEvent, EndPackEvent>();
                cfg.CreateMap<LifeHistoryEvent, LostPackEvent>();
                cfg.CreateMap<LifeHistoryEvent, FoundPackEvent>();
                cfg.CreateMap<LifeHistoryEvent, UnsuccessfulLitterEvent>();
                cfg.CreateMap<LifeHistoryEvent, ShortLivedLitterEvent>();
                cfg.CreateMap<LifeHistoryEvent, SuccessfulLitterEvent>();
                cfg.CreateMap<LifeHistoryEvent, IndividualAssumedDeadEvent>();
                cfg.CreateMap<LifeHistoryEvent, IndividualDiedEvent>();
                cfg.CreateMap<LifeHistoryEvent, IndividualLastSeenEvent>();
                cfg.CreateMap<LifeHistoryEvent, IndividualFirstSeenEvent>();
                cfg.CreateMap<LifeHistoryEvent, IndividualStartEvictionEvent>();
                cfg.CreateMap<LifeHistoryEvent, IndividualEndEvictionEvent>();
                cfg.CreateMap<LifeHistoryEvent, IndividualLeaveEvent>();
                cfg.CreateMap<LifeHistoryEvent, IndividualReturnEvent>();
                cfg.CreateMap<LifeHistoryEvent, IndividualImmigrateEvent>();
                cfg.CreateMap<LifeHistoryEvent, IndividualEmmigrateEvent>();
                cfg.CreateMap<LifeHistoryEvent, IndividualPregnantEvent>();
                cfg.CreateMap<LifeHistoryEvent, IndividualAbortEvent>();
                cfg.CreateMap<LifeHistoryEvent, IndividualBirthEvent>();
            });

            var mapper = config.CreateMapper();

            switch (entity.Code)
            {
                case "endgrp":
                    send.PublishEntity(mapper.Map<LifeHistoryEvent, EndPackEvent>(entity));
                    break;
                case "lgrp":
                    send.PublishEntity(mapper.Map<LifeHistoryEvent, LostPackEvent>(entity));
                    break;
                case "fgrp":
                    send.PublishEntity(mapper.Map<LifeHistoryEvent, FoundPackEvent>(entity));
                    break;
                case "unsuccessful":
                    send.PublishEntity(mapper.Map<LifeHistoryEvent, UnsuccessfulLitterEvent>(entity));
                    break;
                case "short-lived":
                    send.PublishEntity(mapper.Map<LifeHistoryEvent, ShortLivedLitterEvent>(entity));
                    break;
                case "successful":
                    send.PublishEntity(mapper.Map<LifeHistoryEvent, SuccessfulLitterEvent>(entity));
                    break;
                case "adied":
                    send.PublishEntity(mapper.Map<LifeHistoryEvent, IndividualAssumedDeadEvent>(entity));
                    break;
                case "died":
                    send.PublishEntity(mapper.Map<LifeHistoryEvent, IndividualDiedEvent>(entity));
                    break;
                case "lseen":
                    send.PublishEntity(mapper.Map<LifeHistoryEvent, IndividualLastSeenEvent>(entity));
                    break;
                case "fseen":
                    send.PublishEntity(mapper.Map<LifeHistoryEvent, IndividualFirstSeenEvent>(entity));
                    break;
                case "stev":
                    send.PublishEntity(mapper.Map<LifeHistoryEvent, IndividualStartEvictionEvent>(entity));
                    break;
                case "endev":
                    send.PublishEntity(mapper.Map<LifeHistoryEvent, IndividualEndEvictionEvent>(entity));
                    break;
                case "leave":
                    send.PublishEntity(mapper.Map<LifeHistoryEvent, IndividualLeaveEvent>(entity));
                    break;
                case "return":
                    send.PublishEntity(mapper.Map<LifeHistoryEvent, IndividualReturnEvent>(entity));
                    break;
                case "imm":
                    send.PublishEntity(mapper.Map<LifeHistoryEvent, IndividualImmigrateEvent>(entity));
                    break;
                case "emm":
                    send.PublishEntity(mapper.Map<LifeHistoryEvent, IndividualEmmigrateEvent>(entity));
                    break;
                case "fpreg":
                    send.PublishEntity(mapper.Map<LifeHistoryEvent, IndividualPregnantEvent>(entity));
                    break;
                case "abort":
                    send.PublishEntity(mapper.Map<LifeHistoryEvent, IndividualAbortEvent>(entity));
                    break;
                case "birth":
                    send.PublishEntity(mapper.Map<LifeHistoryEvent, IndividualBirthEvent>(entity));
                    break;
                default:
                    logger.Error($"Code '{entity.Code}' was not found.");
                    break;
            }
        }
    }
}