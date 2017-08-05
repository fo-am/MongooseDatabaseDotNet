using Dapper;
using DapperExtensions;
using Mono.Data.Sqlite;
using System.Collections.Generic;
using System.Configuration;

using System.IO;

namespace DataPipe.Main
{
    class Data
    { 
        public Data()
        {
        }

        public static string DbFile
        {
            get { return ConfigurationManager.AppSettings["SqliteLocation"]; }
        }

        public static SqliteConnection SimpleDbConnection()
        {
            DapperExtensions.DapperExtensions.SqlDialect = new DapperExtensions.Sql.SqliteDialect();

            return new SqliteConnection("Data Source=" + DbFile);

        }

        internal static void MarkVarcharAsSent(sync_value_varchar message)
        {
            message.version = 99;
            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                cnn.Update<sync_value_varchar>(message);
            }
        }

        public static IEnumerable<sync_value_varchar> GetUnsyncedEntityValueVarchars()
        {
            if (!File.Exists(Path.GetFullPath(Data.DbFile))) return null;

            using (var cnn = Data.SimpleDbConnection())
            {
                cnn.Open();
                var result = cnn.Query<sync_value_varchar>(
                    @"SELECT id, entity_id, attribute_id, value, dirty, version FROM sync_value_varchar where version != 99");
                return result;
            }
        }

        internal static void MarkEntityAsSent(sync_entity message)
        {
            message.version = 99;
            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                cnn.Update<sync_entity>(message);
            }
        }

        public static IEnumerable<sync_entity> GetUnsyncedEntitys()
        {

            if (!File.Exists(Path.GetFullPath(Data.DbFile))) return null;

            using (var cnn = Data.SimpleDbConnection())
            {
                cnn.Open();
                var result = cnn.Query<sync_entity>(
             @"SELECT entity_id, entity_type, unique_id, dirty, version FROM sync_entity where version != 99");
                return result;
            }
        }
    }
}