using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Dapper;
using DataPipe.Main.Model;
using DataPipe.Main.Model.LifeHistory;
using Microsoft.Data.Sqlite;
using NLog;
using NLog.Config;



namespace DataPipe.Main
{
    public class Data
    {
        private static Logger logger;

        public static IDbConnection SimpleDbConnection()
        {
            LogManager.Configuration = new XmlLoggingConfiguration("nlog.config");
            logger = LogManager.GetLogger("Data");
            var dbFile = GetAppSettings.Get().SqliteLocation;

            if (File.Exists(dbFile))
            {
                logger.Info($"Database found at '{dbFile}' ");
            }
            else
            {
                logger.Error($"Database not found at '{dbFile}'");

                throw new FileNotFoundException($"database not located at '{dbFile}'");
            }

            return new SqliteConnection($"Data Source={dbFile};");
        }

        public static void MarkAsSent<T>(T message) where T : class, ISendable
        {
            LogManager.Configuration = new XmlLoggingConfiguration("nlog.config");
            logger = LogManager.GetLogger("Data");

            if (message.entity_type == "mongoose" || message.entity_type == "pack")
            {
                using (var cnn = SimpleDbConnection())
                {
                    logger.Info(
                        $"Marking {message} as sent. Type = '{message.entity_type}' entity_id = '{message.entity_id}'");

                    cnn.Open();

                    var t = cnn.BeginTransaction();

                    cnn.Execute("update sync_entity set sent = 1 where entity_id = @entity_id",
                        new { entity_id = message.entity_id });
                    cnn.Execute("update sync_value_int set sent = 1 where entity_id = @entity_id",
                        new { entity_id = message.entity_id });
                    cnn.Execute("update sync_value_real set sent = 1 where entity_id = @entity_id",
                        new { entity_id = message.entity_id });
                    cnn.Execute("update sync_value_varchar set sent = 1 where entity_id = @entity_id",
                        new { entity_id = message.entity_id });

                    t.Commit();
                }
            }
            else
            {
                using (var cnn = SimpleDbConnection())
                {
                    logger.Info(
                        $"Marking {message} as sent. Type = '{message.entity_type}' entity_id = '{message.entity_id}'");

                    cnn.Open();
                    var t = cnn.BeginTransaction();

                    cnn.Execute("update stream_entity set sent = 1 where entity_id = @entity_id",
                        new { entity_id = message.entity_id });
                    cnn.Execute("update stream_value_int set sent = 1 where entity_id = @entity_id",
                        new { entity_id = message.entity_id });
                    cnn.Execute("update stream_value_real set sent = 1 where entity_id = @entity_id",
                        new { entity_id = message.entity_id });
                    cnn.Execute("update stream_value_varchar set sent = 1 where entity_id = @entity_id",
                        new { entity_id = message.entity_id });

                    t.Commit();
                }
            }
        }

        private static IEnumerable<T> RunSql<T>(string sql) where T : new()
        {
            LogManager.Configuration = new XmlLoggingConfiguration("nlog.config");
            var logger = LogManager.GetLogger("Data");

            using (var cnn = SimpleDbConnection())
            {
                List<T> result;
                try
                {
                    result = cnn.Query<T>(sql).ToList();
                }
                catch (Exception exception)
                {
                    logger.Error(exception, $"Could not get data from the database conn = '{cnn.ConnectionString}' sql='{sql}'");
                    result = new List<T>();
                }

                return result;
            }
        }

