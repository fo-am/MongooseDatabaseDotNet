using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using Dapper;
using NLog;
using Npgsql;
using psDataImporter.Contracts.Access;
using psDataImporter.Contracts.dtos;
using psDataImporter.Contracts.Postgres;

namespace psDataImporter.Data
{
    public class PostgresRepository
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public List<Pack> GetAllPacks()
        {
            var pgPacks = new List<Pack>();
            using (IDbConnection conn = new NpgsqlConnection(ConfigurationManager
                .ConnectionStrings["postgresConnectionString"]
                .ConnectionString))
            {
                pgPacks = conn
                    .Query<Pack>("Select pack_id as PackId, name, pack_created_date as CreatedDate from mongoose.pack")
                    .ToList();
            }
            return pgPacks;
        }

        public List<Individual> GetAllIndividuals()
        {
            var pgIndividuals = new List<Individual>();
            using (IDbConnection conn = new NpgsqlConnection(ConfigurationManager
                .ConnectionStrings["postgresConnectionString"]
                .ConnectionString))
            {
                pgIndividuals = conn
                    .Query<Individual>(
                        "Select individual_id as IndividualId, litter_id as LitterId, name, sex from mongoose.individual")
                    .ToList();
            }
            return pgIndividuals;
        }

        public void AddWeights(IEnumerable<Weights> weights, List<Individual> pgIndividuals)
        {
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
                        cmd.Parameters.AddWithValue("CollarWeight", (object) weight.Collar ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("Comment", (object) weight.Comment ?? DBNull.Value);

                        cmd.ExecuteNonQuery();
                    }
                    Logger.Info($"Insert weight for: {weight.Indiv} weight {weight.Weight}");
                }
            }
        }

        public void AddPackHistory(IEnumerable<PackHistoryDto> packHistorys, IEnumerable<Pack> pgPacks,
            IEnumerable<Individual> pgIndividuals)
        {
            using (IDbConnection conn = new NpgsqlConnection(ConfigurationManager
                .ConnectionStrings["postgresConnectionString"]
                .ConnectionString))

            {
                //select and see if we have an entry

                foreach (var membership in packHistorys.OrderByDescending(ph => ph.DateJoined))
                {
                    var packId = pgPacks.Single(p => p.Name == membership.PackName).PackId;
                    var individualId = pgIndividuals.Single(i => i.Name == membership.IndividualName).IndividualId;

                    var packHistory = conn.Query<PackHistory>(
                        "SELECT pack_history_id as PackHistoryId, pack_id as PackId, individual_id as IndividualId, date_joined as DateJoined from mongoose.pack_history where pack_id = @pack and individual_id = @individual",
                        new {pack = packId, individual = individualId}).FirstOrDefault();


                    if (packHistory != null)
                    {
                        if (packHistory.DateJoined < membership.DateJoined)
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
                            new {PackId = packId, IndividualId = individualId, membership.DateJoined});

                        Logger.Info($"Insert pack history: {membership.DateJoined}");
                    }
                }
            }
        }

        public void AddIndividuals(IEnumerable<Individual> individuals)
        {
            foreach (var indiv in individuals)
            {
                using (IDbConnection conn = new NpgsqlConnection(ConfigurationManager
                    .ConnectionStrings["postgresConnectionString"]
                    .ConnectionString))
                {
                    conn.ExecuteScalar<int>(
                        "Insert into mongoose.individual (name, sex) values (@name, @sex) ON CONFLICT DO NOTHING",
                        new {indiv.Name, indiv.Sex});
                }
                Logger.Info($"created indiv: {indiv.Name}");
            }
        }

        public void AddGroups(IEnumerable<string> packNames)
        {
            foreach (var packName in packNames)
            {
                using (IDbConnection conn = new NpgsqlConnection(ConfigurationManager
                    .ConnectionStrings["postgresConnectionString"]
                    .ConnectionString))
                {
                    conn.ExecuteScalar<int>("Insert into mongoose.pack (name) values (@name) ON CONFLICT DO NOTHING ",
                        new {name = packName});
                }
                Logger.Info($"created pack: {packName}");
            }
        }
    }
}