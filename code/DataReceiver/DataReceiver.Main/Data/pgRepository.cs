﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using NLog;

using Dapper;

using DataReceiver.Main.Interfaces;
using DataReceiver.Main.Model;
using DataReceiver.Main.Model.LifeHistory;
using DataReceiver.Main.Model.Oestrus;
using DataReceiver.Main.Model.PregnancyFocal;
using DataReceiver.Main.Model.PupFocal;

using Npgsql;

namespace DataReceiver.Main.Data
{
    public class PgRepository : IPgRepository

    {
        private ILogger _logger;
        private readonly IConnectionManager _connectionManager;

        public PgRepository(ILogger logger, IConnectionManager connectionManager)
        {
            _logger = logger;
            _connectionManager = connectionManager;
        }


        public int StoreMessage(string fullName, string message, string messageId)
        {
            _logger.Info($"Stored message type '{fullName}' with Id '{messageId}'");
            using (IDbConnection conn = _connectionManager.GetConn())
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

        public void MessageHandledOk(int entityId)
        {
            _logger.Info($"Message handled ok.");
            using (IDbConnection conn = _connectionManager.GetConn())
            {
                conn.Execute("update mongoose.event_log set success = true where event_log_id = @entityId",
                    new { entityId });
            }
        }

        public int FailedToHandleMessage(int entityId, Exception ex)
        {
            _logger.Error(ex, "Failed to handle message.");
            using (IDbConnection conn = _connectionManager.GetConn())
            {
                return conn.ExecuteScalar<int>(
                    "update mongoose.event_log set success = false, error = @error where event_log_id = @entityId returning delivered_count",
                    new { entityId, error = ex.ToString() });
            }
        }

        public void InsertNewPack(PackCreated message)
        {
            _logger.Info($@"Add new pack: ""{message.Name}"".");
            using (IDbConnection conn = _connectionManager.GetConn())
            {
                conn.Open();
                using (var tr = conn.BeginTransaction())
                {
                    // check if pack exists using unique id
                    // if it does then check if the name is diffent
                    // if so update the name and make an entry in history

                    // if it does match then we are done.

                    UpdatePackWithUniqueId(message.UniqueId, message.Name, conn);

                    var current = GetPackNameByUniqueId(message.UniqueId, conn);




                    if (current != null && !string.IsNullOrEmpty(message.Name) && message.Name != current.Name)
                    {
                        _logger.Info($"Updating pack '{current.Name}' to {message.Name}!");
                        UpdatePackName(current.PackId, message.Name, current.Name, conn);
                        tr.Commit();
                        return;
                    }

                    if (TryGetPackId(message.Name, conn).HasValue)
                    {
                        //todo: what if we are updateing something on this indiv?
                        _logger.Info($"Asked to add pack '{message.Name}' but they already exist!");
                        tr.Commit();
                        return;
                    }

                    var packId = InsertPack(message.Name, message.UniqueId, conn, message.CreatedDate);
                    CreatePackEvent(packId, message.CreatedDate, "NGRP", conn);
                    tr.Commit();
                }
            }
        }

        private void UpdatePackWithUniqueId(string uniqueId, string name, IDbConnection conn)
        {
            var pack = GetPackNameByUniqueId(uniqueId, conn);
            var packId = TryGetPackId(name, conn);
            if (pack == null && packId.HasValue)
            {
                conn.ExecuteScalar(
                    "Update mongoose.pack set unique_id = @uniqueId where pack_id = @id and unique_id is null;",
                    new
                    {
                        uniqueId = uniqueId,
                        id = packId
                    });
            }
        }

        private void UpdatePackName(object packId, string newName, string oldName, IDbConnection conn)
        {
            // insert oldName into the database history table
            conn.ExecuteScalar(
                "Insert into mongoose.pack_name_history (pack_id, name, date_changed) values(@packId, @name, @date)",
                new { packId = packId, name = oldName, date = DateTime.UtcNow });
            // update the pack with new name.
            conn.ExecuteScalar("Update mongoose.pack set name = @newName where pack_id = @id", new
            {
                newName = newName,
                id = packId
            });
        }

        private dynamic GetPackNameByUniqueId(string uniqueId, IDbConnection conn)
        {
            return conn.Query<dynamic>(
                @"select name as ""Name"", pack_id as ""PackId"" from mongoose.pack where unique_id = @uniqueId",
                new { uniqueId }).FirstOrDefault();
        }

        public void InsertNewWeight(WeightMeasure message)
        {
            _logger.Info($"Add new weight for '{message.IndividualName}'. Weight = '{message.Weight}'");

            // get a packid
            // get a individualid
            // get a pack history id

            // add the weight info

            using (IDbConnection conn = _connectionManager.GetConn())
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
            _logger.Info($"Add new individual '{message.Name}'.");

            using (IDbConnection conn = _connectionManager.GetConn())
            {
                message.Gender = NormaliseGender(message.Gender);

                conn.Open();
                using (var tr = conn.BeginTransaction())
                {
                    // get name by unique id
                    // if not there try by name
                    // if there and name is differnet update name
                    // if either are there don't carry on with the add.
                    UpdateIndividualWithUniqueId(message.UniqueId, message.Name, conn);

                    var current = GetIndividualNameByUniqueId(message.UniqueId, conn);
                    if (current != null)
                    {
                        if (message.Name != null && message.Name != current.Name)
                        {
                            _logger.Info($"Updating Individual name '{current.Name}' to '{message.Name}'!");
                            UpdateIndividualName(current.IndividualId, message.Name, current.Name, conn);

                        }

                        if (message.CollarWeight.HasValue && !current.CollarWeight.Equals(message.CollarWeight))
                        {
                            // update collerweight
                            _logger.Info($"Updating Individual collar weight '{current.CollarWeight}' to {message.CollarWeight}!");
                            UpdateCollarWeight(current.IndividualId, message.CollarWeight, current.CollarWeight, conn);

                        }

                        if (!string.IsNullOrEmpty(message.Gender) && current.Sex != message.Gender)
                        {
                            _logger.Info($"Updating individual gender '{current.Sex}' to {message.Gender}!");
                            UpdateGender(current.IndividualId, message.Gender, current.Sex, conn);
                        }

                        if (!string.IsNullOrEmpty(message.ChipCode) && current.TransponderId != message.ChipCode)
                        {
                            // update transponder
                            _logger.Info($"Updating individual transponder code '{current.TransponderId}' to {message.ChipCode}!");
                            UpdateTransponder(current.IndividualId, message.ChipCode, current.TransponderId, conn);
                        }

                        if (message.DateOfBirth != null && current.DateOfBirth == message.DateOfBirth)
                        {
                            //date of birth
                            _logger.Info($"Updating individual date of birth '{current.DateOfBirth}' to {message.DateOfBirth}!");
                            UpdateDateOfBirth(current.IndividualId, message.DateOfBirth, current.DateOfBirth, conn);
                        }

                        tr.Commit();
                        return;
                    }


                    if (TryGetIndividualId(message.Name, conn).HasValue)
                    {
                        //todo: what if we are updateing something on this indiv?
                        _logger.Info($"Asked to add Individual '{message.Name}' but they already exist!");
                        return;
                    }

                    var packId = InsertPack(message.PackCode, message.PackUniqueId, conn);
                    // need to get the litter from the pack? what if we have not received the litter update yet??
                    int? litterId = null;
                    if (string.IsNullOrEmpty(message.LitterCode))
                    {
                        litterId = GetCurrentLitterForPack(packId, conn);
                    }
                    else
                    {
                        litterId = InsertLitter(message.LitterCode, packId, conn);
                    }

                    var individualId = InsertIndividual(message, litterId, conn);
                    var packhistory = InsertPackHistory(packId, individualId, message.DateOfBirth, conn);

                    if (message.DateOfBirth.HasValue)
                    {
                        CreateIndividualEvent(packhistory, message.DateOfBirth, "BORN", conn);
                        if (litterId.HasValue)
                        {
                            if (LitterIsNotBorn(litterId.Value, conn))
                            {
                                CreateLitterEvent(litterId.Value, message.DateOfBirth.Value, "BORN", conn);
                            }
                        }
                    }
                    else
                    {
                        //todo: what if born already? (or fseen already?!)
                        CreateIndividualEvent(packhistory, DateTime.UtcNow, "FSEEN", conn);
                    }

                    tr.Commit();
                }
            }
        }

        private string NormaliseGender(string gender)
        {
            var male = new List<string> { "M", "m", "Male", "male" };
            var female = new List<string> { "F", "f", "Female", "female" };
            var pup = new List<string> { "p" };

            if (male.Contains(gender))
            {
                return "M";
            }

            if (female.Contains(gender))
            {
                return "F";
            }

            if (pup.Contains(gender))
            {
                return "P";
            }

            return null;
        }

        private int? GetCurrentLitterForPack(int packId, IDbConnection conn)
        {
            return conn.ExecuteScalar<int?>(
                "select litter_id from mongoose.litter where pack_id = @id order by date_formed desc limit 1",
                new
                {
                    id = packId
                });
        }

        private void UpdateIndividualWithUniqueId(string uniqueId, string name, IDbConnection conn)
        {
            // if there is a memeber with this unique then good times
            // if thee is a member with this name but no unique then good times


            var individualByUniqueId = GetIndividualNameByUniqueId(uniqueId, conn);
            var individualId = TryGetIndividualId(name, conn);
            if (individualByUniqueId == null && individualId.HasValue)
            {
                _logger.Info($@"updating '{name}' to unique Id '{uniqueId}'.");
                conn.ExecuteScalar(
                    "Update mongoose.individual set unique_id = @uniqueId where individual_id = @id and unique_id is null;",
                    new
                    {
                        uniqueId = uniqueId,
                        id = individualId
                    });
            }
        }

        private void UpdateDateOfBirth(int individualId, DateTime? newDateOfBirth, DateTime? currentDateOfBirth,
            IDbConnection conn)
        {
            //Update date of birth in individual table
            conn.ExecuteScalar(
                "Update mongoose.individual set date_of_birth = @newDateOfBirth where individual_id = @id",
                new
                {
                    newDateOfBirth = newDateOfBirth,
                    id = individualId
                });

            // update / add born event for this individual.


        }

        private void UpdateTransponder(int individualId, string newChipCode, string currentTransponderId, IDbConnection conn)
        {
            conn.ExecuteScalar(
                "Update mongoose.individual set transponder_id = @newChipCode where individual_id = @id",
                new
                {
                    newChipCode = newChipCode,
                    id = individualId
                });
        }

        private void UpdateGender(int individualId, string newGender, string currentSex, IDbConnection conn)
        {
            conn.ExecuteScalar(
                "Update mongoose.individual set sex = @newGender where individual_id = @id",
                new
                {
                    newGender = newGender,
                    id = individualId
                });
        }

        private void UpdateCollarWeight(int individualId, double? newCollarWeight, double? currentCollarWeight, IDbConnection conn)
        {
            conn.ExecuteScalar(
                "Update mongoose.individual set collar_weight = @newCollarWeight where individual_id = @id",
                new
                {
                    newCollarWeight = newCollarWeight,
                    id = individualId
                });
        }

        private void UpdateIndividualName(int individualId, string newName, string oldName, IDbConnection conn)
        {
            // insert oldName into the database history table
            conn.ExecuteScalar(
                "Insert into mongoose.individual_name_history (individual_id, name, date_changed) values(@individualId, @name, @date)",
                new { individualId = individualId, name = oldName, date = DateTime.UtcNow });
            // update the individual with new name.
            conn.ExecuteScalar("Update mongoose.individual set name = @newName where individual_id = @id", new
            {
                newName = newName,
                id = individualId
            });
        }

        private DatabaseIndividual GetIndividualNameByUniqueId(string uniqueId, IDbConnection conn)
        {

            return conn.Query<DatabaseIndividual>(
                  @"SELECT 
                    individual_id as IndividualId, 
                    litter_id as LitterId,
                    name as Name,
                    sex as Sex,
                    transponder_id as TransponderId,
                    unique_id as UniqueId, 
                    collar_weight as CollarWeight,
                    date_of_birth as DateOfBirth,
                    is_mongoose as IsMongoose
	            FROM mongoose.individual where unique_id = @uniqueId;",
                  new { uniqueId }).FirstOrDefault();
        }

        private bool LitterIsNotBorn(int litterId, IDbConnection conn)
        {
            var litterName = conn.Query<string>(@"SELECT l.name
	                                    FROM mongoose.litter l
                                        join mongoose.litter_event le on  le.litter_id = l.litter_id
                                        join mongoose.litter_event_code lec on lec.litter_event_code_id = le.litter_event_code_id
                                        where lec.code = 'BORN' and l.litter_id = @litterId", new
            {
                litterId
            });

            return string.IsNullOrEmpty(litterName.FirstOrDefault());
        }

        private int InsertPack(string packName, string packUniqueId, IDbConnection conn, DateTime? packCreatedDate = null)
        {
            if (packName == null)
            {
                packName = "Unknown";
            }

            var packId = TryGetPackId(packName, conn);

            if (packCreatedDate.HasValue && packCreatedDate.Value == DateTime.MinValue)
            {
                packCreatedDate = null;
            }

            return packId ?? conn.ExecuteScalar<int>(
                       "Insert into mongoose.pack (name, unique_id, pack_created_date) values (@name, @unique_id, @pack_created_date) RETURNING pack_id",
                       new { name = packName, unique_id = packUniqueId, pack_created_date = packCreatedDate });
        }

        private int? TryGetPackId(string packName, IDbConnection conn)
        {
            var packId = conn.ExecuteScalar<int?>("select pack_id from mongoose.pack where name = @name",
                new { name = packName });
            return packId;
        }

        private int InsertIndividual(IndividualCreated message, int? litterId, IDbConnection conn)
        {
            var individualId = TryGetIndividualId(message.Name, conn);

            // todo: if we have an individual then look at its data and see if we can add some more

            return individualId ?? AddIndividual(message.Name, message.Gender, message.ChipCode, message.UniqueId,
                       litterId, message.DateOfBirth, conn);
        }

        private int AddIndividual(string name, string gender, string chipcode, string uniqueId, int? litterId, DateTime? dateOfBirth, IDbConnection conn)
        {
            return conn.ExecuteScalar<int>(
                @"Insert into mongoose.individual
                 (name, sex, litter_id, transponder_id, date_of_birth, unique_id) 
                 values 
                 (@name, @sex, @litter_id, @transponder_id, @dateOfBirth, @unique_id) RETURNING individual_id",
                new
                {
                    name,
                    sex = gender,
                    litter_id = litterId,
                    transponder_id = chipcode,
                    dateOfBirth = dateOfBirth,
                    unique_id = uniqueId
                });
        }

        private int? TryGetIndividualId(string name, IDbConnection conn)
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
            _logger.Info($@"{message.GetType().Name} Event for pack: ""{message.entity_name}"".");
            using (IDbConnection conn = _connectionManager.GetConn())
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

        private void CreatePackEvent(int packId, DateTime? messageDate, string code, IDbConnection conn)
        {
            _logger.Info($"Adding '{code}' event for pack '{packId}'");

            var eventId = conn.ExecuteScalar<int>(
                @"SELECT pack_event_code_id FROM mongoose.pack_event_code where code = @code",
                new
                {
                    code = code.ToUpperInvariant()
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
            _logger.Info($@"{message.GetType().Name} Event for Individual: ""{message.entity_name}"".");
            using (IDbConnection conn = _connectionManager.GetConn())
            {
                conn.Open();
                using (var tr = conn.BeginTransaction())
                {
                    var individiualId = TryGetIndividualId(message.entity_name, conn);

                    if (!individiualId.HasValue)
                    {
                        individiualId = InsertIndividual(new IndividualCreated { Name = message.entity_name, Gender =null, UniqueId = message.UniqueId }, null, conn);
                    }

                    var pack_id = InsertPack(message.associated_pack_name, null, conn);
                    int packHistoryId = InsertPackHistory(pack_id, individiualId.Value, message.Date, conn);
                   
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
            _logger.Info($"Getting Pack History Id for individuaulId '{individiualId}'.");

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

            _logger.Info($"Adding '{code}' event for pack history '{packHistoryId}'.");

            var eventId = conn.ExecuteScalar<int>(
                @"SELECT individual_event_code_id FROM mongoose.individual_event_code where code = @code",
                new
                {
                    code = code.ToUpperInvariant()
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
            _logger.Info($@"{message.GetType().Name} Event for litter: ""{message.entity_name}"".");
            using (IDbConnection conn = _connectionManager.GetConn())
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
            _logger.Info($"Adding '{code}' event for Litter '{litterId}'");

            var eventId = conn.ExecuteScalar<int>(
                @"SELECT litter_event_code_id FROM mongoose.litter_event_code where code = @code",
                new
                {
                    code = code.ToUpperInvariant()
                });

            conn.Execute(@"INSERT INTO mongoose.litter_event
                            (litter_id, litter_event_code_id, date)
	                        VALUES (@litter_id, @litter_event_code_id, @date);",
                new
                {
                    litter_id = litterId,
                    litter_event_code_id = eventId,
                    date = messageDate
                });
        }

        private int? TryGetLitterId(string litterName, IDbConnection conn)
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
            _logger.Info($@"Adding New Litter : ""{message.LitterName}"".");
            using (IDbConnection conn = _connectionManager.GetConn())
            {
                conn.Open();
                using (var tr = conn.BeginTransaction())
                {
                    if (string.IsNullOrEmpty(message.PackName))
                    {
                        message.PackName = "Unknown";
                    }

                    var packId = TryGetPackId(message.PackName, conn);
                    if (!packId.HasValue)
                    {
                        throw new Exception($"Pack Name '{message.PackName}' not found.");
                    }

                    var litterId = TryGetLitterId(message.LitterName, conn);

                    if (litterId.HasValue)
                    {
                        UpdateCreationDateIfAltered(litterId.Value, message.Date, conn);
                        _logger.Info($"Litter already in database!  {message.LitterName}");
                        tr.Commit();
                        return;
                    }

                    //insert a packo evento
                    CreateLitter(message.LitterName, packId.Value, message.Date, conn);

                    tr.Commit();
                }
            }
        }

        private void UpdateCreationDateIfAltered(int litterId, DateTime newDate, IDbConnection conn)
        {
            if (newDate == DateTime.MinValue)
            {
                return;
            }

            var date = conn.ExecuteScalar<DateTime?>("select date_formed from mongoose.litter where litter_id = @id",
                new { id = litterId });

            if (date == null || DateTime.Compare(newDate.Date, date.Value.Date) < 0)
            {
                conn.Execute("UPDATE mongoose.litter SET date_formed = @date WHERE litter_id = @litterId",
                    new { date = newDate, litterId = litterId });
                _logger.Info($"Updated litter id '{litterId}' from '{date}' to '{newDate}'");
            }
        }

        private void CreateLitter(string litterName, int packId, DateTime dateFormed, IDbConnection conn)
        {

            conn.Execute(@"INSERT INTO mongoose.litter
                            (pack_id, name, date_formed) 
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
            _logger.Info(
                $@"Moving '{message.MongooseName}' from Pack '{message.PackSourceName}' to Pack '{message.PackDestintionName}'.");
            using (IDbConnection conn = _connectionManager.GetConn())
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
            _logger.Info(
                $@"Inter Group Event between '{message.packName}' and '{message.otherPackName}'.");
            using (IDbConnection conn = _connectionManager.GetConn())
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
                    RecordInterGroupInteraction(focalPackId, secondPackId, leaderId, outcomeId, message.time, message.latitude,
                        message.longitude, conn);

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
            _logger.Info(
                $@"Group Alarm pack:'{message.packName}' type: '{message.cause}'.");
            using (IDbConnection conn = _connectionManager.GetConn())
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

                    RecordGroupAlarm(packId, causeId, callerId, message.time, message.othersJoin, message.Latitude,
                        message.Longitude, conn);

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
            _logger.Info(
                $@"Group move pack:'{message.pack}' '{message.Direction}' '{message.Destination}'.");

            using (IDbConnection conn = _connectionManager.GetConn())
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
            _logger.Info(
                $@"Oestrus '{message.packName}' Individual '{message.focalIndividualName}'.");
            using (IDbConnection conn = _connectionManager.GetConn())
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
	                            oestrus_focal_id, nearest_individual_id, close_individuals, time)
	                            VALUES (@oestrus_focal_id, @nearest_individual_id, @close_individuals, @time);",
                    new
                    {
                        oestrus_focal_id = oestrusEventId,
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
	                             oestrus_focal_id, with_individual_id, behaviour, female_response, male_response, success, time, location)
	                            VALUES ( @oestrus_focal_id, @with_individual_id, @behaviour, @female_response, @male_response, @success, @time, {
                            loc
                        });",
                    new
                    {
                        oestrus_focal_id = oestrusEventId,
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

                var receiverIndividualId = TryGetIndividualId(oestrusMaleAggression.receiverIndividualName, conn);
                if (!receiverIndividualId.HasValue)
                {
                    throw new Exception($"individual Name '{oestrusMaleAggression.receiverIndividualName}' not found.");
                }

                conn.Execute($@"INSERT INTO mongoose.oestrus_male_aggression(
	                oestrus_focal_id, initiator_individual_id, receiver_individual_id, level, winner, owner, time, location)
	                VALUES (@oestrus_focal_id, @initiator_individual_id, @receiver_individual_id, @level, @winner, @owner, @time, {
                            loc
                        });",
                    new
                    {
                        oestrus_focal_id = oestrusEventId,
                        initiator_individual_id = initiatorIndividualId,
                        receiver_individual_id = receiverIndividualId,
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
	 oestrus_focal_id, with_individual_id, initiate, over, time, location)
	VALUES (@oestrus_focal_id, @with_individual_id, @initiate, @over, @time, {loc});",
                    new
                    {
                        oestrus_focal_id = oestrusEventId,
                        with_individual_id = withIndividualId,
                        initiate = oestrusAffiliationEvent.initiate,
                        over = oestrusAffiliationEvent.over,
                        time = oestrusAffiliationEvent.time

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
	                 oestrus_focal_id, with_individual_id, initate, level, over, win, time, location)
	                VALUES (@oestrus_focal_id, @with_individual_id, @initate, @level, @over, @win, @time, {loc});",
                    new
                    {
                        oestrus_focal_id = oestrusEventId,
                        with_individual_id = withIndividualId,
                        initate = oestrusAggressionEvent.initiate,
                        level = oestrusAggressionEvent.level,
                        over = oestrusAggressionEvent.over,
                        win = oestrusAggressionEvent.win,
                        time = oestrusAggressionEvent.time

                    });
            }
        }

        private int InsertOestrus(int packId, int individualId, int? depth, int? numberOfIndividuals, int? width, DateTime time,
            double latitude, double longitude, IDbConnection conn)
        {

            var loc = GetLocationString(latitude, longitude);
            var packHistoryId = InsertPackHistory(packId, individualId, time, conn);

            var oestrusEventId = conn.ExecuteScalar<int>(
                $@"INSERT INTO mongoose.oestrus_focal(
	             pack_history_id, depth_of_pack, number_of_individuals, width, time, location)
	            VALUES (@pack_history_id, @depth_of_pack, @number_of_individuals, @width, @time, {loc}) RETURNING oestrus_focal_id;",
                new
                {
                    pack_history_id = packHistoryId,
                    depth_of_pack = depth,
                    number_of_individuals = numberOfIndividuals,
                    width,
                    time
                });

            return oestrusEventId;

        }

        public void HandlePupFocal(PupFocal message)
        {
            _logger.Info(
                $@"Pup Focal Pack '{message.packName}' Individual '{message.focalIndividualName}'.");
            using (IDbConnection conn = _connectionManager.GetConn())
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
                    var pupFocalId = InsertPupFocal(packId.Value, individualId.Value, message.depth, message.visibleIndividuals,
                        message.width, message.time, message.latitude, message.longitude, conn);
                    InsertPupCare(message.PupCareList, pupFocalId, conn);
                    InsertPupAggression(message.PupAggressionList, pupFocalId, conn);
                    InsertPupFeed(message.PupFeedList, pupFocalId, conn);
                    InsertPupFind(message.PupFindList, pupFocalId, conn);
                    InsertPupNearest(message.PupNearestList, pupFocalId, conn);


                    tr.Commit();
                }
            }
        }

        private void InsertPupNearest(List<PupNearest> pupNearestList, int pupFocalId, IDbConnection conn)
        {
            foreach (var pupNearest in pupNearestList)
            {
                // var loc = GetLocationString(pupNearest.latitude, pupNearest.longitude);

                var nearestIndividualId = TryGetIndividualId(pupNearest.nearestIndividualName, conn);
                if (!nearestIndividualId.HasValue)
                {
                    throw new Exception($"individual Name '{pupNearest.nearestIndividualName}' not found.");
                }

                conn.Execute($@"INSERT INTO mongoose.pup_nearest(
	                     pup_focal_id, nearest_individual_id, list_of_closest_individuals, scan_time)
	                    VALUES (  @pup_focal_id, @nearest_individual_id, @list_of_closest_individuals, @scan_time);",
                    new
                    {
                        pup_focal_id = pupFocalId,

                        nearest_individual_id = nearestIndividualId,
                        list_of_closest_individuals = string.Join(",", pupNearest.CloseListNames),
                        scan_time = pupNearest.scanTime
                    });
            }
        }

        private void InsertPupFind(List<PupFind> pupFindList, int pupFocalId, IDbConnection conn)
        {
            foreach (var pupFind in pupFindList)
            {
                var loc = GetLocationString(pupFind.latitude, pupFind.longitude);

                conn.Execute($@"INSERT INTO mongoose.pup_find(
	                 pup_focal_id, size, time, location)
	                 VALUES ( @pup_focal_id,  @size, @time, {loc});",
                    new
                    {
                        pup_focal_id = pupFocalId,
                        size = pupFind.size,
                        time = pupFind.time
                    });
            }
        }