        public static IEnumerable<IndividualCreated> GetUnsyncedIndividuals()
        {
            var newIndividuals = new List<IndividualCreated>();
            var stringsSql = @"
                        select se.entity_id, se.unique_id as unique_id,se.entity_type, svv.attribute_id as attribute_id ,svv.value
                        from sync_entity se
                        join sync_value_varchar svv on se.entity_id = svv.entity_id
                        where se.entity_type = ""mongoose"" and se.sent = 0
                        group by    se.unique_id , svv.attribute_id ,svv.value;";
            var strings = RunSql<DatabaseRow<string>>(stringsSql).ToList();

            var longSql = @"
                        select se.entity_id, se.unique_id as unique_id, se.entity_type, svr.attribute_id ,svr.value 
                        from sync_entity se
                        join sync_value_real svr on se.entity_id = svr.entity_id
                        where se.entity_type = ""mongoose""  and se.sent = 0
                        group by    se.unique_id , svr.attribute_id ,svr.value;";
            var longs = RunSql<DatabaseRow<double>>(longSql).ToList();

            foreach (var uniqueCode in longs.Select(l => l.unique_id).Distinct())
            {
                var newIndividual = new IndividualCreated
                {
                    entity_id = strings.SingleOrDefault(s => s.attribute_id == "name" && s.unique_id == uniqueCode)
                        .entity_id,
                    UniqueId = uniqueCode,
                    entity_type = strings.SingleOrDefault(s => s.attribute_id == "name" && s.unique_id == uniqueCode)
                        ?.entity_type,
                    ChipCode = strings.SingleOrDefault(s => s.attribute_id == "chip-code" && s.unique_id == uniqueCode)
                        ?.value,
                    CollerWeight =
                        longs.SingleOrDefault(s => s.attribute_id == "collar-weight" && s.unique_id == uniqueCode)
                            ?.value,
                    DateOfBirth =
                        GetDate(
                            strings.SingleOrDefault(s => s.attribute_id == "dob" && s.unique_id == uniqueCode)?.value),
                    Gender =
                        strings.SingleOrDefault(s => s.attribute_id == "gender" && s.unique_id == uniqueCode)?.value,
                    Name = strings.SingleOrDefault(s => s.attribute_id == "name" && s.unique_id == uniqueCode)?.value,
                    LitterCode =
                        strings.SingleOrDefault(s => s.attribute_id == "litter-code" && s.unique_id == uniqueCode)
                            ?.value,
                    PackCode = GetPackName(strings
                        .SingleOrDefault(s => s.attribute_id == "pack-id" && s.unique_id == uniqueCode)?.value),
                    PackUniqueId = strings
                        .SingleOrDefault(s => s.attribute_id == "pack-id" && s.unique_id == uniqueCode)
                        ?.value
                };
                if (!string.IsNullOrEmpty(newIndividual.Name))
                {
                    if (newIndividual.DateOfBirth != null && newIndividual.DateOfBirth != DateTime.MinValue)
                    {
                        newIndividuals.Add(newIndividual);
                    }
                }
            }

            return newIndividuals;
        }

