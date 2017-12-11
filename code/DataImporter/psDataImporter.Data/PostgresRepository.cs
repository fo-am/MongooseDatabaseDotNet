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
                        cmd.Parameters.AddWithValue("Accuracy", (object) weight.Accuracy ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("Session", (object) weight.Session ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("CollarWeight", (object) weight.Collar ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("Comment", (object) weight.Comment ?? DBNull.Value);

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
                    new {packName = pack, individualName = individual});
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
                    new {PackId = packId, IndividualId = individualId, DateJoined = date});

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
                    new {date, packHistoryId = packHistory.PackHistoryId});

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
                        new {codeValue = code});
                }
            }
        }

        public PackHistory GetPackHistory(int packId, int? individualId)
        {
            using (IDbConnection conn = new NpgsqlConnection(ConfigurationManager
                .ConnectionStrings["postgresConnectionString"]
                .ConnectionString))

            {
                return conn.Query<PackHistory>(
                    "SELECT pack_history_id as PackHistoryId, pack_id as PackId, individual_id as IndividualId, date_joined as DateJoined from mongoose.pack_history where pack_id = @pack and individual_id = @individual",
                    new {pack = packId, individual = individualId}).FirstOrDefault();
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
                        new {newIndividual.Name}).SingleOrDefault();

                if (inDatabaseIndividual != null)
                {
                    //If database has no sex, but our new individual does
                    if (string.IsNullOrEmpty(inDatabaseIndividual.Sex) && !string.IsNullOrEmpty(newIndividual.Sex))
                    {
                        Logger.Info(
                            $"Added sex: '{newIndividual.Sex}' to individiual with Id : '{inDatabaseIndividual.IndividualId}'");

                        conn.Execute("Update mongoose.Individual set sex = @sex where individual_id = @id",
                            new {sex = newIndividual.Sex, id = inDatabaseIndividual.IndividualId});
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
                    var notMongooses = new[] { "NONE", "ALL", "UNK", "ALLBS", "MOST", "INF", "NEXT LITTER", "Unknown", "SUB" };
                    if (notMongooses.Contains(newIndividual.Name))
                    {
                        isMongoose = false;
                    }
                    conn.ExecuteScalar<int>(
                        "Insert into mongoose.individual (name, sex, litter_id, is_mongoose) values (@name, @sex, @litterId, @is_mongoose) ON CONFLICT DO NOTHING",
                        new {newIndividual.Name, newIndividual.Sex, litterId = newIndividual.LitterId, is_mongoose = isMongoose });
                    Logger.Info($"created indivual : {newIndividual.Name}");
                }
            }
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
                    new {name = packName});
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

        public void RemoveRadioCollarData()
        {
            using (IDbConnection conn = new NpgsqlConnection(ConfigurationManager
                .ConnectionStrings["postgresConnectionString"]
                .ConnectionString))
            {
                conn.Execute("truncate mongoose.radiocollar");
                Logger.Info("Truncated radiocollar table");
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
                    new {pack_history_id, frequency, weight, fitted, turnedOn, removed, comment, dateEntered});
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
                    new {packid = litter.pgPackId, name = litter.Litter});

                //update individual with litter id
                conn.Execute(
                    "update mongoose.individual set litter_id = @litterId where individual_id = @individual_id",
                    new {litterId, individual_id = litter.pgIndividualId});

                Logger.Info($"Added litter {litter.Litter}.");
            }
        }

        public void AddPackEventCodes(IEnumerable<string> codes)
        {
            foreach (var code in codes)
            {
                AddPackEventCode(code);
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
                        new {codeValue = code});
                }
            }
        }

        public List<IndividualEventCode> GetIndividualCodes()
        {
            using (IDbConnection conn = new NpgsqlConnection(ConfigurationManager
                .ConnectionStrings["postgresConnectionString"]
                .ConnectionString))
            {
                return conn.Query<IndividualEventCode>(
                        "Select individual_event_code_id as IndividualEventCodeId, code from mongoose.individual_event_code")
                    .ToList();
            }
        }

        public List<PackEventCode> GetPackEventCodes()
        {
            using (IDbConnection conn = new NpgsqlConnection(ConfigurationManager
                .ConnectionStrings["postgresConnectionString"]
                .ConnectionString))
            {
                return conn.Query<PackEventCode>(
                        "Select pack_event_code_id as PackEventCodeId, code, detail from mongoose.pack_event_code")
                    .ToList();
            }
        }

        public int GetPackEventCodeId(string code)
        {
            using (IDbConnection conn = new NpgsqlConnection(ConfigurationManager
                .ConnectionStrings["postgresConnectionString"]
                .ConnectionString))
            {
                return conn.ExecuteScalar<int>(
                    "Select pack_event_code_id  from mongoose.pack_event_code where code = @code", new {code});
            }
        }

        public void LinkIndividualEvents(int pack_history_id, int individualEventCodeId, string latitude,
            string longitude,
            string status, DateTime date, string exact, string cause, string comment)
        {
            // create geography if lat and long are present.
            var locationString = LocationString(latitude, longitude);
          
            var sql =
                "Insert into mongoose.individual_event (pack_history_id, individual_event_code_id, status, date, exact, cause, comment, location )" +
                $" values (@pack_history_id, @individualEventCodeId, @status, @date, @exact, @cause, @Comment, {locationString})";

            using (IDbConnection conn = new NpgsqlConnection(ConfigurationManager
                .ConnectionStrings["postgresConnectionString"]
                .ConnectionString))
            {
                Logger.Info(
                    $"Linking individualId: {pack_history_id} with Individual Event codeId: {individualEventCodeId}");
                conn.Execute(sql, new {pack_history_id, individualEventCodeId, status, date, exact, cause, comment});
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
                conn.Execute(sql, new {packId, packEventCodeId, status, date, exact, cause, comment});
            }
        }

        public int GetPackId(string packName)
        {
            using (IDbConnection conn = new NpgsqlConnection(ConfigurationManager
                .ConnectionStrings["postgresConnectionString"]
                .ConnectionString))
            {
                return conn.ExecuteScalar<int>(
                    "Select pack_id from mongoose.pack where name = @packName", new {packName});
            }
        }

        public int? TryGetPackId(string packName)
        {
            using (IDbConnection conn = new NpgsqlConnection(ConfigurationManager
                .ConnectionStrings["postgresConnectionString"]
                .ConnectionString))
            {
                return conn.ExecuteScalar<int?>(
                    "Select pack_id from mongoose.pack where name = @packName", new {packName});
            }
        }

        public int? GetIndividualId(string individualName)
        {
            if (string.IsNullOrEmpty(individualName))
            {
                return null;
            }

            using (IDbConnection conn = new NpgsqlConnection(ConfigurationManager
                .ConnectionStrings["postgresConnectionString"]
                .ConnectionString))
            {
                return conn
                    .ExecuteScalar<int>("Select individual_id from mongoose.individual where name = @individualName",
                        new {individualName});
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
                        new {lifeHistoryCode = individualEventCode});
            }
        }

        public void AddOestrusEvent(Oestrus oestrus)
        {
            //get pack history id (femailid and pack id)
            var packHistoryId = GetPackHistoryId(oestrus.GROUP, oestrus.FEMALE_ID);
            var femailId = GetIndividualId(oestrus.FEMALE_ID);
            //guardId
            var guardid = GetIndividualId(oestrus.GUARD_ID);
            //pesterir 1-4 id
            var pesterer1Id = GetIndividualId(oestrus.PESTERER_ID);
            var pesterer2Id = GetIndividualId(oestrus.PESTERER_ID_2);
            var pesterer3Id = GetIndividualId(oestrus.PESTERER_ID_3);
            var pesterer4Id = GetIndividualId(oestrus.PESTERER_ID_4);

            var locationString = LocationString(oestrus.Latitude, oestrus.Longitude);

            var sql = $"INSERT INTO mongoose.oestrus("
                      + $"pack_history_id, oestrus_code, guard_id, pesterer_id_1, pesterer_id_2, pesterer_id_3, pesterer_id_4, strength, confidence, copulation, location, comment)"
                      + $"VALUES( @pack_history_id, @oestrus_code, @guard_id, @pesterer_id_1, @pesterer_id_2, @pesterer_id_3, @pesterer_id_4, @strength, @confidence, @copulation, {locationString}, @comment)";

            using (IDbConnection conn = new NpgsqlConnection(ConfigurationManager
                .ConnectionStrings["postgresConnectionString"]
                .ConnectionString))
            {
                Logger.Info($"Adding Oestrus event. {oestrus.OESTRUS_CODE} ");
                conn.Execute(sql, new
                {
                    pack_history_id = packHistoryId,
                    oestrus_code = oestrus.OESTRUS_CODE,
                    guard_id = guardid,
                    pesterer_id_1 = pesterer1Id,
                    pesterer_id_2 = pesterer2Id,
                    pesterer_id_3 = pesterer3Id,
                    pesterer_id_4 = pesterer4Id,
                    strength = oestrus.STRENGTH,
                    confidence = oestrus.CONFIDENCE,
                    copulation = oestrus.CONFIDENCE,
                    comment = oestrus.COMMENT
                });
            }
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
                    new {name = litterName, packId = packId});
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
                    new {name = litterName});
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
                        new {codeValue = code});
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
                    new {code = litterEventCode});
            }
        }

        public void LinkLitterEvent(int litterId, int litterEventCodeId, NewLifeHistory lifeHistory)
        {
            Logger.Info($"Linking litter with event");

            using (IDbConnection conn = new NpgsqlConnection(ConfigurationManager
                .ConnectionStrings["postgresConnectionString"]
                .ConnectionString))
            {
                conn.Execute(
                    "Insert into mongoose.litter_event (litter_id, litter_event_code_id, cause, exact, last_seen, comment) values (@litterId, @litterEventCodeId, @cause, @exact, @lastSeen, @comment) ON CONFLICT DO NOTHING",
                    new
                    {
                        litterId = litterId,
                        litterEventCodeId = litterEventCodeId,
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

            // , / or 
            var secondpacks = lifeHistory.Cause.Split(new[] {"/", ",", " OR ", " or "}, StringSplitOptions.None);

            foreach (var secondpack in secondpacks)
            {
                if (secondpack.Contains("UNK"))
                {
                    continue;
                }
                InsertSinglePack(secondpack);
            }

            var secondPackIds = secondpacks.Select(GetPackId).Select(packId => (int?) packId).ToList();

            if (lifeHistory.Cause.Contains("UNK"))
            {
                secondPackIds.Clear();
                secondPackIds.Add(null);
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
            var yesValues = new[] {"Y", "YES", "YE S", "YEYS", "1"};
            var noValues = new[] {"N", "NO", "0", "-1"};


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
                           pup_pack_history_id, escort_id, date, strength, confidence, location, comment )
                            VALUES (@pup_pack_history_id, @escort_id, @date, @strength, @confidence, {locationString}, @comment)"
                    ,
                    new
                    {
                        pup_pack_history_id = packHistoryId,
                        escort_id = escortId,
                        date = pupAssociation.DATE,
                        strength = pupAssociation.STRENGTH,
                        confidence = pupAssociation.CONFIDENCE,
                        comment = pupAssociation.COMMENT + " " + pupAssociation.Editing_comments
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
                            VALUES (@babysitter_pack_history_id, @date, @litter_id, @type, @time_start, @den_distance, @time_end, @accuracy, @comment,  {locationString})"
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
            if (!string.IsNullOrEmpty(latitude) && !string.IsNullOrEmpty(longitude) && latitude != "0" && longitude != "0")
            {
                locationString = $"ST_GeographyFromText('SRID=4326;POINT({latitude} {longitude})')";
            }
            return locationString;
        }
    }
}