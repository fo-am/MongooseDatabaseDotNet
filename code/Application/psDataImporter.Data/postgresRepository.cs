using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using Dapper;
using NLog;
using Npgsql;
using psDataImporter.Contracts.Access;

namespace psDataImporter.Data
{
    public class PostgresRepository
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public void PushLifeHistorysToPostgres(IEnumerable<NewLifeHistory> data)
        {
            try
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
                            Logger.Error(ex, "insert error " + ex.Message);
                        }

                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "postgres error" + ex.Message);
            }
        }
    }
}
