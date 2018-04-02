﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using Dapper;

using DataReciever.Main.Model;
using DataReciever.Main.Model.LifeHistory;
using DataReciever.Main.Model.Oestrus;
using DataReciever.Main.Model.PupFocal;

using NLog;

using Npgsql;

namespace DataReciever.Main.Data
{
    public class PgRepository
    {
        private static readonly Logger logger = LogManager.GetLogger("PgRepository");

        public static int StoreMessage(string fullName, string message, string messageId)
        {
            logger.Info($"Stored message type '{fullName}' with Id '{messageId}'");
            using (IDbConnection conn = new NpgsqlConnection(GetAppSettings.Get().PostgresConnection))
            {
                return conn.ExecuteScalar<int>(
                    @"Insert into mongoose.event_log (type, message_id, object)
                     values (@type, @message_id, @message::json)
                      ON CONFLICT(message_id) DO UPDATE SET delivered_count = event_log.delivered_count + 1
                     RETURNING event_log_id",
                    new
                    {
                        type = fullName,
                        message_id = messageId,
                        message
                    });
            }
        }

        public static void MessageHandledOk(int entityId)
        {
            logger.Info($"Message handled ok.");
            using (IDbConnection conn = new NpgsqlConnection(GetAppSettings.Get().PostgresConnection))
            {
                conn.Execute("update mongoose.event_log set success = true where event_log_id = @entityId",
                    new { entityId });
            }
        }

        public static int FailedToHandleMessage(int entityId, Exception ex)
        {
            logger.Error(ex, "Failed to handle message.");
            using (IDbConnection conn = new NpgsqlConnection(GetAppSettings.Get().PostgresConnection))
            {
                return conn.ExecuteScalar<int>(
                    "update mongoose.event_log set success = false, error = @error where event_log_id = @entityId returning delivered_count",
                    new { entityId, error = ex.ToString() });
            }
        }

        public void InsertNewPack(PackCreated message)
        {
            logger.Info($@"Add new pack: ""{message.Name}"".");
            using (IDbConnection conn = new NpgsqlConnection(GetAppSettings.Get().PostgresConnection))
            {
                conn.Open();
                using (var tr = conn.BeginTransaction())
                {
                    var packId = InsertPack(message.Name, message.UniqueId, conn, message.CreatedDate);
                    CreatePackEvent(packId, message.CreatedDate, "ngrp", conn);
                    tr.Commit();
                }
            }
        }

        public void InsertNewWeight(WeightMeasure message)
        {
            logger.Info($"Add new weight for '{message.IndividualName}'. Weight = '{message.Weight}'");

            // get a packid
            // get a individualid
            // get a pack history id

            // add the weight info

            using (IDbConnection conn = new NpgsqlConnection(GetAppSettings.Get().PostgresConnection))
            {
                conn.Open();
                using (var tr = conn.BeginTransaction())
                {
                    var packId = InsertPack(message.PackId, message.PackUniqueId, conn);
                    var individualId =
                        InsertIndividual(
                            new IndividualCreated
                            {
                                PackCode = message.PackId,
                                PackUniqueId = message.PackUniqueId,
                                Name = message.IndividualName,
                                UniqueId = message.IndividualUniqueId
                            }, null, conn);

                    var packHistoryId = InsertPackHistory(packId, individualId, message.Time, conn);

                    var loc = GetLocationString(message.Latitude, message.Longitude);

                    conn.ExecuteScalar<int>(
                        $"Insert into mongoose.weight (pack_history_id, weight, time, accuracy, collar_weight, location) values (@pack_history_id, @weight, @time, @accuracy, @collar_weight, {loc}) RETURNING weight_id",
                        new
                        {
                            pack_history_id = packHistoryId,
                            weight = message.Weight,
                            time = message.Time,
                            accuracy = message.Accurate,
                            collar_weight = message.CollarWeight
                        });

                    tr.Commit();
                }
            }
        }

        private string GetLocationString(double? latitude, double? longitude)
        {
            var locationString = "NULL";
            if (latitude.HasValue && longitude.HasValue)
            {
                locationString =
                    $"ST_GeographyFromText('SRID=4326;POINT({latitude} {longitude})')";
            }

            return locationString;
        }

