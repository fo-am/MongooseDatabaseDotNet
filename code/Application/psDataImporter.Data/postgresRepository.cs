using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using Dapper;
using NLog;
using Npgsql;
using psDataImporter.Contracts.Access;
using psDataImporter.Contracts.Postgres;

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
                    .ConnectionStrings["postgresConnectionString"]
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

        public void ProcessWeights(IEnumerable<Weights> weights)
        {
            // add packs

            var pgPacks = new List<Pack>();

            foreach (var group in weights.Select(s => s.Group).Distinct())
            {
                pgPacks.Add(NewPack(group));
                Logger.Info($"created pack: {group}");
            }


            // add individuals
            var pgIndividuals = new List<Individual>();
            foreach (var indiv in weights.Select(s => s.Indiv).Distinct())
            {
                pgIndividuals.Add(NewIndividual(indiv));
                Logger.Info($"created indiv: {indiv}");
            }

            // get litters for new indivs
            // get pack memebership
            foreach (var membership in weights.Select((grp) => new {group = grp.Group, indiv = grp.Indiv, date = grp.TimeMeasured  }))
            {
                using (IDbConnection conn = new NpgsqlConnection(ConfigurationManager
                    .ConnectionStrings["postgresConnectionString"]
                    .ConnectionString))
                {
                    var pack = conn.ExecuteScalar<PackHistory>(
                        "SELECT pack_id, individual_id, date_joined from mongoose.pack_history where pack_id = @pack and individual_id = @individual", new { pack = membership.group, individual = membership.indiv });
                    
             
                //select and see if we have an entry

               
                    // if not insert new info

                    //if we do then check the date, if our date is older then update with the new older date
                }
            }


            // add wieights
            // turn lat/long into geography
            // turn date and time into one datetime
        }

        private Individual NewIndividual(string indiv)
        {
            using (IDbConnection conn = new NpgsqlConnection(ConfigurationManager
                .ConnectionStrings["postgresConnectionString"]
                .ConnectionString))
            {
                var newid = conn.ExecuteScalar<int>(
                    "Insert into mongoose.individual (name) values (@val) RETURNING individual_id ", new {val = indiv});
                return new Individual(newid, indiv);
            }
        }

        private Pack NewPack(string group)
        {
            using (IDbConnection conn = new NpgsqlConnection(ConfigurationManager
                .ConnectionStrings["postgresConnectionString"]
                .ConnectionString))
            {
                var newid = conn.ExecuteScalar<int>("Insert into mongoose.pack (name) values (@val) RETURNING pack_id ",
                    new {val = group});
                return new Pack(newid, group);
            }
        }
    }
}