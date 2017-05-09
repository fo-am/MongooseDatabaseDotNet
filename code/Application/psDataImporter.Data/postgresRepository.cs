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
                    {
                        try
                        {
                            conn.Execute("insert into mongoose.pack (name) values (@PACK)", new {PACK = item.Pack});
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex, "insert error " + ex.Message);
                        }
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
            weights = weights as IList<Weights> ?? weights.ToList();

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
            using (IDbConnection conn = new NpgsqlConnection(ConfigurationManager
                .ConnectionStrings["postgresConnectionString"]
                .ConnectionString))

            {
                //select and see if we have an entry

                foreach (var membership in weights
                    .Select(grp => new {group = grp.Group, indiv = grp.Indiv, date = grp.TimeMeasured})
                    .OrderByDescending(g => g.date))
                {
                    var packId = pgPacks.Single(p => p.Name == membership.group).PackId;
                    var individualId = pgIndividuals.Single(i => i.Name == membership.indiv).IndividualId;
                    var packHistory = conn.Query<PackHistory>(
                        "SELECT pack_history_id as PackHistoryId, pack_id as PackId, individual_id as IndividualId, date_joined as DateJoined from mongoose.pack_history where pack_id = @pack and individual_id = @individual",
                        new {pack = packId, individual = individualId}).FirstOrDefault();


                    if (packHistory != null)
                    {
                        if (packHistory.DateJoined < membership.date)
                        {
                            //if we do then check the date, if our date is older then update with the new older date
                            conn.Execute(
                                "update mongoose.pack_history set date_joined = @date where pack_history_id = @packHistoryId",
                                new {date = packHistory.DateJoined, packHistoryId = packHistory.PackHistoryId});

                            Logger.Info($"update pack history: {packHistory.PackHistoryId}");
                        }
                    }
                    else
                    {
                        // if not insert new info

                        conn.Execute(
                            "Insert into mongoose.pack_history (pack_id, individual_id, date_joined) values (@PackId, @IndividualId, @DateJoined)",
                            new {PackId = packId, IndividualId = individualId, DateJoined = membership.date});

                        Logger.Info($"Insert pack history: {membership.date}");
                    }
                }
            }
            // add wieights
            foreach (var weight in weights)
            {
                // create geography if lat and long are present.
                var locationString = "NULL";
                if (!string.IsNullOrEmpty(weight.Latitude) && !string.IsNullOrEmpty(weight.Longitude))
                {
                    locationString = $"ST_GeographyFromText('SRID=4326;POINT({weight.Latitude} {weight.Longitude})')";
                }

                var sql =
                    "Insert into mongoose.weight (individual_id, weight, time, accuracy, session, collar_weight, comment, location)" +
                    $" values (@IndividualId, @Weight, @Time, @Accuracy, @Session, @CollarWeight, @Comment, {locationString})";

                using (var conn = new NpgsqlConnection(ConfigurationManager
                    .ConnectionStrings["postgresConnectionString"]
                    .ConnectionString))

                {
                    using (var cmd = new NpgsqlCommand())
                    {
                        cmd.Connection = conn;
                        conn.Open();
                        cmd.AllResultTypesAreUnknown = true;
                        var individualId = pgIndividuals.Single(i => i.Name == weight.Indiv).IndividualId;

                        cmd.CommandText = sql;

                        cmd.Parameters.AddWithValue("IndividualId", individualId);
                        cmd.Parameters.AddWithValue("Weight", weight.Weight);
                        cmd.Parameters.AddWithValue("Time", weight.TimeMeasured);
                        cmd.Parameters.AddWithValue("Accuracy", (object) weight.Accuracy ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("Session", (object) weight.Session ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("CollarWeight", (object) weight.Collar??DBNull.Value);
                        cmd.Parameters.AddWithValue("Comment", (object) weight.Comment ?? DBNull.Value);

                        cmd.ExecuteNonQuery();
                    }
                    Logger.Info($"Insert weight for: {weight.Indiv} weight {weight.Weight}");
                }
            }
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