using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;

using Dapper;

using NLog;

using Npgsql;

using psDataImporter.Contracts.Access;
using psDataImporter.Contracts.dtos;
using psDataImporter.Contracts.Postgres;

namespace psDataImporter.Data
{
    public class PostgresRepository
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public List<Pack> GetAllPacks()
        {
            var pgPacks = new List<Pack>();
            using (IDbConnection conn = new NpgsqlConnection(ConfigurationManager
                .ConnectionStrings["postgresConnectionString"]
                .ConnectionString))
            {
                pgPacks = conn
                    .Query<Pack>("Select pack_id as PackId, name, pack_created_date as CreatedDate from mongoose.pack")
                    .ToList();
            }

            return pgPacks;
        }

        public List<Individual> GetAllIndividuals()
        {
            var pgIndividuals = new List<Individual>();
            using (IDbConnection conn = new NpgsqlConnection(ConfigurationManager
                .ConnectionStrings["postgresConnectionString"]
                .ConnectionString))
            {
                pgIndividuals = conn
                    .Query<Individual>(
                        "Select individual_id as IndividualId, litter_id as LitterId, name, sex from mongoose.individual")
                    .ToList();
            }

            return pgIndividuals;
        }

        public void AddWeights(IEnumerable<Weights> weights)
        {
            foreach (var weight in weights)
            {
                using (var conn = new NpgsqlConnection(ConfigurationManager
                    .ConnectionStrings["postgresConnectionString"]
                    .ConnectionString))

                {
                    var locationString = LocationString(weight.Latitude, weight.Longitude);

                    var sql =
                        "Insert into mongoose.weight (pack_history_id, weight, time, accuracy, session, collar_weight, comment, location)" +
                        $" values (@pack_history_id, @Weight, @Time, @Accuracy, @Session, @CollarWeight, @Comment, {locationString})";

                    using (var cmd = new NpgsqlCommand())
                    {
                        cmd.Connection = conn;
                        conn.Open();
                        cmd.AllResultTypesAreUnknown = true;
                        int pack_history_id =
                            GetPackHistoryId(weight.Group,
                                weight.Indiv); // pgIndividuals.Single(i => i.Name == weight.Indiv).IndividualId;

                        cmd.CommandText = sql;

                        cmd.Parameters.AddWithValue("pack_history_id", pack_history_id);
                        cmd.Parameters.AddWithValue("Weight", weight.Weight);
                        cmd.Parameters.AddWithValue("Time", weight.TimeMeasured);
                        cmd.Parameters.AddWithValue("Accuracy", (object)weight.Accuracy ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("Session", (object)weight.Session ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("CollarWeight", (object)weight.Collar ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("Comment", (object)weight.Comment ?? DBNull.Value);

                        cmd.ExecuteNonQuery();
                    }

                    Logger.Info($"Insert weight for: {weight.Indiv} weight {weight.Weight}");
                }
            }
        }

        public int GetPackHistoryId(string pack, string individual)
        {
            using (IDbConnection conn = new NpgsqlConnection(ConfigurationManager
                .ConnectionStrings["postgresConnectionString"]
                .ConnectionString))

            {
                return conn.ExecuteScalar<int>(@"select ph.pack_history_id  from mongoose.pack_history ph 
            join mongoose.pack p on p.pack_id = ph.pack_id
            join mongoose.individual i on ph.individual_id = i.individual_id
            where p.name = @packName and i.name = @individualName",
                    new { packName = pack, individualName = individual });
            }
        }

        public void InsertPackHistory(int packId, int? individualId, DateTime? date)
        {
            using (IDbConnection conn = new NpgsqlConnection(ConfigurationManager
                .ConnectionStrings["postgresConnectionString"]
                .ConnectionString))

            {
                conn.Execute(
                    "Insert into mongoose.pack_history (pack_id, individual_id, date_joined) values (@PackId, @IndividualId, @DateJoined)",
                    new { PackId = packId, IndividualId = individualId, DateJoined = date });

                Logger.Info($"Insert pack history: {date}");
            }
        }

        public void UpdatePackHistoryDate(DateTime date, PackHistory packHistory)
        {
            using (IDbConnection conn = new NpgsqlConnection(ConfigurationManager
                .ConnectionStrings["postgresConnectionString"]
                .ConnectionString))

            {
                conn.Execute(
                    "update mongoose.pack_history set date_joined = @date where pack_history_id = @packHistoryId",
                    new { date, packHistoryId = packHistory.PackHistoryId });

                Logger.Info($"update pack history: {packHistory.PackHistoryId}");
            }
        }

        public void AddIndividualEventCodes(IEnumerable<string> codes)
        {
            foreach (var code in codes)
            {
                AddIndividualEventCode(code);
            }
        }

        private static void AddIndividualEventCode(string code)
        {
            if (!string.IsNullOrEmpty(code))
            {
                Logger.Info($"Adding individual code: {code}");

                using (IDbConnection conn = new NpgsqlConnection(ConfigurationManager
                    .ConnectionStrings["postgresConnectionString"]
                    .ConnectionString))
                {
                    conn.Execute(
                        "Insert into mongoose.individual_event_code (code) values (@codeValue) ON CONFLICT DO NOTHING",
                        new { codeValue = code.ToLower() });
                }
            }
        }

        public void AddIndividuals(IEnumerable<Individual> individuals)
        {
            foreach (var newIndividual in individuals)
            {
                InsertIndividual(newIndividual);
            }
        }

        public void InsertIndividual(Individual newIndividual)
        {
            using (IDbConnection conn = new NpgsqlConnection(ConfigurationManager
                .ConnectionStrings["postgresConnectionString"]
                .ConnectionString))
            {
                if (string.IsNullOrEmpty(newIndividual.Name))
                {
                    Logger.Warn("Tried to add individual with null name.");
                    return;
                }

                // see if we have the individual
                // if so do we have all the data we have at this point?
                // if not then update it with what we have...
                // what if we have the same data but it conflicts (Sex eg)
                // 
                var inDatabaseIndividual = conn
                    .Query<Individual>(
                        "Select individual_id as IndividualId, litter_id as LitterId,  name, sex  from mongoose.individual where name = @name",
                        new { newIndividual.Name }).SingleOrDefault();

                if (inDatabaseIndividual != null)
                {
                    //If database has no sex, but our new individual does
                    if (string.IsNullOrEmpty(inDatabaseIndividual.Sex) && !string.IsNullOrEmpty(newIndividual.Sex))
                    {
                        Logger.Info(
                            $"Added sex: '{newIndividual.Sex}' to individiual with Id : '{inDatabaseIndividual.IndividualId}'");

                        conn.Execute("Update mongoose.Individual set sex = @sex where individual_id = @id",
                            new { sex = newIndividual.Sex, id = inDatabaseIndividual.IndividualId });
                    }

                    if (inDatabaseIndividual.LitterId == null && newIndividual.LitterId != null)
                    {
                        Logger.Info(
                            $"Added litter: '{newIndividual.LitterId}' to individiual with Id : '{inDatabaseIndividual.IndividualId}'");

                        conn.Execute("Update mongoose.Individual set litter_id = @litterId where individual_id = @id",
                            new { litterId = newIndividual.LitterId, id = inDatabaseIndividual.IndividualId });
                    }
                }
                else
                {
                    var isMongoose = true;
                    var notMongooses = new[]
                        { "NONE", "NA", "ALL", "UNK", "ALLBS", "MOST", "INF", "NEXT LITTER", "Unknown", "SUB" };
                    if (notMongooses.Contains(newIndividual.Name))
                    {
                        isMongoose = false;
                    }

                    conn.ExecuteScalar<int>(
                        @"Insert into mongoose.individual 
                            (name, sex, litter_id, date_of_birth, is_mongoose)
                            values 
                            (@name, @sex, @litterId, @dateOfBirth, @is_mongoose) ON CONFLICT DO NOTHING",
                        new
                        {
                            newIndividual.Name,
                            newIndividual.Sex,
                            litterId = newIndividual.LitterId,
                            dateOfBirth = newIndividual.DateOfBirth,
                            is_mongoose = isMongoose
                        });
                    Logger.Info($"created indivual : {newIndividual.Name}");
                }
            }
        }

        public void InsertOestrusEvent(NewLifeHistory lifeHistory, int? oestrusEventId)
        {

            Logger.Info($"Adding oestrus event code: {lifeHistory.Code} OestrusId {lifeHistory.Litter}.");

            using (IDbConnection conn = new NpgsqlConnection(ConfigurationManager
                .ConnectionStrings["postgresConnectionString"]
                .ConnectionString))
            {
                conn.Execute(
                     @"INSERT INTO mongoose.oestrus_event(
	                     oestrus_event_code_id, oestrus_code, date)
	                    VALUES (@oestrus_event_code_id, @oestrus_code, @date);",
                    new
                    {
                        oestrus_event_code_id = oestrusEventId,
                        oestrus_code = lifeHistory.Litter,
                        date = lifeHistory.Date
                    });
            }
        }

        public int? GetOestrusCodeId(string code)
        {
            int? returval = null;
            if (!string.IsNullOrEmpty(code))
            {
                Logger.Info($"Adding Oestrus code: {code}");

                using (IDbConnection conn = new NpgsqlConnection(ConfigurationManager
                    .ConnectionStrings["postgresConnectionString"]
                    .ConnectionString))
                {
                    returval = conn.ExecuteScalar<int?>(
                        @"with i as (
                            INSERT INTO mongoose.oestrus_event_code(code) VALUES (@codeValue) ON CONFLICT (code) DO NOTHING RETURNING oestrus_event_code_id
                        )
                        select oestrus_event_code_id from i
                        union all
                        select oestrus_event_code_id from mongoose.oestrus_event_code where  code = @codeValue
                        limit 1",
                        new { codeValue = code.ToUpper() });
                }
            }
            return returval;
        }

        public void AddPacks(IEnumerable<string> packNames)
        {
            foreach (var packName in packNames)
            {
                InsertSinglePack(packName);
            }
        }

        public void InsertSinglePack(string packName)
        {
            if (string.IsNullOrEmpty(packName))
            {
                Logger.Warn("Tried to create a null pack.");
                return;
            }

            using (IDbConnection conn = new NpgsqlConnection(ConfigurationManager
                .ConnectionStrings["postgresConnectionString"]
                .ConnectionString))
            {
                conn.ExecuteScalar<int>("Insert into mongoose.pack (name) values (@name) ON CONFLICT DO NOTHING ",
                    new { name = packName });
            }

            Logger.Info($"created pack: {packName}");
        }

        public void AddFoetus(int pack_history_id, int foetusNumber, DateTime ultrasoundDate, string foetusSize,
            float? crossViewWidth, float? crossViewLength, float? longViewLength, float? longViewWidth,
            string comment, string observer)
        {
            using (IDbConnection conn = new NpgsqlConnection(ConfigurationManager
                .ConnectionStrings["postgresConnectionString"]
                .ConnectionString))
            {
                conn.Execute(
                    "Insert into mongoose.ultrasound (pack_history_id, observation_date, foetus_number, foetus_size, cross_view_length, cross_view_width, long_view_length, long_view_width, observer, comment)" +
                    " values(@pack_history_id, @observationDate, @foetusNumber, @foetusSize, @crossViewLength, @crossViewWidth, @longViewLength, @longViewWidth, @Observer, @comment)",
                    new
                    {
                        pack_history_id,
                        observationDate = ultrasoundDate,
                        foetusNumber,
                        foetusSize,
                        crossViewWidth,
                        crossViewLength,
                        longViewLength,
                        longViewWidth,
                        comment,
                        observer
                    });
            }
        }

        public void RemoveUltrasoundData()
        {
            using (IDbConnection conn = new NpgsqlConnection(ConfigurationManager
                .ConnectionStrings["postgresConnectionString"]
                .ConnectionString))
            {
                conn.Execute("truncate mongoose.ultrasound");
                Logger.Info("Truncated ultrasound table");
            }
        }

        public void AddRadioCollar(int pack_history_id, DateTime? fitted, DateTime? turnedOn, DateTime? removed,
            int? frequency, int weight, DateTime? dateEntered, string comment)
        {
            using (IDbConnection conn = new NpgsqlConnection(ConfigurationManager
                .ConnectionStrings["postgresConnectionString"]
                .ConnectionString))
            {
                conn.Execute(
                    "insert into mongoose.radiocollar (pack_history_id, frequency, weight, fitted, turned_on, removed, comment, date_entered) " +
                    "values (@pack_history_id, @frequency, @weight, @fitted, @turnedOn, @removed, @comment, @dateEntered)",
                    new { pack_history_id, frequency, weight, fitted, turnedOn, removed, comment, dateEntered });
                Logger.Info("Added radio collar data.");
            }
        }

        public void AddLitter(LifeHistoryDto litter)
        {
            if (string.IsNullOrEmpty(litter.Pack) || string.IsNullOrEmpty(litter.Individual) ||
                string.IsNullOrEmpty(litter.Litter))
            {
                Logger.Warn(
                    $"Something was null for this litter. pack:{litter.Pack} Individual:{litter.Individual} Litter {litter.Litter}");
                return;
            }

            using (IDbConnection conn = new NpgsqlConnection(ConfigurationManager
                .ConnectionStrings["postgresConnectionString"]
                .ConnectionString))
            {
                //Insert new litter and get back Id
                var litterId = conn.ExecuteScalar<int>(
                    "insert into mongoose.litter (pack_id, name)"
                    + "  values(@packId, @name)"
                    + " on conflict(name) do update set name = @name  RETURNING litter_id",
                    new { packid = litter.pgPackId, name = litter.Litter });

                //update individual with litter id
                conn.Execute(
                    "update mongoose.individual set litter_id = @litterId where individual_id = @individual_id",
                    new { litterId, individual_id = litter.pgIndividualId });

                Logger.Info($"Added litter {litter.Litter}.");
            }
        }

        public void AddPackEventCode(string code)
        {
            if (!string.IsNullOrEmpty(code))
            {
                Logger.Info($"Adding pack code:{code}");

                using (IDbConnection conn = new NpgsqlConnection(ConfigurationManager
                    .ConnectionStrings["postgresConnectionString"]
                    .ConnectionString))
                {
                    conn.Execute(
                        "Insert into mongoose.pack_event_code (code) values (@codeValue) ON CONFLICT DO NOTHING",
                        new { codeValue = code.ToLower() });
                }
            }
        }

        public int GetPackEventCodeId(string code)
        {
            using (IDbConnection conn = new NpgsqlConnection(ConfigurationManager
                .ConnectionStrings["postgresConnectionString"]
                .ConnectionString))
            {
                return conn.ExecuteScalar<int>(
                    "Select pack_event_code_id  from mongoose.pack_event_code where code = @code",
                    new { code = code.ToLower() });
            }
        }

        public void LinkIndividualEvents(int pack_history_id, int individualEventCodeId, string latitude,
            string longitude,
            string startend,
            string status, DateTime date, string exact, string cause, string comment)
        {
            // create geography if lat and long are present.
            var locationString = LocationString(latitude, longitude);

            var sql =
                "Insert into mongoose.individual_event (pack_history_id, individual_event_code_id, status, date, exact, start_end, cause, comment, location )" +
                $" values (@pack_history_id, @individualEventCodeId, @status, @date, @exact, @start_end, @cause, @Comment, {locationString})";

            using (IDbConnection conn = new NpgsqlConnection(ConfigurationManager
                .ConnectionStrings["postgresConnectionString"]
                .ConnectionString))
            {
                Logger.Info(
                    $"Linking individualId: {pack_history_id} with Individual Event codeId: {individualEventCodeId}");
                conn.Execute(sql,
                    new
                    {
                        pack_history_id,
                        individualEventCodeId,
                        status,
                        start_end = startend,
                        date,
                        exact,
                        cause,
                        comment

                    });
            }
        }

        public void LinkPackEvents(int packId, int packEventCodeId, string status, DateTime date,
            string exact, string cause, string comment, string latitude, string longitude)
        {
            var locationString = LocationString(latitude, longitude);

            var sql =
                "Insert into mongoose.pack_event (pack_id, pack_event_code_id, status, date, exact, cause, comment, location )" +
                $" values (@PackId, @packEventCodeId, @status, @date, @exact, @cause, @Comment, {locationString})";

            using (IDbConnection conn = new NpgsqlConnection(ConfigurationManager
                .ConnectionStrings["postgresConnectionString"]
                .ConnectionString))
            {
                Logger.Info($"Linking packId:{packId} with codeId:{packEventCodeId}");
                conn.Execute(sql, new { packId, packEventCodeId, status, date, exact, cause, comment });
            }
        }

        public int GetPackId(string packName)
        {
            using (IDbConnection conn = new NpgsqlConnection(ConfigurationManager
                .ConnectionStrings["postgresConnectionString"]
                .ConnectionString))
            {
                return conn.ExecuteScalar<int>(
                    "Select pack_id from mongoose.pack where name = @packName", new { packName });
            }
        }

        public int? TryGetPackId(string packName)
        {
            using (IDbConnection conn = new NpgsqlConnection(ConfigurationManager
                .ConnectionStrings["postgresConnectionString"]
                .ConnectionString))
            {
                return conn.ExecuteScalar<int?>(
                    "Select pack_id from mongoose.pack where name = @packName", new { packName });
            }
        }

        public int GetIndividualId(string individualName)
        {
            if (string.IsNullOrEmpty(individualName))
            {
                throw new Exception("Individual not found");
            }

            using (IDbConnection conn = new NpgsqlConnection(ConfigurationManager
                .ConnectionStrings["postgresConnectionString"]
                .ConnectionString))
            {
                return conn
                    .ExecuteScalar<int>("Select individual_id from mongoose.individual where name = @individualName",
                        new { individualName });
            }
        }

        public int GetIndiviudalEventCodeId(string individualEventCode)
        {
            AddIndividualEventCode(individualEventCode);

            using (IDbConnection conn = new NpgsqlConnection(ConfigurationManager
                .ConnectionStrings["postgresConnectionString"]
                .ConnectionString))
            {
                return conn
                    .ExecuteScalar<int>(
                        "Select individual_event_code_id from mongoose.individual_event_code where code = @lifeHistoryCode",
                        new { lifeHistoryCode = individualEventCode.ToLower() });
            }
        }

        public void AddOestrusEvent(Oestrus oestrus, List<string> males)
        {
            //get pack history id (femailid and pack id)
            var packHistoryId = GetPackHistoryId(oestrus.GROUP, oestrus.FEMALE_ID);

            //guardId

            var guardid = GetPossibleNullIndividualId(oestrus.GUARD_ID);

            //pesterir 1-4 id
            var pesterer1Id = GetPossibleNullIndividualId(oestrus.PESTERER_ID);
            var pesterer2Id = GetPossibleNullIndividualId(oestrus.PESTERER_ID_2);
            var pesterer3Id = GetPossibleNullIndividualId(oestrus.PESTERER_ID_3);
            var pesterer4Id = GetPossibleNullIndividualId(oestrus.PESTERER_ID_4);

            //    var copulationWithId = GetPossibleNullIndividualId(oestrus.COPULATION);

            var maleIds = males.Select(GetPossibleNullIndividualId);



            var locationString = LocationString(oestrus.Latitude, oestrus.Longitude);

            var sql = $@"INSERT INTO mongoose.oestrus(
                      pack_history_id, date, time, oestrus_code, guard_id, pesterer_id_1, pesterer_id_2, pesterer_id_3, pesterer_id_4, strength, confidence,  location, comment)
                      VALUES( @pack_history_id, @date, @time, @oestrus_code, @guard_id, @pesterer_id_1, @pesterer_id_2, @pesterer_id_3, @pesterer_id_4, @strength, @confidence, {locationString}, @comment) RETURNING oestrus_id;";

            using (IDbConnection conn = new NpgsqlConnection(ConfigurationManager
                .ConnectionStrings["postgresConnectionString"]
                .ConnectionString))
            {
                Logger.Info($"Adding Oestrus event. {oestrus.OESTRUS_CODE} ");
                var oestrusId = conn.ExecuteScalar(sql, new
                {
                    pack_history_id = packHistoryId,
                    date = oestrus.DATE,
                    time = oestrus.TIME,
                    oestrus_code = oestrus.OESTRUS_CODE,
                    guard_id = guardid,
                    pesterer_id_1 = pesterer1Id,
                    pesterer_id_2 = pesterer2Id,
                    pesterer_id_3 = pesterer3Id,
                    pesterer_id_4 = pesterer4Id,
                    strength = oestrus.STRENGTH,
                    confidence = oestrus.CONFIDENCE,
                    comment = oestrus.COMMENT
                });

                foreach (var maleId in maleIds)
                {
                    if (maleId.HasValue)
                    {
                        sql = @"INSERT INTO mongoose.oestrus_copulation_male(
                                    oestrus_id, individual_id)
	                                VALUES(@oestrus_id, @individual_id);";
                        conn.Execute(sql, new
                        {
                            oestrus_id = oestrusId,
                            individual_id = maleId

                        });
                    }
                }
            }
        }

        public Individual GetIndivdualById(int individualId)
        {
            using (IDbConnection conn = new NpgsqlConnection(ConfigurationManager
               .ConnectionStrings["postgresConnectionString"]
               .ConnectionString))

            {
                return conn.Query<Individual>(@"select 
                                                individual_id as IndividualId,
                                                litter_id as LitterId,
                                                name as Name,
                                                sex as Sex,
                                                date_of_birth as DateOfBirth,
                                                transponder_id,
                                                unique_id,
                                                collar_weight,
                                                is_mongoose
                                                from mongoose.individual 
                                                where individual_id = @individual_id",
                     new { individual_id = individualId }).Single();

             
            }
        }

        public int? GetPossibleNullIndividualId(string individualName)
        {
            return string.IsNullOrEmpty(individualName) ? (int?)null : GetIndividualId(individualName);
        }

        public void InsertSingleLitter(string litterName, int packId)
        {
            if (string.IsNullOrEmpty(litterName))
            {
                Logger.Warn("Tried to create a null litter!");
                return;
            }

            if (litterName == "ESD1502")
            {
                packId = GetPackId("11");
            }

            using (IDbConnection conn = new NpgsqlConnection(ConfigurationManager
                .ConnectionStrings["postgresConnectionString"]
                .ConnectionString))
            {
                conn.ExecuteScalar<int>(
                    "Insert into mongoose.litter (name, pack_id) values (@name, @packId) ON CONFLICT DO NOTHING ",
                    new { name = litterName, packId = packId });
            }

            Logger.Info($"created litter: {litterName}");
        }

        public int? GetLitterId(string litterName)
        {
            using (IDbConnection conn = new NpgsqlConnection(ConfigurationManager
                .ConnectionStrings["postgresConnectionString"]
                .ConnectionString))

            {
                return conn.ExecuteScalar<int?>("select litter_id from mongoose.litter where name = @name",
                    new { name = litterName });
            }
        }

        public void AddLitterEventCode(string code)
        {
            if (!string.IsNullOrEmpty(code))
            {
                Logger.Info($"Adding litter event code: {code}");

                using (IDbConnection conn = new NpgsqlConnection(ConfigurationManager
                    .ConnectionStrings["postgresConnectionString"]
                    .ConnectionString))
                {
                    conn.Execute(
                        "Insert into mongoose.litter_event_code (code) values (@codeValue) ON CONFLICT DO NOTHING",
                        new { codeValue = code.ToLower() });
                }
            }
        }

        public int GetLitterEventCodeId(string litterEventCode)
        {
            using (IDbConnection conn = new NpgsqlConnection(ConfigurationManager
                .ConnectionStrings["postgresConnectionString"]
                .ConnectionString))

            {
                return conn.ExecuteScalar<int>(
                    "select litter_event_code_id from mongoose.litter_event_code where code = @code",
                    new { code = litterEventCode.ToLower() });
            }
        }

        public void LinkLitterEvent(int litterId, int litterEventCodeId, NewLifeHistory lifeHistory)
        {
            Logger.Info("Linking litter with event");
            var locationString = LocationString(lifeHistory.Latitude, lifeHistory.Longitude);

            using (IDbConnection conn = new NpgsqlConnection(ConfigurationManager
                .ConnectionStrings["postgresConnectionString"]
                .ConnectionString))
            {
                conn.Execute(
                    $@"Insert into mongoose.litter_event 
                      (litter_id, litter_event_code_id, date, cause, exact, last_seen, location, comment)
                       values
                      (@litterId, @litterEventCodeId, @date, @cause, @exact, @lastSeen, {locationString}, @comment)
                      ON CONFLICT DO NOTHING",
                    new
                    {
                        litterId = litterId,
                        litterEventCodeId = litterEventCodeId,
                        date = lifeHistory.Date,
                        cause = lifeHistory.Cause,
                        exact = lifeHistory.Exact,
                        lastSeen = lifeHistory.Lseen,
                        comment = lifeHistory.Comment
                    });
            }
        }

        public void AddIGI(NewLifeHistory lifeHistory)
        {
            // split if we have multiple secondarys
            // for each .. if it is 'unk' then stick in a null
            // if it is anything else add it to packs table then get its id
            // enter the ids into the table.

            Logger.Info($"Inserting an IGI");

            // makes sure first pack is in the db
            InsertSinglePack(lifeHistory.Pack);


            var secondpacks = lifeHistory.Cause.Split(new[] { "/", ",", " OR ", " or " }, StringSplitOptions.None);
            List<int?> secondPackIds = new List<int?>();
            foreach (var secondpack in secondpacks)
            {
                var packname = secondpack;
                if (packname.Contains("UNK") || packname.Contains("NA"))
                {
                    packname = "Unknown";
                }

                InsertSinglePack(packname);
                secondPackIds.Add(GetPackId(packname));
            }

            foreach (var secondpack in secondPackIds)
            {
                var locationString = LocationString(lifeHistory.Latitude, lifeHistory.Longitude);

                using (IDbConnection conn = new NpgsqlConnection(ConfigurationManager
                    .ConnectionStrings["postgresConnectionString"]
                    .ConnectionString))
                {
                    conn.Execute(
                        $"Insert into mongoose.inter_group_interaction (focalpack_id, secondpack_id, time, location, comment) values (@focalpack_id, @secondpack_id, @time, {locationString}, @comment)",
                        new
                        {
                            focalpack_id = GetPackId(lifeHistory.Pack),
                            secondpack_id = secondpack,
                            time = lifeHistory.Date,
                            comment = lifeHistory.Comment
                        });
                }
            }
        }

        public void AddTransponder(string captureTransponder, int? individualId)
        {
            if (individualId == null)
            {
                Logger.Warn($"Tried to add a Transponder to a null individualId. Transponder: {captureTransponder}");
                return;
            }

            using (IDbConnection conn = new NpgsqlConnection(ConfigurationManager
                .ConnectionStrings["postgresConnectionString"]
                .ConnectionString))
            {
                conn.Execute(
                    "Update mongoose.individual set transponder_id = @TransponderId where individual_id = @individualId",
                    new
                    {
                        TransponderId = captureTransponder,
                        individualId
                    });
            }
        }

        public void AddCaptureData(CapturesNew2013 capture, int packHistoryId)
        {
            Logger.Info($"Adding capture: {capture.Capture_DATE}, ");
            using (IDbConnection conn = new NpgsqlConnection(ConfigurationManager
                .ConnectionStrings["postgresConnectionString"]
                .ConnectionString))
            {
                conn.Execute(
                    @"INSERT INTO mongoose.capture(
                            pack_history_id, date, trap_time, process_time, trap_location, bleed_time, release_time, examiner, age, drugs, reproductive_status, teats_ext, ultrasound, foetuses, foetus_size, weight, head_width, head_length, body_length, hind_foot_length, tail_length, tail_circumference, ticks, fleas, wounds_and_scars, plasma_sample_id, plasma_freeze_time, blood_sample_id, blood_sample_freeze_time, bucket, white_blood_count, white_blood_freeze_time, white_blood_cell_bucket, whisker_sample_id, ear_clip, tail_tip, ""2d4d_photo"", agd_photo, blood_sugar, red_cell_percentage, fat_neck_1, fat_neck_2, fat_armpit, fat_thigh, testes_length, testes_width, testes_depth, tooth_wear, comment)
                            VALUES (@pack_history_id, @date, @trap_time, @process_time, @trap_location, @bleed_time, @release_time, @examiner, @age, @drugs, @reproductive_status, @teats_ext, @ultrasound, @foetuses, @foetus_size, @weight, @head_width, @head_length, @body_length, @hind_foot_length, @tail_length, @tail_circumference, @ticks, @fleas, @wounds_and_scars, @plasma_sample_id, @plasma_freeze_time, @blood_sample_id, @blood_sample_freeze_time, @bucket, @white_blood_count, @white_blood_freeze_time, @white_blood_cell_bucket, @whisker_sample_id, @ear_clip, @tail_tip, @dd_photo, @agd_photo, @blood_sugar, @red_cell_percentage, @fat_neck_1, @fat_neck_2, @fat_armpit, @fat_thigh, @testes_length, @testes_width, @testes_depth, @tooth_wear, @comment); "
                    ,
                    new
                    {
                        pack_history_id = packHistoryId,
                        date = capture.Capture_DATE,
                        trap_time = capture.TRAP_TIME,
                        process_time = capture.PROCESS_TIME,
                        trap_location = capture.TRAP_LOCN,
                        bleed_time = capture.BLEED_TIME,
                        release_time = capture.RELEASE_TIME,
                        examiner = capture.Examiner,
                        age = capture.AGE,
                        drugs = capture.DRUGS,
                        reproductive_status = capture.REPRO_STATUS,
                        teats_ext = capture.TEATS_EXT,
                        ultrasound = capture.ULTRASOUND,
                        foetuses = capture.FOETUSES,
                        foetus_size = capture.FOET_SIZE,
                        weight = capture.WEIGHT,
                        head_width = capture.HEAD_WIDTH,
                        head_length = capture.HEAD_LENGTH,
                        body_length = capture.BODY_LENGTH,
                        hind_foot_length = capture.HINDFOOT_LENGTH,
                        tail_length = capture.TAIL_LENGTH,
                        tail_circumference = capture.TAIL_CIRC,
                        ticks = capture.TICKS,
                        fleas = capture.FLEAS,
                        wounds_and_scars = capture.SCARS_WOUNDS,
                        plasma_sample_id = capture.PLASMA_SAMPLE_PL,
                        plasma_freeze_time = capture.FREEZE_TIME_PL,
                        blood_sample_id = capture.BLOOD_SAMPLE_BL,
                        blood_sample_freeze_time = capture.FREEZE_TIME_BL,
                        bucket = capture.BUCKET_PLxxx_AND_BLxxx,
                        white_blood_count = capture.White_blood_WBC,
                        white_blood_freeze_time = capture.FREEZE_TIME_WBC,
                        white_blood_cell_bucket = capture.BUCKET_WBC,
                        whisker_sample_id = capture.WHISKER_SAMPLE_WSK,
                        ear_clip = Boolify(capture.EAR_CLIP_TAKEN),
                        tail_tip = Boolify(capture.TAIL_TIP),
                        dd_photo = Boolify(capture.twoDfourD_photos),
                        agd_photo = Boolify(capture.AGD_photos),
                        blood_sugar = capture.Blood_sugar,
                        red_cell_percentage = capture.Red_cell_percentage,
                        fat_neck_1 = capture.Fat_neck_1,
                        fat_neck_2 = capture.Fat_neck_2,
                        fat_armpit = capture.Fat_armpit,
                        fat_thigh = capture.Fat_thigh,
                        testes_length = capture.TESTES_L,
                        testes_width = capture.TESTES_W,
                        testes_depth = capture.TESTES_DEPTH,
                        tooth_wear = capture.TOOTH_WEAR,
                        comment = capture.COMMENTS
                    });
            }
        }

        private bool? Boolify(string valueToCheck)
        {
            var yesValues = new[] { "Y", "YES", "YE S", "YEYS", "1" };
            var noValues = new[] { "N", "NO", "0", "-1", "X" };

            if (yesValues.Contains(valueToCheck))
            {
                return true;
            }

            if (noValues.Contains(valueToCheck))
            {
                return false;
            }

            return null;
        }

        public void InsertPupAssociation(int packHistoryId, int? escortId, PupAssociation pupAssociation)
        {
            Logger.Info($"Adding pup Association: {pupAssociation.PUP}");

            var locationString = "NULL";
            if (!string.IsNullOrEmpty(pupAssociation.Latitude) && !string.IsNullOrEmpty(pupAssociation.Longitude))
            {
                locationString =
                    $"ST_GeographyFromText('SRID=4326;POINT({pupAssociation.Latitude} {pupAssociation.Longitude})')";
            }

            using (IDbConnection conn = new NpgsqlConnection(ConfigurationManager
                .ConnectionStrings["postgresConnectionString"]
                .ConnectionString))
            {
                conn.Execute($@"INSERT INTO mongoose.pup_association(
                           pup_pack_history_id, escort_id, date, strength, confidence, location, comment, comment_editing)
                            VALUES (@pup_pack_history_id, @escort_id, @date, @strength, @confidence, {locationString}, @comment, @comment_editing)"
                    ,
                    new
                    {
                        pup_pack_history_id = packHistoryId,
                        escort_id = escortId,
                        date = pupAssociation.DATE,
                        strength = pupAssociation.STRENGTH,
                        confidence = pupAssociation.CONFIDENCE,
                        comment = pupAssociation.COMMENT,
                        comment_editing = pupAssociation.Editing_comments
                    }
                );
            }
        }

        public void InsertBabysittingRecord(int packHistoryId, int? litterId, DateTime? startTime, DateTime? endTime,
            int? denDistance, BABYSITTING_RECORDS babysitting)
        {
            Logger.Info($"Adding babysitting record: {babysitting.BS}");

            var locationString = LocationString(babysitting.Latitude, babysitting.Longitude);

            using (IDbConnection conn = new NpgsqlConnection(ConfigurationManager
                .ConnectionStrings["postgresConnectionString"]
                .ConnectionString))
            {
                conn.Execute($@"INSERT INTO mongoose.babysitting(
                         babysitter_pack_history_id, date, litter_id, type, time_start, den_distance, time_end, accuracy, comment, location)
                            VALUES (@babysitter_pack_history_id, @date, @litter_id, @type, @time_start, @den_distance, @time_end, @accuracy, @comment,  {
                            locationString
                        })"
                    ,
                    new
                    {
                        babysitter_pack_history_id = packHistoryId,
                        date = babysitting.DATE,
                        litter_id = litterId,
                        type = babysitting.TYPE,
                        time_start = startTime,
                        den_distance = denDistance,
                        time_end = endTime,
                        accuracy = babysitting.ACCURACY,
                        comment = babysitting.COMMENT
                    }
                );
            }
        }

        private string LocationString(string latitude, string longitude)
        {
            var locationString = "NULL";
            if (!string.IsNullOrEmpty(latitude) && !string.IsNullOrEmpty(longitude) && latitude != "0" &&
                longitude != "0")
            {
                locationString = $"ST_GeographyFromText('SRID=4326;POINT({latitude} {longitude})')";
            }

            return locationString;
        }

        public void InsertGroupComposition(int? packId, int? malesOverOneYear, int? femalesOverOneYear,
            int? malesOverThreeMonths, int? femalesOverThreeMonths, DiaryAndGrpComposition groupComposition)
        {
            Logger.Info($"Insert group composition on date: {groupComposition.Date}");

            using (IDbConnection conn = new NpgsqlConnection(ConfigurationManager
                .ConnectionStrings["postgresConnectionString"]
                .ConnectionString))
            {
                conn.Execute($@"INSERT INTO mongoose.group_composition(
                         pack_id, date, observer, session, group_status, weather_start, weather_end, males_over_one_year, females_over_one_year, males_over_three_months, females_over_three_months, male_pups, female_pups, unknown_pups, pups_in_den, comment)
                            VALUES (@pack_id, @date, @observer, @session, @group_status, @weather_start, @weather_end, @males_over_one_year, @females_over_one_year, @males_over_three_months, @females_over_three_months, @male_pups, @female_pups, @unknown_pups, @pups_in_den, @comment)"
                    ,
                    new
                    {
                        pack_id = packId,
                        date = groupComposition.Date,
                        observer = groupComposition.Observer,
                        session = groupComposition.Session,
                        group_status = groupComposition.Group_status,
                        weather_start = groupComposition.ST_Weather,
                        weather_end = groupComposition.END_Weather,
                        males_over_one_year = malesOverOneYear,
                        females_over_one_year = femalesOverOneYear,
                        males_over_three_months = malesOverThreeMonths,
                        females_over_three_months = femalesOverThreeMonths,
                        male_pups = groupComposition.Male_em_pups,
                        female_pups = groupComposition.Female_em_pups,
                        unknown_pups = groupComposition.Unk_em_pups,
                        pups_in_den = groupComposition.Pups_in_Den,
                        comment = groupComposition.Comment
                    }
                );
            }
        }

        public void InsertPooRecord(int packHistoryId, DateTime? emergenceTime, DateTime? timeOfCollection,
            POO_DATABASE pooSample)
        {
            Logger.Info($"Insert poo: {pooSample.Individual}");

            if (pooSample.Sample_Number == "F08213" && pooSample.Individual == "TM373")
            {
                pooSample.Sample_Number = "F08213A";
                pooSample.Comment += " Duplicate sample number F08213";
            }

            using (IDbConnection conn = new NpgsqlConnection(ConfigurationManager
                .ConnectionStrings["postgresConnectionString"]
                .ConnectionString))
            {
                conn.Execute($@"INSERT INTO mongoose.poo_sample(
                          pack_history_id, sample_number, date, pack_status, emergence_time, collection_time, freezer_time, parasite_sample, comment)
                            VALUES (@pack_history_id, @sample_number, @date, @pack_status, @emergence_time, @collection_time, @freezer_time, @parasite_sample, @comment)",
                    new
                    {
                        pack_history_id = packHistoryId,
                        sample_number = pooSample.Sample_Number,
                        date = pooSample.Date,
                        pack_status = pooSample.Pack_Status,
                        emergence_time = emergenceTime,
                        collection_time = timeOfCollection,
                        freezer_time = pooSample.Time_in_Freezer,
                        parasite_sample = Boolify(pooSample.Parasite_sample_taken),
                        comment = pooSample.Comment
                    }
                );
            }
        }

        public void InsertWeather(METEROLOGICAL_DATA meterologicalData)
        {
            Logger.Info($"Insert weather: {meterologicalData.DATE}");

            using (IDbConnection conn = new NpgsqlConnection(ConfigurationManager
                .ConnectionStrings["postgresConnectionString"]
                .ConnectionString))
            {
                conn.Execute($@"INSERT INTO mongoose.meterology(
	 date, rain, temp_max, temp_min, temp, humidity_max, humidity_min, observer)
	VALUES (  @date, @rain, @temp_max, @temp_min, @temp, @humidity_max, @humidity_min, @observer);",
                    new
                    {
                        date = meterologicalData.DATE,
                        rain = meterologicalData.RAIN_MWEYA,
                        temp_max = meterologicalData.MAX_TEMP,
                        temp_min = meterologicalData.MIN_TEMP,
                        temp = meterologicalData.TEMP,
                        humidity_max = meterologicalData.MAX_HUMIDITY,
                        humidity_min = meterologicalData.MIN_HUMIDITY,
                        observer = meterologicalData.OBSERVER
                    }
                );
            }
        }

        public void AddConditioningLitter(int? litterId, Maternal_Condition_Experiment_Litters litter)
        {
            Logger.Info($"Insert Conditioning litter: {litter.Experiment_Number}");

            using (IDbConnection conn = new NpgsqlConnection(ConfigurationManager
                .ConnectionStrings["postgresConnectionString"]
                .ConnectionString))
            {
                conn.Execute($@"INSERT INTO mongoose.maternal_conditioning_litter(
	 litter_id, experiment_number, pregnancy_check_date, date_started, experiment_type, foetus_age, ""number_T_females"", ""number_C_females"", notes)
	VALUES (  @litter_id, @experiment_number, @pregnancy_check_date, @date_started, @experiment_type, @foetus_age, @number_T_females, @number_C_females, @notes);",
                    new
                    {
                        litter_id = litterId,
                        experiment_number = litter.Experiment_Number,
                        pregnancy_check_date = litter.Preg_check_trap_date,
                        date_started = litter.Date_started,
                        experiment_type = litter.Type_of_experiment,
                        foetus_age = litter.Foetus_age_at_start_weeks,
                        number_T_females = litter.No_of_T_females,
                        number_C_females = litter.No_of_C_females,
                        notes = litter.Notes
                    }
                );
            }
        }

        public void AddConditioningFemale(int packHistoryId, int? pairedFemaleId, int? litterId,
            Maternal_Condition_Experiment_Females female)

        {
            Logger.Info($"Insert Conditioning litter: {female.Female_ID}");

            using (IDbConnection conn = new NpgsqlConnection(ConfigurationManager
                .ConnectionStrings["postgresConnectionString"]
                .ConnectionString))
            {
                conn.Execute($@"INSERT INTO mongoose.maternel_conditioning_females(
	                             pack_history_id,litter_id, experiment_type, catagory, paired_female_id, notes)
	                            VALUES ( @pack_history_id, @litter_id, @experiment_type, @catagory, @paired_female_id, @notes);",
                    new
                    {
                        pack_history_id = packHistoryId,
                        litter_id = litterId,
                        experiment_type = female.Experiment_type,
                        catagory = female.Category,
                        paired_female_id = pairedFemaleId,
                        notes = female.Notes
                    }
                );
            }
        }

        public void insertProvisioning(int packHistoryId, int? litterId, ProvisioningData provisioning)
        {
            Logger.Info($"Insert Provisioning for  : {provisioning.Female_ID}");

            using (IDbConnection conn = new NpgsqlConnection(ConfigurationManager
                .ConnectionStrings["postgresConnectionString"]
                .ConnectionString))
            {
                conn.Execute(@"INSERT INTO mongoose.provisioning_data(
	             pack_history_id, litter_id, date, visit_time, egg_weight, comments)
	            VALUES (@pack_history_id, @litter_id, @date, @visit_time, @egg_weight, @comments);",
                    new
                    {
                        pack_history_id = packHistoryId,
                        litter_id = litterId,
                        date = provisioning.Date,
                        visit_time = provisioning.Visit_time,
                        egg_weight = provisioning.Amount_of_egg,
                        comments = provisioning.Comments
                    }
                );
            }
        }

        public void AddBloodData(int? mongooseId, TimeSpan bleedtime, Jennis_blood_data blood)
        {
            Logger.Info($"Insert blood data for: {blood.Mongoose}");

            using (IDbConnection conn = new NpgsqlConnection(ConfigurationManager
                .ConnectionStrings["postgresConnectionString"]
                .ConnectionString))
            {
                conn.Execute(@"INSERT INTO mongoose.blood_data(
	 individual_id, date, trap_time, bleed_time, weight, release_time, sample, spinning_time, freeze_time, focal, plasma_volume_ul, comment)
	VALUES (@individual_id, @date, @trap_time, @bleed_time, @weight, @release_time, @sample, @spinning_time, @freeze_time, @focal, @plasma_volume_ul, @comment);",
                    new
                    {
                        individual_id = mongooseId,
                        date = blood.Date,
                        trap_time = blood.Trap_time,
                        bleed_time = bleedtime,
                        weight = blood.Weight,
                        release_time = blood.Release_time,
                        sample = blood.Sample,
                        spinning_time = blood.Spinning_time,
                        freeze_time = blood.Freeze_time,
                        focal = blood.Focal,
                        plasma_volume_ul = blood.Amount_of_plasma,
                        comment = blood.Comment
                    }
                );
            }
        }

        public void InsertHpaSample(int? individualId, TimeSpan firstBloodTime, TimeSpan secondBloodTime, TimeSpan timeInTrap,
            HPA_samples hpaSample)
        {
            Logger.Info($"Insert HPA sample data for: {hpaSample.ID}");

            using (IDbConnection conn = new NpgsqlConnection(ConfigurationManager
                .ConnectionStrings["postgresConnectionString"]
                .ConnectionString))
            {
                conn.Execute(@"INSERT INTO mongoose.hpa_sample(
	 individual_id, date, time_in_trap, capture_time, first_blood_sample_taken_time, first_sample_id, first_blood_sample_freezer_time, second_blood_sample_taken_time, second_blood_sample_id, second_blood_sample_freezer_time, head_width, weight, ticks)
	VALUES ( @individual_id, @date, @time_in_trap, @capture_time, @first_blood_sample_taken_time, @first_sample_id, @first_blood_sample_freezer_time, @second_blood_sample_taken_time, @second_blood_sample_id, @second_blood_sample_freezer_time, @head_width, @weight, @ticks);",
                    new
                    {
                        individual_id = individualId,
                        date = hpaSample.Date,
                        time_in_trap = timeInTrap,
                        capture_time = hpaSample.Time_of_capture,
                        first_blood_sample_taken_time = firstBloodTime,
                        first_sample_id = hpaSample.First_sample_number,
                        first_blood_sample_freezer_time = hpaSample.First_sample_freezer_time,
                        second_blood_sample_taken_time = secondBloodTime,
                        second_blood_sample_id = hpaSample.Second_sample_number,
                        second_blood_sample_freezer_time = hpaSample.Second_sample_freezer_time,
                        head_width = hpaSample.Head_width,
                        weight = hpaSample.Weight,
                        ticks = hpaSample.Ticks
                    }
                );
            }
        }

        public void AddDnaSample(int packHistoryId, int? litterId, DNA_SAMPLES dnaSample)
        {
            Logger.Info($"Insert DNA sample for: {dnaSample.ID}");

            using (IDbConnection conn = new NpgsqlConnection(ConfigurationManager
                .ConnectionStrings["postgresConnectionString"]
                .ConnectionString))
            {
                conn.Execute(@"INSERT INTO mongoose.dna_samples(
	 pack_history_id,litter_id, date, sample_type, tissue, storage, tube_id, age, dispersal, box_slot, comment)
	VALUES (@pack_history_id, @litter_id, @date, @sample_type, @tissue, @storage, @tube_id, @age, @dispersal, @box_slot, @comment);",
                    new
                    {
                        pack_history_id = packHistoryId,
                        litter_id = litterId,
                        date = dnaSample.DATE,
                        sample_type = dnaSample.SAMPLE_TYPE,
                        tissue = dnaSample.TISSUE,
                        storage = dnaSample.STORAGE_ID,
                        tube_id = dnaSample.TUBE_ID,
                        age = dnaSample.AGE,
                        dispersal = dnaSample.DISPERSAL,
                        box_slot = dnaSample.box_slot,
                        comment = dnaSample.COMMENT
                    }
                );
            }
        }

        public void AddAntiParasite(int packHistoryId, Antiparasite_experiment antiparasiteExperiment)
        {
            Logger.Info($"Insert anti parasite experiment for: {antiparasiteExperiment.INDIV}");

            using (IDbConnection conn = new NpgsqlConnection(ConfigurationManager
                .ConnectionStrings["postgresConnectionString"]
                .ConnectionString))
            {
                conn.Execute(@"INSERT INTO mongoose.anti_parasite(
	  pack_history_id, started_date, ""fecal_sample_A_date"", first_capture_date, experiment_group, ""fecal_sample_B_date"", ""fecal_sample_C_date"", second_capture, ""fecal_sample_D_date"", ""fecal_sample_E_date"", ""fecal_sample_F_date"", comments)
    VALUES ( @pack_history_id, @started_date, @fecal_sample_A_date, @first_capture_date, @experiment_group, @fecal_sample_B_date, @fecal_sample_C_date, @second_capture, @fecal_sample_D_date, @fecal_sample_E_date, @fecal_sample_F_date, @comments);",
                    new
                    {
                        pack_history_id = packHistoryId,
                        started_date = antiparasiteExperiment.STARTED_EXPERIMENT,
                        fecal_sample_A_date = antiparasiteExperiment.A_FECAL_SAMPLE,
                        first_capture_date = antiparasiteExperiment.FIRST_capture,
                        experiment_group = antiparasiteExperiment.EXPERIMENT_GROUP,
                        fecal_sample_B_date = antiparasiteExperiment.B_FECAL,
                        fecal_sample_C_date = antiparasiteExperiment.C_FECAL,
                        second_capture = antiparasiteExperiment.SECOND_CAPTURE,
                        fecal_sample_D_date = antiparasiteExperiment.D_FECAL,
                        fecal_sample_E_date = antiparasiteExperiment.E_FECAL,
                        fecal_sample_F_date = antiparasiteExperiment.F_FECAL,
                        comments = antiparasiteExperiment.notes
                    }
                );
            }
        }

        public void AddOxFeedingRecord(int packHistoryId, OxShieldingFeedingRecord feedingRecord)
        {
            Logger.Info($"Insert ox feeding record for for: {feedingRecord.Female_ID}");

            using (IDbConnection conn = new NpgsqlConnection(ConfigurationManager
                .ConnectionStrings["postgresConnectionString"]
                .ConnectionString))
            {
                conn.Execute(@"INSERT INTO mongoose.ox_shielding_feeding(
	 pack_history_id, date, time_of_day, amount_of_egg, comments)
	VALUES (@pack_history_id, @date, @time_of_day, @ammount_of_egg, @comments);",
                    new
                    {
                        pack_history_id = packHistoryId,
                        date = feedingRecord.Date,
                        time_of_day = feedingRecord.AMPM.ToUpper(),
                        ammount_of_egg = feedingRecord.Amount_of_egg,
                        comments = feedingRecord.Comments
                    }
                );
            }
        }

        public void AddOxMaleBeingSampled(int packHistoryId, OxShieldingMalesBeingSampled malesBeingSampled)
        {
            Logger.Info($"Insert ox male for: {malesBeingSampled.ID}");

            using (IDbConnection conn = new NpgsqlConnection(ConfigurationManager
                .ConnectionStrings["postgresConnectionString"]
                .ConnectionString))
            {
                conn.Execute(@"INSERT INTO mongoose.ox_shielding_male(
	 pack_history_id, status, start_date, stop_date, comment)
	VALUES ( @pack_history_id, @status, @start_date, @stop_date, @comment);",
                    new
                    {
                        pack_history_id = packHistoryId,
                        status = malesBeingSampled.STATUS,
                        start_date = malesBeingSampled.DATE_START,
                        stop_date = malesBeingSampled.DATE_STOP,
                        comment = malesBeingSampled.COMMENT
                    }
                );
            }
        }

        public void AddOxFemaleTreatmentGroup(int packHistoryId, OxShieldingFemaleTreatmentGroups femaleTreatmentGroup)
        {
            Logger.Info($"Insert ox male for: {femaleTreatmentGroup.ID}");

            using (IDbConnection conn = new NpgsqlConnection(ConfigurationManager
                .ConnectionStrings["postgresConnectionString"]
                .ConnectionString))
            {
                conn.Execute(@"INSERT INTO mongoose.ox_shielding_group(
	 pack_history_id, treatment_group, start_date, comment)
	VALUES (@pack_history_id, @treatment_group, @start_date, @comment);",
                    new
                    {
                        pack_history_id = packHistoryId,
                        treatment_group = femaleTreatmentGroup.Treatment_Group,
                        start_date = femaleTreatmentGroup.Date_Started,
                        comment = femaleTreatmentGroup.Comment
                    }
                );
            }
        }

        public bool PackHistoryExitsAlready(int packId, int individualId, DateTime? date)
        {
            List<int> list;
            using (IDbConnection conn = new NpgsqlConnection(ConfigurationManager
                .ConnectionStrings["postgresConnectionString"]
                .ConnectionString))

            {

                list = conn.Query<int>(@"SELECT pack_history_id, pack_id, individual_id, date_joined
	                                    FROM mongoose.pack_history
                                        where pack_id = @pack_id and individual_id = @individual_id and date_joined is not distinct from @date_joined;",
                    new { pack_id = packId, individual_id = individualId, date_joined = date }).ToList();
            }

            return list.Count > 0;
        }

        public List<PackHistory> GetAllPackHistoriesForIndividual(int individualId)
        {
            using (IDbConnection conn = new NpgsqlConnection(ConfigurationManager
                .ConnectionStrings["postgresConnectionString"]
                .ConnectionString))

            {
                return conn.Query<PackHistory>(
                    @"SELECT pack_history_id as PackHistoryId, pack_id as PackId, individual_id as IndividualId, date_joined as DateJoined
	                    FROM mongoose.pack_history
                        where  individual_id = @individual_id;",
                    new { individual_id = individualId }).ToList();
            }
        }

        public List<string> GetPreviousNamesForIndividual(int individualId)
        {
            using (IDbConnection conn = new NpgsqlConnection(ConfigurationManager
                .ConnectionStrings["postgresConnectionString"]
                .ConnectionString))
            {
                return conn.Query<string>(
                    @"SELECT name  
                      FROM mongoose.individual_name_history
                      WHERE individual_id = @individual_id",
                    new { individual_id = individualId }).ToList();
            }
        }

        public void AddPreviousNameForIndividual(int individualId, string oldName)
        {
            using (IDbConnection conn = new NpgsqlConnection(ConfigurationManager
                .ConnectionStrings["postgresConnectionString"]
                .ConnectionString))

            {
                conn.Execute(
                    @"INSERT INTO mongoose.individual_name_history(individual_id, name)
	                  VALUES (@IndividualId, @name); ",
                    new { IndividualId = individualId, name = oldName });

            }
        }
    }
}