        private void InsertPupFeed(List<PupFeed> pupFeedList, int pupFocalId, IDbConnection conn)
        {
            foreach (var pupFeed in pupFeedList)
            {
                var loc = GetLocationString(pupFeed.latitude, pupFeed.longitude);

                var whoIndividualId = TryGetIndividualId(pupFeed.whoIndividualName, conn);
                if (!whoIndividualId.HasValue)
                {
                    throw new Exception($"individual Name '{pupFeed.whoIndividualName}' not found.");
                }

                conn.Execute($@"INSERT INTO mongoose.pup_feed(
	                 pup_focal_id, who_individual_id, size, time, location)
	                VALUES ( @pup_focal_id, @who_individual_id, @size, @time, {loc});",
                    new
                    {
                        pup_focal_id = pupFocalId,
                        who_individual_id = whoIndividualId,
                        size = pupFeed.size,
                        time = pupFeed.time
                    });
            }
        }

        private void InsertPupAggression(List<PupAggressionEvent> pupAggressionList, int pupFocalId, IDbConnection conn)
        {
            foreach (var pupAggressionEvent in pupAggressionList)
            {
                var loc = GetLocationString(pupAggressionEvent.latitude, pupAggressionEvent.longitude);

                var withIndividualId = TryGetIndividualId(pupAggressionEvent.withIndividualName, conn);
                if (!withIndividualId.HasValue)
                {
                    throw new Exception($"individual Name '{pupAggressionEvent.withIndividualName}' not found.");
                }

                conn.Execute($@"INSERT INTO mongoose.pup_aggression(
                    pup_focal_id, with_individual_id, initiate, level, over, win, time, location)
                    VALUES (@pup_focal_id, @with_individual_id, @initiate, @level, @over, @win, @time, {loc});",
                    new
                    {
                        pup_focal_id = pupFocalId,

                        with_individual_id = withIndividualId,
                        initiate = pupAggressionEvent.initiate,
                        level = pupAggressionEvent.level,
                        over = pupAggressionEvent.over,
                        win = pupAggressionEvent.win,
                        time = pupAggressionEvent.time
                    });
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

        private int InsertPupFocal(int packId, int individualId, int? depth, int? visibleIndividuals, int? width, DateTime time,
            double latitude, double longitude, IDbConnection conn)
        {
            var loc = GetLocationString(latitude, longitude);
            var packHistoryId = InsertPackHistory(packId, individualId, time, conn);

            var pupFocalId = conn.ExecuteScalar<int>(
                $@"INSERT INTO mongoose.pup_focal(
	             pack_history_id, depth, individuals, width, time, location)
	            VALUES (@pack_history_id, @depth, @individuals, @width, @time, {loc}) RETURNING pup_focal_id;",
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

        public void HandlePregnancyFocal(PregnancyFocal message)
        {
            _logger.Info(
                $@"Pregnancy Focal Pack '{message.packName}' Individual '{message.focalIndividualName}'.");
            using (IDbConnection conn = _connectionManager.GetConn())
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
                    var pregnancyFocalId = InsertPregnancyFocal(packId.Value, individualId.Value, message.depth,
                        message.visibleIndividuals,
                        message.width, message.time, message.latitude, message.longitude, conn);

                    InsertPregnancyAffilliation(message.PregnancyAffiliationList, pregnancyFocalId, conn);
                    InsertPregnancyAggression(message.PregnancyAggressionList, pregnancyFocalId, conn);
                    InsertPregnancyNearest(message.PregnancyNearestList, pregnancyFocalId, conn);


                    tr.Commit();
                }
            }
        }

