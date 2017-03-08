
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using Npgsql;

namespace DataImporter.Console
{
    //Get all groupIds from access database
    // put them in some sort of data strucutre.
    // maybe check they are okay? all two chars? or something
    // write them all to the postgres database.


    class Program
    {
        static void Main(string[] args)
        {
            var idsList = new HashSet<string>();

            using (var accessconn = new OleDbConnection())
            {
                accessconn.ConnectionString = @"Provider=Microsoft.Jet.OLEDB.4.0;" +
                                              @"Data source= C:\Users\aidan\Dropbox\foam\TheDatabase\" +
                                              @"Project replica August 2016.mdb";

              accessconn.Open();
                using (var cmd = new OleDbCommand())
                {
                    cmd.Connection = accessconn;
                    cmd.CommandText = "Select distinct group from weights";
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var item = reader.GetString(0);
                            System.Console.WriteLine(item + " Added: " + idsList.Add(item));

                        }
                    }
                }
            }
            System.Console.WriteLine("Total Groups: " + idsList.Count);

            using (var npgsqlConnection = new NpgsqlConnection("Host=localhost;Username=postgres;Password=Dctagl04;Database=FoAM"))
            {
                npgsqlConnection.Open();
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = npgsqlConnection;

                  

                    // Retrieve all rows
                var query =    @"INSERT INTO mongoosedb.""Group""(""Group"") VALUES ";


                    query += string.Join(",", idsList.Select(id => string.Format("('" + id + "')")));

                    System.Console.WriteLine(query);

                    cmd.CommandText = query;
                    cmd.ExecuteNonQuery();
                
                }
            }
            System.Console.ReadLine();
        }
    }
}
