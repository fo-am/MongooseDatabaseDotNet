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

        public IEnumerable<NewLifeHistory> GetNewLifeHistories()
        {
            IEnumerable<NewLifeHistory> lifeHistories = new List<NewLifeHistory>();
            // attach to access
            try
            {
                using (var conn = new OleDbConnection(ConfigurationManager
                    .ConnectionStrings["accessConnectionString"]
                    .ConnectionString))
                {
                    conn.Open();
                    lifeHistories = conn.Query<NewLifeHistory>(
                        @"SELECT [NEW LIFE HISTORY].PACK, [NEW LIFE HISTORY].INDIV, [NEW LIFE HISTORY].SEX, [NEW LIFE HISTORY].[AGE CAT] as AgeCat, [NEW LIFE HISTORY].STATUS, [NEW LIFE HISTORY].[START/END] as StartEnd, [NEW LIFE HISTORY].CODE, [NEW LIFE HISTORY].EXACT, [NEW LIFE HISTORY].LSEEN, [NEW LIFE HISTORY].CAUSE, [NEW LIFE HISTORY].LITTER, [NEW LIFE HISTORY].[PREV NAME] as PrevName, [NEW LIFE HISTORY].COMMENT, [NEW LIFE HISTORY].EDITED, [NEW LIFE HISTORY].Latitude, [NEW LIFE HISTORY].Longitude FROM [NEW LIFE HISTORY] WHERE [START/END] is not null"); // [NEW LIFE HISTORY].CODE = ""FPREG"" ");
                    conn.Close();


                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Access error" + ex.Message);
            }
            return lifeHistories;
        }

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
                        @"SELECT *, [DATE]+[TIME] as [TimeMeasured] FROM WEIGHTS"); 
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