        public void InsertNewIndividual(IndividualCreated message)
        {
            logger.Info($"Add new individual '{message.Name}'.");

            using (IDbConnection conn = new NpgsqlConnection(GetAppSettings.Get().PostgresConnection))
            {
                conn.Open();
                using (var tr = conn.BeginTransaction())
                {
                    var packId = InsertPack(message.PackCode, message.PackUniqueId, conn);
                    var litterId = InsertLitter(message.LitterCode, packId, conn);
                    var individualId = InsertIndividual(message, litterId, conn);
                var packhistory =    InsertPackHistory(packId, individualId, message.DateOfBirth, conn);

                    if (message.DateOfBirth.HasValue)
                    {
                        CreateIndividualEvent(packhistory, message.DateOfBirth, "born", conn);
                        if (litterId.HasValue)
                        {
                            if (LitterIsNotBorn(litterId.Value, conn))
                            {
                                CreateLitterEvent(litterId.Value, message.DateOfBirth.Value, "born", conn);
                            }
                        }
                    }
                    else
                    {
                        CreateIndividualEvent(packhistory, DateTime.UtcNow, "fseen", conn);
                    }

                    tr.Commit();
                }
            }
        }

        private bool LitterIsNotBorn(int litterId, IDbConnection conn)
        {
            var litterName = conn.Query<string>(@"SELECT l.name
	                                    FROM mongoose.litter l
                                        join mongoose.litter_event le on  le.litter_id = l.litter_id
                                        join mongoose.litter_event_code lec on lec.litter_event_code_id = le.litter_event_code_id
                                        where lec.code = 'born' and l.litter_id = @litterId", new
            {
                litterId
            });

            return string.IsNullOrEmpty(litterName.FirstOrDefault());
        }

        private int InsertPack(string packName, string packUniqueId, IDbConnection conn,
            DateTime? packCreatedDate = null)
        {
            var packId = TryGetPackId(packName, conn);

            if (packCreatedDate.HasValue && packCreatedDate.Value == DateTime.MinValue)
            {
                packCreatedDate = null;
            }

            return packId ?? conn.ExecuteScalar<int>(
                       "Insert into mongoose.pack (name, unique_id, pack_created_date) values (@name, @unique_id, @pack_created_date) RETURNING pack_id",
                       new { name = packName, unique_id = packUniqueId, pack_created_date = packCreatedDate });
        }

        private static int? TryGetPackId(string packName, IDbConnection conn)
        {
            var packId = conn.ExecuteScalar<int?>("select pack_id from mongoose.pack where name = @name",
                new { name = packName });
            return packId;
        }

        private int InsertIndividual(IndividualCreated message, int? litterId, IDbConnection conn)
        {
            var individualId = TryGetIndividualId(message.Name, conn);

            // if we have an individual then look at its data and see if we can add some more

            return individualId ?? AddIndividual(message.Name, message.Gender, message.ChipCode, message.UniqueId,
                       litterId, conn);
        }

        private static int AddIndividual(string name, string gender, string chipcode, string uniqueId, int? litterId,
            IDbConnection conn)
        {
            return conn.ExecuteScalar<int>(
                "Insert into mongoose.individual (name, sex, litter_id, transponder_id, unique_id) values (@name, @sex, @litter_id, @transponder_id, @unique_id) RETURNING individual_id",
                new
                {
                    name,
                    sex = gender,
                    litter_id = litterId,
                    transponder_id = chipcode,
                    unique_id = uniqueId
                });
        }

        private static int? TryGetIndividualId(string name, IDbConnection conn)
        {
            if (string.IsNullOrEmpty(name))
            {
                name = "Unknown";
            }

            var individualId = conn.ExecuteScalar<int?>(
                "select individual_id from mongoose.individual where name = @name",
                new { name });
            return individualId;
        }

        private int InsertPackHistory(int packId, int individualId, DateTime? dateOfInteraction, IDbConnection conn)
        {
            // get a pack history by ids.
            int? packHistoryId = null;
            var packHistory = conn.Query(
                "select pack_history_id, date_joined from mongoose.pack_history where pack_id = @packId and individual_id = @individualId",
                new { packId, individualId }).FirstOrDefault();
            // if one exists
            if (packHistory?.pack_history_id != null)
            {
                packHistoryId = packHistory.pack_history_id;

                if (packHistory.date_joined == null || dateOfInteraction < packHistory.date_joined)
                {
                    conn.Execute("Update mongoose.pack_history set date_joined = @date_joined",
                        new { date_joined = dateOfInteraction });
                }
            }

            // if no pack history then add one.
            return packHistoryId ?? conn.ExecuteScalar<int>(
                       "Insert into mongoose.pack_history (pack_id, individual_id, date_joined) values (@pack_id, @individual_id, @date_joined) RETURNING pack_history_id",
                       new { pack_id = packId, individual_id = individualId, date_joined = dateOfInteraction });
        }

