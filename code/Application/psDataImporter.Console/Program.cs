using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using Dapper;
using NLog;
using Npgsql;

namespace psDataImporter.Console
{
    internal class Program
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private static void Main(string[] args)
        {
            IEnumerable<NewLifeHistory> data = new List<NewLifeHistory>();
            // attach to access
            try
            {
                data = GetLifeHistoryDataFromAccess();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Access error"+ ex.Message);
            }

            // attach to postgres
            try
            {
                PushDataToPostgres(data);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "postgres error" + ex.Message);
            }


            // import all packs
            // import all litters
            // import all individuals
            // import all pregnanceys

            // win!
            logger.Info("done");
            System.Console.ReadLine();
        }

        private static void PushDataToPostgres(IEnumerable<NewLifeHistory> data)
        {
            using (IDbConnection conn = new NpgsqlConnection(ConfigurationManager
                .ConnectionStrings["accessConnectionString"]
                .ConnectionString))
            {
                conn.Open();
                foreach (var item in data)
                    try
                    {
                        conn.Execute("insert into mongoose.pack (name) values (@PACK)", new {item.PACK});
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, "insert error " + ex.Message);
                    }

                conn.Close();
            }
        }

        public static IEnumerable<NewLifeHistory> GetLifeHistoryDataFromAccess()
        {
            using (var conn = new OleDbConnection(ConfigurationManager
                .ConnectionStrings["accessConnectionString"]
                .ConnectionString))
            {
                conn.Open();
                var results = conn.Query<NewLifeHistory>(
                    @"SELECT [PACK], [DATE], [INDIV], [CODE], [LITTER]  FROM [NEW LIFE HISTORY] WHERE [CODE] = ""FPREG"" ");
                conn.Close();

                return results;
            }
        }
    }

    internal class NewLifeHistory
    {
        public string CODE;
        public DateTime DATE;
        public string INDIV;
        public string LITTER;
        public string PACK;
    }
}


// use raw sql import
// log errors not success
// create lib of useful db calls.