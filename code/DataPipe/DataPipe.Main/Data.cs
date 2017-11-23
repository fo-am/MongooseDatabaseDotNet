using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dapper;
using NLog;
using NLog.Config;
using SQLite;

namespace DataPipe.Main
{
    public class Data
    {
        public static SQLiteConnection SimpleDbConnection()
        {
            var dbFile = GetAppSettings.Get().SqliteLocation;
            return new SQLiteConnection(dbFile);
        }

        public static void MarkAsSent<T>(T message) where T : class, ISendable
        {
            LogManager.Configuration = new XmlLoggingConfiguration("nlog.config");
            var logger = LogManager.GetLogger("Data");

            if (message.entity_type == "mongoose" || message.entity_type == "pack")
            {
                using (var cnn = SimpleDbConnection())
                {
                    logger.Info(
                        $"Marking {message} as sent. Type = '{message.entity_type}' entity_id = '{message.entity_id}'");

                    cnn.BeginTransaction();

                    cnn.Execute("update sync_entity set sent = 1 where entity_id = @entity_id", message.entity_id);
                    cnn.Execute("update sync_value_int set sent = 1 where entity_id = @entity_id", message.entity_id);
                    cnn.Execute("update sync_value_real set sent = 1 where entity_id = @entity_id", message.entity_id);
                    cnn.Execute("update sync_value_varchar set sent = 1 where entity_id = @entity_id",
                        message.entity_id);

                    cnn.Commit();
                }
            }
        }

        private static IEnumerable<T> RunSql<T>(string sql) where T : new()
        {
            LogManager.Configuration = new XmlLoggingConfiguration("nlog.config");
            var logger = LogManager.GetLogger("Data");

            var dbFile = GetAppSettings.Get().SqliteLocation;
            var fullPath = Path.GetFullPath(dbFile);
            if (!File.Exists(fullPath))
            {
                logger.Error($"Database not found at '{fullPath}'");

                throw new FileNotFoundException($"database not located at '{fullPath}'");
            }
            using (var cnn = SimpleDbConnection())
            {
                List<T> result;
                try
                {
                    result = cnn.Query<T>(sql);
                }
                catch (Exception exception)
                {
                    logger.Error(exception, $"Could not get data from the database '{fullPath}' sql='{sql}'");
                    result = new List<T>();
                }

                return result;
            }
        }

        public static IEnumerable<IndividualCreated> GetUnsyncedIndividuals()
        {
            var newIndividuals = new List<IndividualCreated>();
            var stringsSql = @"
                        select se.entity_id, se.unique_id as uniqueId,se.entity_type, svv.attribute_id as attribute_id ,svv.value
                        from sync_entity se
                        join sync_value_varchar svv on se.entity_id = svv.entity_id
                        where se.entity_type = ""mongoose""
                        group by    se.unique_id , svv.attribute_id ,svv.value;";
            var strings = RunSql<DatabaseRow<string>>(stringsSql).ToList();

            var longSql = @"
                        select se.entity_id, se.unique_id as uniqueId, se.entity_type, svr.attribute_id ,svr.value 
                        from sync_entity se
                        join sync_value_real svr on se.entity_id = svr.entity_id
                        where se.entity_type = ""mongoose""  
                        group by    se.unique_id , svr.attribute_id ,svr.value;";
            var longs = RunSql<DatabaseRow<double>>(longSql).ToList();

            foreach (var uniqueCode in longs.Select(l => l.uniqueId).Distinct())
            {
                var newIndividual = new IndividualCreated
                {
                    entity_id = strings.SingleOrDefault(s => s.attribute_id == "name" && s.uniqueId == uniqueCode)
                        .entity_id,
                    UniqueId = uniqueCode,
                    entity_type = strings.SingleOrDefault(s => s.attribute_id == "name" && s.uniqueId == uniqueCode)
                        ?.entity_type,
                    ChipCode = strings.SingleOrDefault(s => s.attribute_id == "chip-code" && s.uniqueId == uniqueCode)
                        ?.value,
                    CollerWeight =
                        longs.SingleOrDefault(s => s.attribute_id == "collar-weight" && s.uniqueId == uniqueCode)
                            ?.value,
                    DateOfBirth =
                        GetDate(
                            strings.SingleOrDefault(s => s.attribute_id == "dob" && s.uniqueId == uniqueCode)?.value),
                    Gender =
                        strings.SingleOrDefault(s => s.attribute_id == "gender" && s.uniqueId == uniqueCode)?.value,
                    Name = strings.SingleOrDefault(s => s.attribute_id == "name" && s.uniqueId == uniqueCode)?.value,
                    LitterCode =
                        strings.SingleOrDefault(s => s.attribute_id == "litter-code" && s.uniqueId == uniqueCode)
                            ?.value,
                    PackCode = GetPackId(strings
                        .SingleOrDefault(s => s.attribute_id == "pack-id" && s.uniqueId == uniqueCode)?.value),
                    PackUniqueId = strings.SingleOrDefault(s => s.attribute_id == "pack-id" && s.uniqueId == uniqueCode)
                        ?.value,
                };
                if (!string.IsNullOrEmpty(newIndividual.Name))
                {
                    newIndividuals.Add(newIndividual);
                }
                else
                {
                    // new individual has no name. just dump it for now.
                }
            }
            return newIndividuals;
        }

        private static string GetPackId(string packUniqueId)
        {
            var sql = $@"
                        select svv.value from sync_entity se 
                        join sync_value_varchar svv on se.entity_id = svv.entity_id
                        where se.unique_id = ""{packUniqueId}""";

            return RunSql<DatabaseRow<string>>(sql).FirstOrDefault()?.value;
        }

        private static DateTime? GetDate(string dateString)
        {
            if (DateTime.TryParse(dateString, out var dateTime))
            {
                return dateTime;
            }
            return null;
        }
    }

    public class DatabaseRow<T>
    {
        public string entity_type { get; set; }
        public int entity_id { get; set; }
        public string uniqueId { get; set; }
        public string attribute_id { get; set; }
        public T value { get; set; }
    }
}