        private int? InsertLitter(string litterName, int packId, IDbConnection conn)
        {
            if (string.IsNullOrEmpty(litterName))
            {
                return null;
            }

            var litterId = TryGetLitterId(litterName, conn);

            return litterId ?? conn.ExecuteScalar<int>(
                       "Insert into mongoose.litter (name, pack_id) values (@name, @pack_id) RETURNING litter_id",
                       new { name = litterName, pack_id = packId });
        }


        public void PackEvent(LifeHistoryEvent message)
        {
            logger.Info($@"{message.GetType().Name} Event for pack: ""{message.entity_name}"".");
            using (IDbConnection conn = new NpgsqlConnection(GetAppSettings.Get().PostgresConnection))
            {
                conn.Open();
                using (var tr = conn.BeginTransaction())
                {
                    var packId = TryGetPackId(message.entity_name, conn);

                    if (!packId.HasValue)
                    {
                        packId = InsertPack(message.entity_name, message.UniqueId, conn);
                    }

                    //insert a packo evento
                    CreatePackEvent(packId.Value, message.Date, message.Code, conn);

                    tr.Commit();
                }
            }
        }

        private void CreatePackEvent(int packId, DateTime messageDate, string code, IDbConnection conn)
        {
            logger.Info($"Adding '{code}' event for pack '{packId}'");

            var eventId = conn.ExecuteScalar<int>(
                @"SELECT pack_event_code_id FROM mongoose.pack_event_code where code = @code",
                new
                {
                    code
                });

            conn.Execute(@"INSERT INTO mongoose.pack_event
                            (pack_id, pack_event_code_id, date)
	                        VALUES (@pack_id, @pack_event_code_id, @date);",
                new
                {
                    pack_id = packId,
                    pack_event_code_id = eventId,
                    date = messageDate
                });
        }


        public void NewIndividualEvent(LifeHistoryEvent message)
        {
            logger.Info($@"{message.GetType().Name} Event for Individual: ""{message.entity_name}"".");
            using (IDbConnection conn = new NpgsqlConnection(GetAppSettings.Get().PostgresConnection))
            {
                conn.Open();
                using (var tr = conn.BeginTransaction())
                {
                    var individiualId = TryGetIndividualId(message.entity_name, conn);

                    if (!individiualId.HasValue)
                    {
                        individiualId = AddIndividual(message.entity_name, null, null, message.UniqueId, null, conn);
                    }

                    int packHistoryId = GetPackHistoryId(individiualId.Value, conn);
                    //insert a individual event
                    CreateIndividualEvent(packHistoryId, message.Date, message.Code, conn);

                    tr.Commit();
                }
            }
        }

        private int GetPackHistoryId(int individiualId, IDbConnection conn)
        {
            var packHistoryId = TryGetPackHistoryId(individiualId, conn);
            if (packHistoryId.HasValue)
            {
                return packHistoryId.Value;
            }

            var unknownPackId = conn.ExecuteScalar<int>(@"select pack_id from mongoose.pack where pack.name = 'Unknown'");

            packHistoryId = conn.ExecuteScalar<int>(@"INSERT INTO mongoose.pack_history(
	                                        pack_id, individual_id, date_joined)
	                                        VALUES ( @unknownPackId, @individiualId, @date) RETURNING pack_history_id;", new
            {
                unknownPackId,
                individiualId,
                date = DateTime.UtcNow
            });

            return packHistoryId.Value;
        }

        private int? TryGetPackHistoryId(int? individiualId, IDbConnection conn)
        {
            logger.Info($"Getting Pack History Id for individuaulId '{individiualId}'.");

            var packHistoryId = conn.ExecuteScalar<int?>(@"select pack_history_id from mongoose.pack_history
                                                    where pack_history.individual_id = @individualId 
                                                    order by date_joined desc NULLS LAST
                                                    limit 1"
                ,
                new
                {
                    individualId = individiualId
                });

            return packHistoryId;
        }

        private void CreateIndividualEvent(int packHistoryId, DateTime? messageDate, string code, IDbConnection conn)
        {

            logger.Info($"Adding '{code}' event for pack history '{packHistoryId}'.");

            var eventId = conn.ExecuteScalar<int>(
                @"SELECT individual_event_code_id FROM mongoose.individual_event_code where code = @code",
                new
                {
                    code
                });

            conn.Execute(@"INSERT INTO mongoose.individual_event
                            (pack_history_id, individual_event_code_id, date)
	                        VALUES (@pack_history_id, @individual_event_code_id, @date);",
                new
                {
                    pack_history_id = packHistoryId,
                    individual_event_code_id = eventId,
                    date = messageDate
                });

        }

