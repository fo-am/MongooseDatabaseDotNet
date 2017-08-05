using Dapper;
using DapperExtensions;

using System.Collections.Generic;
using System.Configuration;
using System.Data.SQLite;
using System.IO;
using System;

namespace DataReciever.Main
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

        public static SQLiteConnection SimpleDbConnection()
        {
            DapperExtensions.DapperExtensions.SqlDialect = new DapperExtensions.Sql.SqliteDialect();

            return new SQLiteConnection("Data Source=" + DbFile);

        }

        public static void StoreValue(sync_value_varchar output)
        {
            using (var conn = SimpleDbConnection())
            {
                conn.Insert<sync_value_varchar>(output);
            }
        }

        public static void StoreEntity(sync_entity output)
        {
            using (var conn = SimpleDbConnection())
            {
                conn.Insert<sync_entity>(output);
            }          
        }

        public static void SetupDatabase()
        {
            if (!File.Exists(Data.DbFile))
            {
                SQLiteConnection.CreateFile(Data.DbFile);

                using (var conn = Data.SimpleDbConnection())
                {
                    conn.Open();
                    string sql = "create table sync_value_varchar (id INTEGER, entity_id INTEGER, attribute_id TEXT, value TEXT, dirty INTEGER, version INTEGER)";
                    new SQLiteCommand(sql, conn).ExecuteNonQuery();
                }
                using (var conn = Data.SimpleDbConnection())
                {
                    conn.Open();
                    string sql = "create table sync_entity (entity_id INTEGER, entity_type TEXT, unique_id TEXT, dirty INTEGER, version INTEGER)";
                    new SQLiteCommand(sql, conn).ExecuteNonQuery();
                }
            }
        }
    }
}