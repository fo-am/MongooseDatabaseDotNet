using NLog;
using System;
using System.Data.OleDb;
using Dapper;
using System.Collections.Generic;
using System.Data;
using Npgsql;

namespace psDataImporter.Console
{
    class Program
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            IEnumerable<NewLifeHistory> data = new List<NewLifeHistory>();
            // attach to access
            try
            {
                 data = GetLifeHistoryDataFromAccess();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Access error");
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
            using (IDbConnection conn = new NpgsqlConnection(@"Server=localhost;Port=5432;Database=new_database;User Id=aidan; Password=user;"))
            {
                conn.Open();
                foreach (var item in data)
                {
                    try
                    {
                        conn.Execute("insert into mongoose.pack (name) values (@PACK)", new { item.PACK });
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, "insert error " + ex.Message);
                    }
                   
                    
                }

                conn.Close();              

            }
        }

        public static IEnumerable<NewLifeHistory> GetLifeHistoryDataFromAccess()
        {
            using (OleDbConnection conn = new OleDbConnection(@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=D:\DropBox\Mongoose\NON_replicated Jan 2017.accdb;Persist Security Info=True"))
            {
                conn.Open();
                var results = conn.Query<NewLifeHistory>(@"SELECT [PACK], [DATE], [INDIV], [CODE], [LITTER]  FROM [NEW LIFE HISTORY] WHERE [CODE] = ""FPREG"" ");
                conn.Close();

                return results;

            }
        }
    }

 class NewLifeHistory
    {
        public string PACK;
        public string INDIV;
        public DateTime DATE;
        public string CODE;
        public string LITTER;


    }
}


// use raw sql import
// log errors not success
// create lib of useful db calls.