        public void InsertNewLitterEvent(LifeHistoryEvent message)
        {
            logger.Info($@"{message.GetType().Name} Event for litter: ""{message.entity_name}"".");
            using (IDbConnection conn = new NpgsqlConnection(GetAppSettings.Get().PostgresConnection))
            {
                conn.Open();
                using (var tr = conn.BeginTransaction())
                {
                    var litterId = TryGetLitterId(message.entity_name, conn);

                    if (!litterId.HasValue)
                    {
                        throw new Exception($"No Litter Found with Id {message.entity_name}");
                    }

                    //insert a litter evento
                    CreateLitterEvent(litterId.Value, message.Date, message.Code, conn);

                    tr.Commit();
                }
            }
        }

        private void CreateLitterEvent(int litterId, DateTime messageDate, string code, IDbConnection conn)
        {
            logger.Info($"Adding '{code}' event for Litter '{litterId}'");

            var eventId = conn.ExecuteScalar<int>(
                @"SELECT litter_event_code_id FROM mongoose.litter_event_code where code = @code",
                new
                {
                    code
                });

            conn.Execute(@"INSERT INTO mongoose.litter_event
                            (litter_id, litter_event_code_id, last_seen)
	                        VALUES (@litter_id, @litter_event_code_id, @date);",
                new
                {
                    litter_id = litterId,
                    litter_event_code_id = eventId,
                    date = messageDate
                });
        }

        private static int? TryGetLitterId(string litterName, IDbConnection conn)
        {
            var litterId = conn.ExecuteScalar<int?>("select litter_id from mongoose.litter where name = @name",
                new { name = litterName });
            return litterId;
        }

        public void InsertNewLitter(LitterCreated message)
        {
            // get pack id
            // see if litter exists. if so log it and move on
            // if not then add it.
            logger.Info($@"Adding New Litter : ""{message.LitterName}"".");
            using (IDbConnection conn = new NpgsqlConnection(GetAppSettings.Get().PostgresConnection))
            {
                conn.Open();
                using (var tr = conn.BeginTransaction())
                {
                    var packId = TryGetPackId(message.PackName, conn);
                    if (!packId.HasValue)
                    {
                        throw new Exception($"Pack Name '{message.PackName}' not found.");
                    }

                    var litterId = TryGetLitterId(message.LitterName, conn);

                    if (litterId.HasValue)
                    {
                        logger.Info($"Litter already in database!  {message.LitterName}");
                        return;
                    }

                    //insert a packo evento
                    CreateLitter(message.LitterName, packId.Value, message.Date, conn);

                    tr.Commit();
                }
            }
        }

        private void CreateLitter(string litterName, int packId, DateTime dateFormed, IDbConnection conn)
        {

            conn.Execute(@"INSERT INTO mongoose.litter
                            (pack_id, name, dateformed) 
                            VALUES 
                            (@pack_id, @name, @dateformed);",
                new
                {
                    pack_id = packId,
                    name = litterName,
                    dateformed = dateFormed
                });
        }

        public void PackMove(PackMove message)
        {
            //get inital packId
            // get destination packId
            // get individual Id

            // move the individual to the new pack.
            logger.Info(
                $@"Moving '{message.MongooseName}' from Pack '{message.PackSourceName}' to Pack '{message.PackDestintionName}'.");
            using (IDbConnection conn = new NpgsqlConnection(GetAppSettings.Get().PostgresConnection))
            {
                conn.Open();
                using (var tr = conn.BeginTransaction())
                {
                    var sourcePackId = TryGetPackId(message.PackSourceName, conn);
                    if (!sourcePackId.HasValue)
                    {
                        throw new Exception($"Source Pack Name '{message.PackSourceName}' not found.");
                    }

                    var destinationPackId = TryGetPackId(message.PackDestintionName, conn);
                    if (!destinationPackId.HasValue)
                    {
                        throw new Exception($"Destination Pack Name '{message.PackDestintionName}' not found.");
                    }

                    var individualId = TryGetIndividualId(message.MongooseName, conn);
                    //insert a packo evento
                    MovePack(sourcePackId, destinationPackId, individualId, message.Time, conn);

                    tr.Commit();
                }
            }
        }