        private void InsertPregnancyNearest(List<PregnancyNearest> pregnancyNearestList, int pregnancyFocalId, IDbConnection conn)
        {
            foreach (var pregnancyNearest in pregnancyNearestList)
            {
                // var loc = GetLocationString(pregnancyNearest.latitude, pregnancyNearest.longitude);

                var nearestIndividualId = TryGetIndividualId(pregnancyNearest.nearestIndividualName, conn);
                if (!nearestIndividualId.HasValue)
                {
                    throw new Exception($"individual Name '{pregnancyNearest.nearestIndividualName}' not found.");
                }

                conn.Execute($@"INSERT INTO mongoose.pregnancy_nearest(
	                     pregnancy_focal_id, nearest_individual_id, list_of_closest_individuals, scan_time)
	                    VALUES (  @pregnancy_focal_id, @nearest_individual_id, @list_of_closest_individuals, @scan_time);",
                    new
                    {
                        pregnancy_focal_id = pregnancyFocalId,

                        nearest_individual_id = nearestIndividualId,
                        list_of_closest_individuals = string.Join(",", pregnancyNearest.CloseListNames),
                        scan_time = pregnancyNearest.scanTime
                    });
            }
        }

        private void InsertPregnancyAggression(List<PregnancyAggression> pregnancyAggressionList, int pregnancyFocalId,
            IDbConnection conn)
        {
            foreach (var pregnancyAggression in pregnancyAggressionList)
            {
                var loc = GetLocationString(pregnancyAggression.latitude, pregnancyAggression.longitude);

                var withIndividualId = TryGetIndividualId(pregnancyAggression.withIndividualName, conn);
                if (!withIndividualId.HasValue)
                {
                    throw new Exception($"individual Name '{pregnancyAggression.withIndividualName}' not found.");
                }

                conn.Execute($@"INSERT INTO mongoose.pregnancy_aggression(
                    pregnancy_focal_id, with_individual_id, initiate, level, over, win, time, location)
                    VALUES (@pregnancy_focal_id, @with_individual_id, @initiate, @level, @over, @win, @time, {loc});",
                    new
                    {
                        pregnancy_focal_id = pregnancyFocalId,
                        with_individual_id = withIndividualId,
                        initiate = pregnancyAggression.initiate,
                        level = pregnancyAggression.level,
                        over = pregnancyAggression.over,
                        win = pregnancyAggression.win,
                        time = pregnancyAggression.time
                    });
            }
        }


        private void InsertPregnancyAffilliation(List<PregnancyAffiliation> pregnancyAffiliationList, int pregnancyFocalId,
            IDbConnection conn)
        {
            foreach (var pregnancyAffiliation in pregnancyAffiliationList)
            {
                var loc = GetLocationString(pregnancyAffiliation.latitude, pregnancyAffiliation.longitude);

                var withIndividualId = TryGetIndividualId(pregnancyAffiliation.withIndividualName, conn);
                if (!withIndividualId.HasValue)
                {
                    throw new Exception($"individual Name '{pregnancyAffiliation.withIndividualName}' not found.");
                }

                conn.Execute($@"INSERT INTO mongoose.pregnancy_affiliation(
	                         pregnancy_focal_id, with_individual_id, initiate, over, time, location)
	                        VALUES (@pregnancy_focal_id, @with_individual_id, @initiate, @over, @time, {loc});",
                    new
                    {
                        pregnancy_focal_id = pregnancyFocalId,
                        with_individual_id = withIndividualId,
                        initiate = pregnancyAffiliation.initiate,
                        over = pregnancyAffiliation.over,
                        time = pregnancyAffiliation.time

                    });
            }
        }

        private int InsertPregnancyFocal(int packId, int individualId, int? depth, int? visibleIndividuals,
            int? width, DateTime time, double latitude, double longitude, IDbConnection conn)
        {
            var loc = GetLocationString(latitude, longitude);
            var packHistoryId = InsertPackHistory(packId, individualId, time, conn);

            var pupFocalId = conn.ExecuteScalar<int>(
                $@"INSERT INTO mongoose.pregnancy_focal(
	             pack_history_id, depth, individuals, width, time, location)
	            VALUES (@pack_history_id, @depth, @individuals, @width, @time, {loc}) RETURNING pregnancy_focal_id;",
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

        public void HandleGroupComposition(GroupComposition message)
        {
            //handle these types, how to merge with existing data?
            //       message.MateGuardsList
            //         message.PupAssociationsList
            //           message.WeightsList

            // deal with thses?
            //  message.PregnantNames <- put em in a table
            //     message.PupNames   <- put em in a list!

            _logger.Info($@"Group composition pack:'{message.packName}'.");
            using (IDbConnection conn = _connectionManager.GetConn())
            {
                conn.Open();
                using (var tr = conn.BeginTransaction())
                {
                    if (string.IsNullOrEmpty(message.packName))
                    {
                        message.packName = "Unknown";
                    }

                    var packId = TryGetPackId(message.packName, conn);
                    if (!packId.HasValue)
                    {
                        throw new Exception($"Pack Name '{message.packName}' not found.");
                    }

                    // Yeh we call it pack comp now, becase there is historic data in the group composition
                    // table and I did not know what to do so created a pack_composition table for this
                    // new data.
                    var packCompositionId = InsertGroupComposition(packId.Value, message.observer, message.PupNames, message.time,
                        message.Latitude, message.Longitude, conn);

                    InsertMateGuards(message.MateGuardsList, packCompositionId, conn);
                    InsertPregnancies(message.PregnantNames, packCompositionId, conn);
                    InsertWeights(message.WeightsList, packId.Value, packCompositionId, conn);
                    InsertPupAssociations(message.PupAssociationsList, packId.Value, packCompositionId, conn);


                    tr.Commit();
                }
            }
        }

        private void InsertPupAssociations(List<PupAssociation> pupAssociationsList, int packId, int packCompositionId,
            IDbConnection conn)
        {
            //todo: is accurate confidence?
            foreach (var pupAssociation in pupAssociationsList)
            {
                var loc = GetLocationString(pupAssociation.latitude, pupAssociation.longitude);

                var pupIndividualId = TryGetIndividualId(pupAssociation.pupName, conn);
                if (!pupIndividualId.HasValue)
                {
                    throw new Exception($"Pup Name '{pupAssociation.pupName}' not found.");
                }

                var pupPackHistoryId = InsertPackHistory(packId, pupIndividualId.Value, pupAssociation.time, conn);

                var escortIndividualId = TryGetIndividualId(pupAssociation.escortName, conn);
                if (!escortIndividualId.HasValue)
                {
                    throw new Exception($"Escort Name '{pupAssociation.escortName}' not found.");
                }

                conn.Execute($@"INSERT INTO mongoose.pup_association(
	  pack_composition_id, pup_pack_history_id, escort_id, date, strength, confidence, location)
	VALUES (@pack_composition_id, @pup_pack_history_id, @escort_id, @date, @strength, @confidence, {loc});",
                    new
                    {
                        pack_composition_id = packCompositionId,
                        pup_pack_history_id = pupPackHistoryId,
                        escort_id = escortIndividualId,
                        date = pupAssociation.time,
                        strength = pupAssociation.strength,
                        confidence = pupAssociation.accurate
                    });


            }
        }

        private void InsertWeights(List<GroupWeightMeasure> weightsList, int packId, int packCompositionId, IDbConnection conn)
        {
            // todo: what about present but not weieghed?
            foreach (var weight in weightsList)
            {
                var loc = GetLocationString(weight.latitude, weight.longitude);

                var individualId = TryGetIndividualId(weight.individualName, conn);
                if (!individualId.HasValue)
                {
                    throw new Exception($"individual Name '{weight.individualName}' not found.");
                }

                var packHistoryId = InsertPackHistory(packId, individualId.Value, weight.time, conn);

                conn.Execute($@"INSERT INTO mongoose.weight(
	  pack_history_id, pack_composition_id, weight,  accuracy,  collar_weight,time, location)
	VALUES ( @pack_history_id, @pack_composition_id, @weight,  @accuracy,  @collar_weight, @time, {loc});",
                    new
                    {
                        pack_history_id = packHistoryId,
                        pack_composition_id = packCompositionId,
                        weight = weight.weight,
                        accuracy = weight.accurate,
                        collar_weight = weight.collarWeight,
                        time = weight.time
                    });
            }
        }

        private void InsertPregnancies(List<string> pregnantNames, int packCompositionId, IDbConnection conn)
        {
            foreach (var pregnantName in pregnantNames)
            {
                var pregnantIndividualId = TryGetIndividualId(pregnantName, conn);
                if (!pregnantIndividualId.HasValue)
                {
                    throw new Exception($"individual Name '{pregnantName}' not found.");
                }

                conn.Execute($@"INSERT INTO mongoose.pregnancy(
	                             pack_composition_id, pregnant_individual_id)
	                            VALUES (@pack_composition_id, @pregnant_individual_id);",
                    new
                    {
                        pack_composition_id = packCompositionId,
                        pregnant_individual_id = pregnantIndividualId

                    });
            }
        }

        private void InsertMateGuards(List<GroupCompositionMateGuard> messageMateGuardsList, int packCompositionId,
            IDbConnection conn)
        {
            foreach (var groupCompositionMateGuard in messageMateGuardsList)
            {
                var loc = GetLocationString(groupCompositionMateGuard.latitude, groupCompositionMateGuard.longitude);

                var femaleIndividualId = TryGetIndividualId(groupCompositionMateGuard.femaleName, conn);
                if (!femaleIndividualId.HasValue)
                {
                    throw new Exception($"individual Name '{groupCompositionMateGuard.femaleName}' not found.");
                }

                var guardIndividualId = TryGetIndividualId(groupCompositionMateGuard.guardName, conn);
                if (!guardIndividualId.HasValue)
                {
                    throw new Exception($"individual Name '{groupCompositionMateGuard.guardName}' not found.");
                }

                conn.Execute($@"INSERT INTO mongoose.mate_guard(
	                        pack_composition_id, female_individual_id, guard_individual_id, strength, pester, time, location)
	                    VALUES (@pack_composition_id, @female_individual_id, @guard_individual_id, @strength, @pester, @time, {
                            loc
                        });",
                    new
                    {
                        pack_composition_id = packCompositionId,
                        female_individual_id = femaleIndividualId,
                        guard_individual_id = guardIndividualId,
                        strength = groupCompositionMateGuard.strength,
                        pester = groupCompositionMateGuard.pester,
                        time = groupCompositionMateGuard.time

                    });
            }
        }

        private int InsertGroupComposition(int packId, string observer, List<string> PupNames, DateTime time, double latitude,
            double longitude, IDbConnection conn)
        {
            var loc = GetLocationString(latitude, longitude);

            var packCompositionId = conn.ExecuteScalar<int>(
                $@"INSERT INTO mongoose.pack_composition(
                    pack_id, pups, pups_count, observer, time, location)
                   VALUES ( @pack_id, @pups, @pups_count, @observer, @time, {loc})
                    RETURNING pack_composition_id;",
                new
                {
                    pack_id = packId,
                    pups = string.Join(", ", PupNames),
                    pups_count = PupNames.Count,
                    time,
                    observer
                });

            return packCompositionId;
        }
    }

    public class DatabaseIndividual
    {
        public string Name { get; set; }
        public int IndividualId { get; set; }
        public string LitterId { get; set; }
        public string Sex { get; set; }
        public string TransponderId { get; set; }
        public string UniqueId { get; set; }
        public double? CollarWeight { get; set; }
        public bool IsMongoose { get; set; }
        public DateTime? DateOfBirth { get; set; }
    }
}
