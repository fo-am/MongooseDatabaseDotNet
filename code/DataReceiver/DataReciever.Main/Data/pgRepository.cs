using System;
using System.Data;
using System.Linq;

using Dapper;
using Dapper.Contrib.Extensions;

using Npgsql;

namespace DataReciever.Main.Data
{
    public class PgRepository
    {
        public void InsertNewIndividual(IndividualCreated message)
        {
            using (IDbConnection conn = new NpgsqlConnection(GetAppSettings.Get().PostgresConnection))
            {
                conn.Open();
                using (var tr = conn.BeginTransaction())
                {
                    var packId = InsertPack(message.PackCode, message.PackUniqueId, conn);
                    var litterId = InsertLitter(message.LitterCode, packId, conn);
                    var individualId = InsertIndividual(message, litterId, conn);
                    var packHistoryId = InsertPackHistory(packId, individualId, message.DateOfBirth, conn);
                    InsertIndividualBorn(packHistoryId, message, conn);

                    tr.Commit();
                }
            }
        }

        private int InsertPack(string packName, string packUniqueId, IDbConnection conn)
        {
            var packId = conn.ExecuteScalar<int?>("select pack_id from mongoose.pack where name = @name",
                new { name = packName });

            return packId ?? conn.ExecuteScalar<int>(
                       "Insert into mongoose.pack (name, unique_id) values (@name, @unique_id) RETURNING pack_id",
                       new { name = packName, unique_id = packUniqueId });
        }

        private int InsertIndividual(IndividualCreated message, int? litterId, IDbConnection conn)
        {
            var individualId = conn.ExecuteScalar<int?>("select individual_id from mongoose.individual where name = @name",
                new { name = message.Name });
            return individualId ?? conn.ExecuteScalar<int>(
                       "Insert into mongoose.individual (name, sex, litter_id, transponder_id, unique_id) values (@name, @sex, @litter_id, @transponder_id, @unique_id) RETURNING individual_id",
                       new
                       {
                           name = message.Name,
                           sex = message.Gender,
                           litter_id = litterId,
                           transponder_id = message.ChipCode,
                           unique_id = message.UniqueId
                       });
        }

        private int InsertPackHistory(int packId, int individualId, DateTime? dateOfInteraction, IDbConnection conn)
        {
            // get a pack history by ids.
            int? packHistoryId = null;
            var packHistory = conn.Query(
                "select pack_history_id, date_joined from mongoose.pack_history where pack_id = @packId and individual_id = @individualId",
                new { packId = packId, individualId = individualId }).FirstOrDefault();
            // if one exists
            if (packHistory?.pack_history_id != null)
            {
                packHistoryId = packHistory.pack_history_id;
                //   if (DateTime.TryParse(packHistory.date_joined, out DateTime databaseDate))
                //    {
                //        // check if my date is older than that in the database. if so update the database
                if (packHistory.date_joined == null || dateOfInteraction < packHistory.date_joined)
                {
                    conn.Execute("Update mongoose.pack_history set date_joined = @date_joined",
                        new { date_joined = dateOfInteraction });
                }
                //    }
            }

            // if no pack history then add one.
            return packHistoryId ?? conn.ExecuteScalar<int>(
                       "Insert into mongoose.pack_history (pack_id, individual_id, date_joined) values (@pack_id, @individual_id, @date_joined) RETURNING pack_history_id",
                       new { pack_id = packId, individual_id = individualId, date_joined = dateOfInteraction });
        }

        private int? InsertLitter(string litterName, int packId, IDbConnection conn)
        {
            if (string.IsNullOrEmpty(litterName))
            {
                return null;
            }

            var litterId = conn.ExecuteScalar<int?>("select litter_id from mongoose.litter where name = @name",
                new { name = litterName });

            return litterId ?? conn.ExecuteScalar<int>(
                       "Insert into mongoose.litter (name, pack_id) values (@name, @pack_id) RETURNING litter_id",
                       new { name = litterName, pack_id = packId });
        }

        private void InsertIndividualBorn(int packHistoryId, IndividualCreated message, IDbConnection conn)
        {
            var eventIdBorn =
                conn.ExecuteScalar<int?>("Select individual_event_code_id from mongoose.individual_event_code where code = @code",
                    new { code = "BORN" });
            if (eventIdBorn == null)
            {
                eventIdBorn = conn.ExecuteScalar<int>(
                    "Insert into mongoose.individual_event_code (code) values (@born) RETURNING individual_event_code_id",
                    new { born = "BORN" });
            }

            var eventExists = conn.ExecuteScalar<int?>(
                @"select individual_event_id from mongoose.individual_event where individual_event_code_id = @individual_event_code_id and                                      
                  pack_history_id = @pack_history_id and date = @date",
                new { individual_event_code_id = eventIdBorn, pack_history_id = packHistoryId, date = message.DateOfBirth });
            if (!eventExists.HasValue)
            {
                conn.Execute(@"Insert into mongoose.individual_event (individual_event_code_id, pack_history_id, date)
                                                          values (@individual_event_code_id, @pack_history_id, @date)",
                    new { individual_event_code_id = eventIdBorn, pack_history_id = packHistoryId, date = message.DateOfBirth });
            }
        }

        public static int StoreEntity(string fullName, string message)
        {
            using (IDbConnection conn = new NpgsqlConnection(GetAppSettings.Get().PostgresConnection))
            {
                return conn.ExecuteScalar<int>("Insert into event_log (type, object) values (@type, @message)",
                    new { type = fullName, message = message });
            }
        }

        public static void EntityHandled(int entityId)
        {
            using (IDbConnection conn = new NpgsqlConnection(GetAppSettings.Get().PostgresConnection))
            {
                conn.Execute("update event_log set success = true where event_log_id = @entityId", entityId);
            }
        }

        public static void EntityException(int entityId, Exception ex)
        {
            using (IDbConnection conn = new NpgsqlConnection(GetAppSettings.Get().PostgresConnection))
            {
                conn.Execute("update event_log set success = false, error = @error where event_log_id = @entityId",
                    new { entityId = entityId, error = ex.ToString() });
            }
        }
    }
}