        private static string GetPackName(string packUniqueId)
        {
            var sql = $@"
                        select svv.value from sync_entity se 
                        join sync_value_varchar svv on se.entity_id = svv.entity_id
                        where se.unique_id = ""{packUniqueId}"" and svv.attribute_id = ""name""";

            var name = RunSql<DatabaseRow<string>>(sql).FirstOrDefault()?.value;

            return name ?? "Unknown Pack";
        }

        private static DateTime? GetDate(string dateString)
        {
            if (DateTime.TryParse(dateString, out var dateTime))
            {
                return dateTime;
            }

            return null;
        }

        public static IEnumerable<PackCreated> GetNewPacks()
        {
            var newPacks = new List<PackCreated>();
            var stringsSql = @"
                        select se.entity_id, se.unique_id as unique_id,se.entity_type, svv.attribute_id as attribute_id ,svv.value
                        from sync_entity se
                        join sync_value_varchar svv on se.entity_id = svv.entity_id
                        where se.entity_type = ""pack"" and se.sent = 0
                        group by se.unique_id , svv.attribute_id ,svv.value;";
            var strings = RunSql<DatabaseRow<string>>(stringsSql).ToList();

            foreach (var uniqueId in strings.Select(s => s.unique_id).Distinct())
            {
                var pack = new PackCreated
                {
                    Name = strings.SingleOrDefault(s => s.unique_id == uniqueId && s.attribute_id == "name")?.value,
                    UniqueId = uniqueId,
                    entity_id = strings.SingleOrDefault(s => s.unique_id == uniqueId && s.attribute_id == "name")
                        .entity_id,
                    entity_type = strings.SingleOrDefault(s => s.unique_id == uniqueId && s.attribute_id == "name")
                        ?.entity_type
                };
                if (DateTime.TryParse(
                    strings.SingleOrDefault(s => s.unique_id == uniqueId && s.attribute_id == "time")?.value,
                    out var dateCreated))
                {
                    pack.CreatedDate = dateCreated;
                }

                if (!string.IsNullOrEmpty(pack.Name) && pack.CreatedDate != null)
                {
                    newPacks.Add(pack);
                }
            }

            return newPacks;
        }

        public static IEnumerable<WeightMeasure> GetUnsyncedWeights()
        {
            var newWeights = new List<WeightMeasure>();
            var stringSql = @"
                         select se.entity_type, se.entity_id, se.unique_id, svv.attribute_id, svv.value 
                         from stream_entity se
                         join stream_value_varchar svv on se.entity_id = svv.entity_id
                         where se.entity_type = ""group-comp-weight"" and se.sent = 0
                         ";

            var strings = RunSql<DatabaseRow<string>>(stringSql).ToList();

            var intSql = @" select se.entity_type, se.entity_id, se.unique_id, svi.attribute_id, svi.value 
                             from stream_entity se
                             join stream_value_int svi on se.entity_id = svi.entity_id
                            where se.entity_type = ""group-comp-weight"" and se.sent = 0";
            var ints = RunSql<DatabaseRow<int>>(intSql).ToList();

            var realSql = @" select se.entity_type, se.entity_id, se.unique_id, svi.attribute_id, svi.value 
                             from stream_entity se
                             join stream_value_real svi on se.entity_id = svi.entity_id
                            where se.entity_type = ""group-comp-weight"" and se.sent = 0";
            var reals = RunSql<DatabaseRow<double>>(realSql).ToList();

            // get relevent ids for weights

            foreach (var unique_id in strings.Select(s => s.unique_id).Distinct())
            {
                using (var conn = SimpleDbConnection())
                {
                    var individualInfo = conn.Query<dynamic>(@"
                                            select svv.value as indivName, se.unique_id as indivUnique from sync_entity se
                                            join sync_value_varchar svv on se.entity_id = svv.entity_id
                                                where se.unique_id in (select svv.value  from stream_entity se 
                                                join stream_value_varchar svv on svv.entity_id = se.entity_id
                                            where se.unique_id = @unique_id and svv.attribute_id = ""id-mongoose"")
                                            and svv.attribute_id = ""name""",
                        new { unique_id }).FirstOrDefault();
                    var parentId = strings.SingleOrDefault(s => s.unique_id == unique_id && s.attribute_id == "parent")
                        ?.value;
                    var packInfo = conn.Query<dynamic>(
                        @"select svv.value as packName, se.unique_id as packUniqueId from sync_entity se
                                        join sync_value_varchar svv on se.entity_id = svv.entity_id
                                        where se.unique_id in (select svv.value as packUniqueId from stream_entity se
                                        join stream_value_varchar svv on se.entity_id = svv.entity_id
                                        where se.unique_id = @unique_id and svv.attribute_id = ""pack"")
                                        and svv.attribute_id = ""name""",
                        new { unique_id = parentId }).FirstOrDefault();
                    var newWeight = new WeightMeasure
                    {
                        UniqueId = unique_id,
                        IndividualName = individualInfo?.indivName,
                        IndividualUniqueId = individualInfo?.indivUnique,
                        PackId = packInfo?.packName,
                        PackUniqueId = packInfo?.packUniqueId,
                        Accurate = ints.SingleOrDefault(s => s.unique_id == unique_id && s.attribute_id == "accurate")
                            ?.value,
                        Weight = reals.SingleOrDefault(s => s.unique_id == unique_id && s.attribute_id == "weight")
                            .value,
                        CollarWeight = reals
                            .SingleOrDefault(s => s.unique_id == unique_id && s.attribute_id == "cur-collar-weight")
                            ?.value,
                        Latitude = reals.SingleOrDefault(s => s.unique_id == unique_id && s.attribute_id == "lat")
                            ?.value,
                        Longitude = reals.SingleOrDefault(s => s.unique_id == unique_id && s.attribute_id == "lon")
                            ?.value,
                        Time = GetDate(strings
                            .SingleOrDefault(s => s.unique_id == unique_id && s.attribute_id == "time")?.value),
                        entity_id = reals.SingleOrDefault(s => s.unique_id == unique_id && s.attribute_id == "weight")
                            .entity_id,
                        entity_type = reals
                            .SingleOrDefault(s => s.unique_id == unique_id && s.attribute_id == "weight")
                            .entity_type
                    };

                    newWeights.Add(newWeight);
                }
            }

            return newWeights;
        }

        public static IEnumerable<LifeHistoryEvent> GetLifeHistoryEvents()
        {
            var stringsSql = @"select 
	                             se.entity_id,
	                             se.entity_type,
	                             (select value from stream_value_varchar where entity_id = se.entity_id and attribute_id = ""date"") as ""Date"",
	                             (select value from stream_value_varchar where entity_id = se.entity_id and attribute_id = ""type"") as ""Type"",
	                             (select value from stream_value_varchar where entity_id = se.entity_id and attribute_id = ""code"") as ""Code"",
	                             (select value from stream_value_varchar where entity_id = se.entity_id and attribute_id = ""entity-uid"") as ""UniqueId"",
	                             (select value from stream_value_varchar where entity_id = se.entity_id and attribute_id = ""entity-name"") as ""entity_name"",
	                             se.sent
                            from stream_entity se
                            where se.entity_type = ""lifehist-event"" and se.sent = 0;";
            var lifeEvents = RunSql<LifeHistoryEvent>(stringsSql).ToList();



            return lifeEvents;
        }
    }

    public class DatabaseRow<T>
    {
        public string entity_type { get; set; }
        public int entity_id { get; set; }
        public string unique_id { get; set; }
        public string attribute_id { get; set; }
        public T value { get; set; }
    }
}