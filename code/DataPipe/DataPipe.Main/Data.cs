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

            var file = new FileInfo(dbFile);
            Console.WriteLine(file.Directory);

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

            if (message.entity_type == "mongoose" || message.entity_type == "pack"|| message.entity_type == "litter")
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
            var newIndividualsSql = @"
                                    select se.entity_id, 
                                    se.unique_id as 'UniqueId',
                                    se.sent,
                                    se.entity_type,
                                    svvName.value as 'Name',
                                    svvChip.value as 'ChipCode'	,
                                    svvDob.value as 'DateOfBirthString',
                                    svvLitter.value as 'litter_code',
                                    svvGender.value as 'Gender',
                                    svvPack.value as 'PackUniqueId',
                                    (select svv.value as PackCode
                                    from sync_entity se
                                    join sync_value_varchar svv on se.entity_id = svv.entity_id and svv.attribute_id = 'name'
                                    where se.unique_id = svvPack.value ) as 'PackCode',
                                    svrColler.value as CollerWeight,
                                    svrLat.value as Latitude,
                                    svrLon.value as Longitude
                                    from sync_entity se 
                                    left join sync_value_varchar svvName on svvName.entity_id = se.entity_id and svvName.attribute_id = 'name'
                                    left join sync_value_varchar svvChip on svvchip.entity_id = se.entity_id and svvchip.attribute_id = 'chip-code'
                                    left join sync_value_varchar svvDob on svvDob.entity_id = se.entity_id and svvDob.attribute_id = 'dob'
                                    left join sync_value_varchar svvGender on svvGender.entity_id = se.entity_id and svvGender.attribute_id = 'gender'
                                    left join sync_value_varchar svvLitter on svvLitter.entity_id = se.entity_id and svvLitter.attribute_id = 'litter-code'
                                    left join sync_value_varchar svvPack on svvPack.entity_id = se.entity_id and svvPack.attribute_id = 'pack-id'
                                    left join sync_value_real svrColler on svrColler.entity_id = se.entity_id and svrColler.attribute_id = 'collar_weight'
                                    left join sync_value_real svrLat on svrLat.entity_id = se.entity_id and svrLat.attribute_id = 'lat'
                                    left join sync_value_real svrLon on svrLon.entity_id = se.entity_id and svrLon.attribute_id = 'lon'
                                    
                                    where se.entity_type = 'mongoose' and se.sent = 0";
             var newIndividuals = RunSql<IndividualCreated>(newIndividualsSql).ToList();

            foreach (var individualCreated in newIndividuals)
            {
                if (DateTime.TryParse(individualCreated.DateOfBirthString, out var dateOfBirth))
                {
                    individualCreated.DateOfBirth = dateOfBirth;
                }
            }
        

            return newIndividuals;
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
            var ints = RunSql<DatabaseRow<int?>>(intSql).ToList();

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
            var stringSql = @"select 
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
            var lifeEvents = RunSql<LifeHistoryEvent>(stringSql).ToList();



            return lifeEvents;
        }

        public static IEnumerable<LitterCreated> GetUnsynchedLitters()
        {
            var stringSql = @"select 
                             (select value from sync_value_varchar where entity_id = se.entity_id and attribute_id = 'date') as 'Date',
                             (select value from sync_value_varchar where entity_id = se.entity_id and attribute_id = 'name') as 'LitterName',
                             (select value from sync_value_varchar where entity_id = se.entity_id and attribute_id = 'unique_id') as 'UniqueId',
                             (select svv.value from sync_entity se
                                join sync_value_varchar svv on svv.entity_id = se.entity_id
                                where se.unique_id = svvPackId.value and svv.attribute_id = 'name') as 'PackName',
                             svvPackId.value as 'PackUniqueId',
                             'litter' as entity_type,
                             se.entity_id
                             from sync_entity se 
                             join sync_value_varchar svvPackId on svvPackId.entity_id = se.entity_id and svvPackId.attribute_id = 'parent'
                             where se.entity_type = 'litter' and se.sent = 0;";

            var lifeEvents = RunSql<LitterCreated>(stringSql).ToList();
            return lifeEvents; 
        }

        public static IEnumerable<PackMove> GetUnsynchedPackMoves()
        {
            var stringSql = @"select entity_id, entity_type, unique_id,
                            (select value from stream_value_varchar where entity_id = se.entity_id and attribute_id = 'mongoose-id') as 'MongooseId',
                            (select value from stream_value_varchar where entity_id = se.entity_id and attribute_id = 'mongoose-name') as 'MongooseName',
                            (select value from stream_value_varchar where entity_id = se.entity_id and attribute_id = 'pack-dst-id') as 'PackDestinationId',
                            (select value from stream_value_varchar where entity_id = se.entity_id and attribute_id = 'pack-dst-name') as 'PackDestintionName',
                            (select value from stream_value_varchar where entity_id = se.entity_id and attribute_id = 'pack-src-id') as 'PackSourceId',
                            (select value from stream_value_varchar where entity_id = se.entity_id and attribute_id = 'pack-src-name') as 'PackSourceName',
                            (select value from stream_value_varchar where entity_id = se.entity_id and attribute_id = 'time') as 'Time',
                            (select value from stream_value_varchar where entity_id = se.entity_id and attribute_id = 'user') as 'User',
                            (select value from stream_value_real where entity_id = se.entity_id and attribute_id = 'lat') as 'Latitude',
                            (select value from stream_value_real where entity_id = se.entity_id and attribute_id = 'lon') as 'Longitude'
                            from stream_entity se
                            where se.entity_type = 'movepack-event' and se.sent = 0";

            var lifeEvents = RunSql<PackMove>(stringSql).ToList();
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