using System;
using System.Linq;

using SQLite;

namespace DataInput.Core
{
    public class GetSqliteData
    {
        public static string DbFile => GetAppSettings.Get().SqliteLocation;

        public static SQLiteConnection SimpleDbConnection()
        {
            return new SQLiteConnection(DbFile);
        }

        public sync_entity GetSomeData()
        {
            var db = SimpleDbConnection();

            return db.Query<sync_entity>("select  * from sync_entity limit 1").First();
        }
    }
}