        private void MovePack(int? sourcePackId, int? destinationPackId, int? individualId, DateTime date, IDbConnection conn)
        {
            conn.Execute(@"INSERT INTO mongoose.pack_history(
	                     pack_id, individual_id, date_joined)
	                    VALUES (@pack_id, @individual_id, @date_joined);",
                new
                {
                    pack_id = destinationPackId,
                    individual_id = individualId,
                    date_joined = date
                });
        }

        public void HandleInterGroupInteraction(InterGroupInteractionEvent message)
        {
            logger.Info(
                $@"Inter Group Event between '{message.packName}' and '{message.otherPackName}'.");
            using (IDbConnection conn = new NpgsqlConnection(GetAppSettings.Get().PostgresConnection))
            {
                conn.Open();
                using (var tr = conn.BeginTransaction())
                {
                    var focalPackId = TryGetPackId(message.packName, conn);
                    if (!focalPackId.HasValue)
                    {
                        throw new Exception($"Focal Pack Name '{message.packName}' not found.");
                    }

                    var secondPackId = TryGetPackId(message.otherPackName, conn);
                    if (!secondPackId.HasValue)
                    {
                        throw new Exception($"Second pack name '{message.otherPackName}' not found.");
                    }

                    var leaderId = TryGetIndividualId(message.leaderName, conn);

                    var outcomeId = GetOutcomeId(message.outcome, conn);

                    //insert a packo evento
                    RecordInterGroupInteraction(focalPackId, secondPackId, leaderId,outcomeId, message.time,message.latitude, message.longitude, conn);

                    tr.Commit();
                }
            }
        }

        private int GetOutcomeId(string outcome, IDbConnection conn)
        {
            var outcomeId = conn.ExecuteScalar<int?>(
                  @"Select interaction_outcome_id 
                    from mongoose.interaction_outcome 
                    where outcome = @outcome;",
                new
                {
                    outcome
                });

            return outcomeId ?? throw new Exception($"Outcome '{outcome}' not found in database");
        }

        private void RecordInterGroupInteraction(int? focalPackId, int? secondPackId, int? leaderId, object outcomeId,
            DateTime time, double latitude, double longitude, IDbConnection conn)
        {
            var loc = GetLocationString(latitude, longitude);

            conn.Execute($@"INSERT INTO mongoose.inter_group_interaction(
	                       focalpack_id, secondpack_id, leader_individual_id,  interaction_outcome_id, ""time"", location)
	                       VALUES ( @focalpack_id, @secondpack_id, @leader_individual_id, @interaction_outcome_id, @time, {loc});",
                new
                {
                    focalpack_id = focalPackId,
                    secondpack_id = secondPackId,
                    interaction_outcome_id = outcomeId,
                    leader_individual_id = leaderId,
                    time = time
                });
        }

        public void InsertGroupAlarm(GroupAlarmEvent message)
        {
            logger.Info(
                $@"Group Alarm pack:'{message.packName}' type: '{message.cause}'.");
            using (IDbConnection conn = new NpgsqlConnection(GetAppSettings.Get().PostgresConnection))
            {
                conn.Open();
                using (var tr = conn.BeginTransaction())
                {
                    var packId = TryGetPackId(message.packName, conn);
                    if (!packId.HasValue)
                    {
                        throw new Exception($"Pack Name '{message.packName}' not found.");
                    }

                    var callerId = TryGetIndividualId(message.callerName, conn);

                    var causeId = GetCauseId(message.cause, conn);

                    RecordGroupAlarm(packId, causeId, callerId, message.time,message.othersJoin, message.Latitude, message.Longitude, conn);

                    tr.Commit();
                }
            }
        }

        private void RecordGroupAlarm(int? packId, int causeId, int? callerId, DateTime time, string othersJoin, double latitude,
            double longitude,
            IDbConnection conn)
        {
            var loc = GetLocationString(latitude, longitude);

            conn.Execute($@"INSERT INTO mongoose.alarm(
	                         date, pack_id, caller_individual_id, alarm_cause_id, others_join, location)
	                        VALUES (@date, @pack_id, @caller_individual_id, @alarm_cause_id, @others_join, {loc});",
                new
                {
                    date = time,
                    pack_id = packId,
                    caller_individual_id = callerId,
                    alarm_cause_id = causeId,
                    others_join = othersJoin
                });

        }

        private int GetCauseId(string cause, IDbConnection conn)
        {
            var causeId = conn.ExecuteScalar<int?>(
                @"Select alarm_cause_id 
                    from mongoose.alarm_cause 
                    where cause = @cause;",
                new
                {
                    cause
                });

            return causeId ?? throw new Exception($"Cause '{cause}' not found in database");
        }

        public void InsertNewGroupMoveEvent(GroupMoveEvent message)
        {
            logger.Info(
                $@"Group move pack:'{message.pack}' '{message.Direction}' '{message.Destination}'.");

            using (IDbConnection conn = new NpgsqlConnection(GetAppSettings.Get().PostgresConnection))
            {
                conn.Open();
                using (var tr = conn.BeginTransaction())
                {
                    var packId = TryGetPackId(message.pack, conn);
                    if (!packId.HasValue)
                    {
                        throw new Exception($"Pack Name '{message.pack}' not found.");
                    }

                    var leaderId = TryGetIndividualId(message.leader, conn);

                    var destinationId = GetDestinationId(message.Destination, conn);

                    RecordGroupMove(packId, destinationId, leaderId, message.Time, message.Direction, message.HowMany,
                        message.Depth, message.Width, message.latitude, message.longitude, conn);

                    tr.Commit();
                }
            }
        }

        private void RecordGroupMove(int? packId, int destinationId, int? leaderId, DateTime time, string direction, int? howMany,
            int? depth, int? width, double latitude, double longitude, IDbConnection conn)
        {
            var loc = GetLocationString(latitude, longitude);

            conn.Execute($@"INSERT INTO mongoose.pack_move(
	                        pack_id, leader_individual_id, pack_move_destination_id, direction, time, width, depth, number_of_individuals, location)
	                        VALUES (@pack_id, @leader_individual_id, @pack_move_destination_id, @direction, @time, @width, @depth, @number_of_individuals, {
                        loc
                    });",
                new
                {
                    pack_id = packId,
                    leader_individual_id = leaderId,
                    pack_move_destination_id = destinationId,
                    direction = direction,
                    time = time,
                    width = width,
                    depth = depth,
                    number_of_individuals = howMany,
                });
        }

        private int GetDestinationId(string destination, IDbConnection conn)
        {
            var destinationId = conn.ExecuteScalar<int?>(
                @"Select pack_move_destination_id
                    from mongoose.pack_move_destination
                    where destination = @destination;",
                new
                {
                    destination = destination
                });

            return destinationId ?? throw new Exception($"Destination '{destination}' not found in database");
        }

        public void InsertOestrusEvent(OestrusEvent message)
        {
            logger.Info(
                $@"Oestrus '{message.packName}' Individual '{message.focalIndividualName}'.");
            using (IDbConnection conn = new NpgsqlConnection(GetAppSettings.Get().PostgresConnection))
            {
                conn.Open();
                using (var tr = conn.BeginTransaction())
                {
                    var packId = TryGetPackId(message.packName, conn);
                    if (!packId.HasValue)
                    {
                        throw new Exception($"Source Pack Name '{message.packName}' not found.");
                    }

                    var individualId = TryGetIndividualId(message.focalIndividualName, conn);
                    if (!individualId.HasValue)
                    {
                        throw new Exception($"Individual Name '{message.focalIndividualName}' not found.");
                    }

                    //insert a event
                    var oestrusEventId = InsertOestrus(packId.Value, individualId.Value, message.depth, message.visibleIndividuals,
                        message.width, message.time, message.latitude, message.longitude, conn);
                    InsertAggression(message.AggressionEventList, oestrusEventId, conn);
                    InsertAffiliation(message.AffiliationEventList, oestrusEventId, conn);
                    InsertMaleAggresssion(message.MaleAggressionList, oestrusEventId, conn);
                    InsertMate(message.MateEventList, oestrusEventId, conn);
                    InsertNearest(message.NearestList, oestrusEventId, conn);


                    tr.Commit();
                }
            }
        }

        private void InsertNearest(List<OestrusNearest> nearestList, int oestrusEventId, IDbConnection conn)
        {
            foreach (var oestrusNearest in nearestList)
            {
                // var loc = GetLocationString(latitude, longitude);


                //todo: everywhere I look for an individual look in the ID to see if it is unknown or nothing.
                var individualId = TryGetIndividualId(oestrusNearest.nearestIndividualName, conn);
                if (!individualId.HasValue)
                {
                    throw new Exception($"individual Name '{oestrusNearest.nearestIndividualName}' not found.");
                }

                conn.Execute(@"INSERT INTO mongoose.oestrus_nearest(
	                            oestrus_event_id, nearest_individual_id, close_individuals, time)
	                            VALUES (@oestrus_event_id, @nearest_individual_id, @close_individuals, @time);",
                    new
                    {
                        oestrus_event_id = oestrusEventId,
                        nearest_individual_id = individualId,
                        close_individuals = string.Join(",", oestrusNearest.CloseListNames),
                        time = oestrusNearest.scanTime
                    });
            }
        }

        private void InsertMate(List<OestrusMateEvent> mateEventList, int oestrusEventId, IDbConnection conn)
        {    
            // TODO: and make lists of options.
            foreach (var oestrusMateEvent in mateEventList)
            {

                var loc = GetLocationString(oestrusMateEvent.latitude, oestrusMateEvent.longitude);

                var withIndividualId = TryGetIndividualId(oestrusMateEvent.withIndividualName, conn);
                if (!withIndividualId.HasValue)
                {
                    throw new Exception($"individual Name '{oestrusMateEvent.withIndividualName}' not found.");
                }

                conn.Execute($@"INSERT INTO mongoose.oestrus_mating(
	                             oestrus_event_id, with_individual_id, behaviour, female_response, male_response, success, time, location)
	                            VALUES ( @oestrus_event_id, @with_individual_id, @behaviour, @female_response, @male_response, @success, @time, {loc});",
                    new
                    {
                        oestrus_event_id = oestrusEventId,
                        with_individual_id = withIndividualId,
                        behaviour = oestrusMateEvent.behaviour,
                        female_response = oestrusMateEvent.femaleResponse,
                        male_response = oestrusMateEvent.maleResponse,
                        success = oestrusMateEvent.success,
                        time = oestrusMateEvent.time
                    });
            }
        }

        private void InsertMaleAggresssion(List<OestrusMaleAggression> maleAggressionList, int oestrusEventId, IDbConnection conn)
        {
            foreach (var oestrusMaleAggression in maleAggressionList)
            {
                var loc = GetLocationString(oestrusMaleAggression.latitude, oestrusMaleAggression.longitude);

                var initiatorIndividualId = TryGetIndividualId(oestrusMaleAggression.initiatorIndividualName, conn);
                if (!initiatorIndividualId.HasValue)
                {
                    throw new Exception($"individual Name '{oestrusMaleAggression.initiatorIndividualName}' not found.");
                }

                var recieverIndividualId = TryGetIndividualId(oestrusMaleAggression.receiverIndividualName, conn);
                if (!recieverIndividualId.HasValue)
                {
                    throw new Exception($"individual Name '{oestrusMaleAggression.receiverIndividualName}' not found.");
                }

                conn.Execute($@"INSERT INTO mongoose.oestrus_male_aggression(
	                oestrus_event_id, initiator_individual_id, reciever_individual_id, level, winner, owner, time, location)
	                VALUES (@oestrus_event_id, @initiator_individual_id, @reciever_individual_id, @level, @winner, @owner, @time, {loc});",
                    new
                    {
                        oestrus_event_id = oestrusEventId,
                        initiator_individual_id = initiatorIndividualId,
                        reciever_individual_id = recieverIndividualId,
                        level = oestrusMaleAggression.level,
                        winner = oestrusMaleAggression.winner,
                        owner = oestrusMaleAggression.owner,
                        time = oestrusMaleAggression.time

                    });
            }
        }

        private void InsertAffiliation(List<OestrusAffiliationEvent> affiliationEventList, int oestrusEventId, IDbConnection conn)
        {
            foreach (var oestrusAffiliationEvent in affiliationEventList)
            {
                var loc = GetLocationString(oestrusAffiliationEvent.latitude, oestrusAffiliationEvent.longitude);

                var withIndividualId = TryGetIndividualId(oestrusAffiliationEvent.withIndividualName, conn);
                if (!withIndividualId.HasValue)
                {
                    throw new Exception($"individual Name '{oestrusAffiliationEvent.withIndividualName}' not found.");
                }

                conn.Execute($@"INSERT INTO mongoose.oestrus_affiliation(
	 oestrus_event_id, with_individual_id, initiate, over, time, location)
	VALUES (@oestrus_event_id, @with_individual_id, @initiate, @over, @time, {loc});",
                    new
                    {
                        oestrus_event_id= oestrusEventId,
                        with_individual_id= withIndividualId,
                        initiate= oestrusAffiliationEvent.initiate,
                        over= oestrusAffiliationEvent.over,
                        time= oestrusAffiliationEvent.time

                    });
            }
        }

        private void InsertAggression(List<OestrusAggressionEvent> aggressionEventList, int oestrusEventId, IDbConnection conn)
        {
            foreach (var oestrusAggressionEvent in aggressionEventList)
            {
                var loc = GetLocationString(oestrusAggressionEvent.latitude, oestrusAggressionEvent.longitude);

                var withIndividualId = TryGetIndividualId(oestrusAggressionEvent.withIndividualName, conn);
                if (!withIndividualId.HasValue)
                {
                    throw new Exception($"individual Name '{oestrusAggressionEvent.withIndividualName}' not found.");
                }

                conn.Execute($@"INSERT INTO mongoose.oestrus_aggression(
	                 oestrus_event_id, with_individual_id, initate, level, over, win, time, location)
	                VALUES (@oestrus_event_id, @with_individual_id, @initate, @level, @over, @win, @time, {loc});",
                    new
                    {
                        oestrus_event_id=oestrusEventId,
                        with_individual_id= withIndividualId,
                        initate= oestrusAggressionEvent.initiate,
                        level = oestrusAggressionEvent.level,
                        over = oestrusAggressionEvent.over,
                        win = oestrusAggressionEvent.win,
                        time = oestrusAggressionEvent.time

                    });
            }
        }

        private int InsertOestrus(int packId, int individualId, int? depth, int? numberOfIndividuals, int? width, DateTime time, double latitude, double longitude, IDbConnection conn)
        {

            var loc = GetLocationString(latitude, longitude);
            var packHistoryId = InsertPackHistory(packId, individualId, time, conn);

            var oestrusEventId = conn.ExecuteScalar<int>(
               $@"INSERT INTO mongoose.oestrus_event(
	             pack_history_id, depth_of_pack, number_of_individuals, width, time, location)
	            VALUES (@pack_history_id, @depth_of_pack, @number_of_individuals, @width, @time, {loc}) RETURNING oestrus_event_id;",
                new
                {
                    pack_history_id = packHistoryId,
                    depth_of_pack= depth,
                    number_of_individuals= numberOfIndividuals,
                    width,
                    time
                });

            return oestrusEventId;

        }

        public void HandlePupFocal(PupFocal message)
        {
         logger.Info(
                $@"Pup Focal Pack '{message.packName}' Individual '{message.focalIndividualName}'.");
            using (IDbConnection conn = new NpgsqlConnection(GetAppSettings.Get().PostgresConnection))
            {
                conn.Open();
                using (var tr = conn.BeginTransaction())
                {
                    var packId = TryGetPackId(message.packName, conn);
                    if (!packId.HasValue)
                    {
                        throw new Exception($"Source Pack Name '{message.packName}' not found.");
                    }

                    var individualId = TryGetIndividualId(message.focalIndividualName, conn);
                    if (!individualId.HasValue)
                    {
                        throw new Exception($"Individual Name '{message.focalIndividualName}' not found.");
                    }

                    //insert a event
                    var pupFocalId = InsertPackFocal(packId.Value, individualId.Value, message.depth, message.visibleIndividuals,
                        message.width, message.time, message.latitude, message.longitude, conn);
                    InsertPupCare(message.PupCareList, pupFocalId, conn);
                    //      InsertAffiliation(message.AffiliationEventList, pupFocalId, conn);
                    //       InsertMaleAggresssion(message.MaleAggressionList, pupFocalId, conn);
                    //        InsertMate(message.MateEventList, pupFocalId, conn);
                    //        InsertNearest(message.NearestList, pupFocalId, conn);


                    tr.Commit();
                }
            }
        }

        private void InsertPupCare(List<PupCare> pupCareList, int pupFocalId, IDbConnection conn)
        {
            foreach (var pupCare in pupCareList)
            {
                var loc = GetLocationString(pupCare.latitude, pupCare.longitude);

                var whoIndividualId = TryGetIndividualId(pupCare.whoIndividualName, conn);
                if (!whoIndividualId.HasValue)
                {
                    throw new Exception($"individual Name '{pupCare.whoIndividualName}' not found.");
                }

                conn.Execute($@"INSERT INTO mongoose.pup_care(
	                         pup_focal_id, who_individual_id, type, time, location)
	                        VALUES (@pup_focal_id, @who_individual_id, @type, @time, {loc});",
                    new
                    {
                        pup_focal_id = pupFocalId,
                        who_individual_id = whoIndividualId,
                        type = pupCare.type,
                        time = pupCare.time
                    });
            }
        }

        private int InsertPackFocal(int packId, int individualId, string depth, string visibleIndividuals, string width, DateTime time, double latitude, double longitude, IDbConnection conn)
        {
            var loc = GetLocationString(latitude, longitude);
            var packHistoryId = InsertPackHistory(packId, individualId, time, conn);

            var pupFocalId = conn.ExecuteScalar<int>(
                $@"INSERT INTO mongoose.pack_focal(
	             pack_history_id, depth, individuals, width, time, location)
	            VALUES (@pack_history_id, @depth, @individuals, @width, @time, {loc}) RETURNING pack_focal_id;",
                new
                {
                    pack_history_id = packHistoryId,
                    depth = depth,
                    individuals = visibleIndividuals,
                    width,
                    time
                });

            return pupFocalId;
        }
    }
}
