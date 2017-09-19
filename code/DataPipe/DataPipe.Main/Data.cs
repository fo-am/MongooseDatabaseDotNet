using System;
using System.Collections.Generic;
using System.IO;

using NLog;

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
            message.sent = 1;
            using (var cnn = SimpleDbConnection())
            {
                Console.WriteLine(message);
                cnn.Update(message);
            }
        }

        public static IEnumerable<stream_attribute> GetUnsyncedStreamAttribute()
        {
            var sql = @"SELECT id, attribute_id, entity_type, attribute_type FROM stream_attribute where sent != 1";
            return RunSql<stream_attribute>(sql);
        }

        public static IEnumerable<stream_entity> GetUnsyncedStreamEntity()
        {
            var sql = @"SELECT entity_id, entity_type, unique_id, dirty, version FROM stream_entity where sent != 1";

            return RunSql<stream_entity>(sql);
        }

        public static IEnumerable<stream_value_file> GetUnsyncedStreamValueFile()
        {
            var sql = @"SELECT id, entity_id, attribute_id, value, dirty, version FROM stream_value_file where sent != 1";
            return RunSql<stream_value_file>(sql);
        }

        public static IEnumerable<stream_value_int> GetUnsyncedStreamValueInt()
        {
            var sql = @"SELECT id, entity_id, attribute_id, value, dirty, version FROM stream_value_int where sent != 1";
            return RunSql<stream_value_int>(sql);
        }

        public static IEnumerable<stream_value_real> GetUnsyncedStreamValueReal()
        {
            var sql = @"SELECT id, entity_id, attribute_id, value, dirty, version FROM stream_value_real where sent != 1";
            return RunSql<stream_value_real>(sql);
        }

        public static IEnumerable<stream_value_varchar> GetUnsyncedStreamValueVarchar()
        {
            var sql = @"SELECT id, entity_id, attribute_id, value, dirty, version FROM stream_value_varchar where sent != 1";
            return RunSql<stream_value_varchar>(sql);
        }

        public static IEnumerable<sync_attribute> GetUnsyncedSyncAttribute()
        {
            var sql = @"SELECT id, attribute_id, entity_type, attribute_type FROM sync_attribute where sent != 1";
            return RunSql<sync_attribute>(sql);
        }

        public static IEnumerable<sync_entity> GetUnsyncedSyncEntity()
        {
            var sql = @"SELECT entity_id, entity_type, unique_id, dirty, version FROM sync_entity where sent != 1";
            return RunSql<sync_entity>(sql);
        }

        public static IEnumerable<sync_value_file> GetUnsyncedSyncValueFile()
        {
            var sql = @"SELECT id, entity_id, attribute_id, value, dirty, version FROM sync_value_file where sent != 1";
            return RunSql<sync_value_file>(sql);
        }

        public static IEnumerable<sync_value_int> GetUnsyncedSyncValueInt()
        {
            var sql = @"SELECT id, entity_id, attribute_id, value, dirty, version FROM sync_value_int where sent != 1";
            return RunSql<sync_value_int>(sql);
        }

        public static IEnumerable<sync_value_real> GetUnsyncedSyncValueReal()
        {
            var sql = @"SELECT id, entity_id, attribute_id, value, dirty, version FROM sync_value_real where sent != 1";
            return RunSql<sync_value_real>(sql);
        }

        public static IEnumerable<sync_value_varchar> GetUnsyncedSyncValueVarchar()
        {
            var sql = @"SELECT id, entity_id, attribute_id, value, dirty, version FROM sync_value_varchar where sent != 1";
            return RunSql<sync_value_varchar>(sql);
        }

        private static IEnumerable<T> RunSql<T>(string sql) where T : new()
        {
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
                    throw;
                }

                return result;
            }
        }
    }
}