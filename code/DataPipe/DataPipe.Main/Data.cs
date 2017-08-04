using Dapper;

using DapperExtensions;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataPipe.Main
{
    class Data
    { 
        public Data()
        {
        }

        public static string DbFile
        {
            get { return "~\\..\\..\\..\\..\\mongoose.db"; }
        }

        public static SQLiteConnection SimpleDbConnection()
        {
            DapperExtensions.DapperExtensions.SqlDialect = new DapperExtensions.Sql.SqliteDialect();

            return new SQLiteConnection("Data Source=" + DbFile);

        }

        internal static void MarkVarcharAsSent(sync_value_varchar message)
        {
            message.sync = 1;
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
                    @"SELECT id, entity_id, attribute_id, value, dirty, version, sync FROM sync_value_varchar where sync=0");
                return result;
            }
        }

        internal static void MarkEntityAsSent(sync_entity message)
        {
            message.sync = 1;
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
             @"SELECT entity_id, entity_type, unique_id, dirty, version, sync FROM sync_entity where sync=0");
                return result;
            }
        }
    }
}