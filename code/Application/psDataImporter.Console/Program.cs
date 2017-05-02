using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using Dapper;
using NLog;
using Npgsql;
using psDataImporter.Contracts.Access;

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
                        conn.Execute("insert into mongoose.pack (name) values (@PACK)", new {PACK = item.Pack});
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
                    @"SELECT [NEW LIFE HISTORY].PACK, [NEW LIFE HISTORY].INDIV, [NEW LIFE HISTORY].SEX, [NEW LIFE HISTORY].[AGE CAT] as AgeCat, [NEW LIFE HISTORY].STATUS, [NEW LIFE HISTORY].[START/END] as StartEnd, [NEW LIFE HISTORY].CODE, [NEW LIFE HISTORY].EXACT, [NEW LIFE HISTORY].LSEEN, [NEW LIFE HISTORY].CAUSE, [NEW LIFE HISTORY].LITTER, [NEW LIFE HISTORY].[PREV NAME] as PrevName, [NEW LIFE HISTORY].COMMENT, [NEW LIFE HISTORY].EDITED, [NEW LIFE HISTORY].Latitude, [NEW LIFE HISTORY].Longitude FROM [NEW LIFE HISTORY] WHERE [START/END] is not null");// [NEW LIFE HISTORY].CODE = ""FPREG"" ");
                conn.Close();

                return results;
            }
        }
    }
}


// use raw sql import
// log errors not success
// create lib of useful db calls.