using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.OleDb;
using Dapper;
using NLog;
using psDataImporter.Contracts.Access;

namespace psDataImporter.Data
{
    public class AccessRepository
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public IEnumerable<Weights> GetWeights()
        {
            IEnumerable<Weights> weights = new List<Weights>();
            try
            {
                using (var conn = new OleDbConnection(ConfigurationManager
                    .ConnectionStrings["accessConnectionString"]
                    .ConnectionString))
                {
                    conn.Open();
                    weights = conn.Query<Weights>(
                        @"SELECT *, [DATE] + IIF(ISNULL([TIME]),0,[TIME]) as [TimeMeasured] FROM WEIGHTS"); // null times are turned to a value.
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Access error" + ex.Message);
            }
            return weights;
        }
    }
}