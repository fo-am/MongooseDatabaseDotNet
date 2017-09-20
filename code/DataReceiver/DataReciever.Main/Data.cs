using System;
using System.Configuration;
using SQLite;

namespace DataReciever.Main
{
    internal class Data
    {
        public static string DbFile => GetAppSettings.Get().SqliteLocation;

        public static SQLiteConnection SimpleDbConnection()
        {
            return new SQLiteConnection(DbFile);
        }

        public static void SetupDatabase()
        {
            var db = new SQLiteConnection(DbFile);

            db.CreateTable<stream_attribute>();
            db.CreateTable<stream_entity>();
            db.CreateTable<stream_value_file>();
            db.CreateTable<stream_value_int>();
            db.CreateTable<stream_value_real>();
            db.CreateTable<stream_value_varchar>();
            db.CreateTable<sync_attribute>();
            db.CreateTable<sync_entity>();
            db.CreateTable<sync_value_file>();
            db.CreateTable<sync_value_int>();
            db.CreateTable<sync_value_real>();
            db.CreateTable<sync_value_varchar>();
        }

        internal static void StoreEntity<T>(T output)
        {
            using (var conn = SimpleDbConnection())
            {
                conn.Insert(output);
            }
        }
    }
}