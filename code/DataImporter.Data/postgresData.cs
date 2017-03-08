using System;
using System.Collections.Generic;
using System.Linq;
using Npgsql;

namespace DataImporter.Data
{
    public class PostgresData
    {
        public void InsertData(HashSet<string> data)
        {
            using (
                var npgsqlConnection =
                    new NpgsqlConnection("Host=localhost;Username=postgres;Password=Dctagl04;Database=FoAM"))
            {
                npgsqlConnection.Open();
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = npgsqlConnection;

                    var query = @"INSERT INTO mongoosedb.""Group""(""Group"") VALUES ";

                    query += string.Join(",", data.Select(id => string.Format("('" + id + "')")));

                    Console.WriteLine(query);

                    cmd.CommandText = query;
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}