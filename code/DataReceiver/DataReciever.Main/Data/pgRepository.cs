using System;
using System.Data;
using System.Linq;
using Dapper;
using DataReciever.Main.Model;
using NLog;
using NLog.Config;
using Npgsql;

namespace DataReciever.Main.Data
{
    public class PgRepository
    {
      //  private static Logger logger;
        private static readonly Logger logger = LogManager.GetLogger("PgRepository");
        public PgRepository()
        {
           
         //   logger = LogManager.GetLogger("PgRepository");
        }

        public static int StoreMessage(string fullName, string message)
        {
            logger.Info($"Stored message type '{fullName}'");
            using (IDbConnection conn = new NpgsqlConnection(GetAppSettings.Get().PostgresConnection))
            {
                return conn.ExecuteScalar<int>(
                    "Insert into mongoose.event_log (type, object) values (@type, @message::json) RETURNING event_log_id",
                    new {type = fullName, message});
            }
        }

        public static void MessageHandledOk(int entityId)
        {
            logger.Info($"Message handled ok.");
            using (IDbConnection conn = new NpgsqlConnection(GetAppSettings.Get().PostgresConnection))
            {
                conn.Execute("update mongoose.event_log set success = true where event_log_id = @entityId", entityId);
            }
        }

        public static void FailedToHandleMessage(int entityId, Exception ex)
        {
            logger.Error(ex, "Failed to handle message.");
            using (IDbConnection conn = new NpgsqlConnection(GetAppSettings.Get().PostgresConnection))
            {
                conn.Execute(
                    "update mongoose.event_log set success = false, error = @error where event_log_id = @entityId",
                    new {entityId, error = ex.ToString()});
            }
        }

        public void InsertNewPack(PackCreated message)
        {
            logger.Info($@"Add new pack: ""{message.Name}"".");
            using (IDbConnection conn = new NpgsqlConnection(GetAppSettings.Get().PostgresConnection))
            {
                conn.Open();
                using (var tr = conn.BeginTransaction())
                {
                    InsertPack(message.Name, message.UniqueId, conn, message.CreatedDate);

                    tr.Commit();
                }
            }
        }

        public void InsertNewWeight(WeightMeasure message)
        {
            logger.Info($"Add new weight for '{message.IndividualName}'. Weight = '{message.Weight}'");

            // get a packid
            // get a individualid
            // get a pack history id

            // add the weight info

            using (IDbConnection conn = new NpgsqlConnection(GetAppSettings.Get().PostgresConnection))
            {
                conn.Open();
                using (var tr = conn.BeginTransaction())
                {
                  var PackId =   InsertPack(message.PackId, message.UniqueId, conn);

                    tr.Commit();
                }
            }
        }

        public void InsertNewIndividual(IndividualCreated message)
        {
            logger.Info($"Add new individual '{message.Name}'.");

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

        private int InsertPack(string packName, string packUniqueId, IDbConnection conn,
            DateTime? packCreatedDate = null)
        {
            var packId = conn.ExecuteScalar<int?>("select pack_id from mongoose.pack where name = @name",
                new {name = packName});

            if (packCreatedDate.HasValue && packCreatedDate.Value == DateTime.MinValue)
            {
                packCreatedDate = null;
            }

            return packId ?? conn.ExecuteScalar<int>(
                       "Insert into mongoose.pack (name, unique_id, pack_created_date) values (@name, @unique_id, @pack_created_date) RETURNING pack_id",
                       new {name = packName, unique_id = packUniqueId, pack_created_date = packCreatedDate});
        }

        private int InsertIndividual(IndividualCreated message, int? litterId, IDbConnection conn)
        {
            var individualId = conn.ExecuteScalar<int?>(
                "select individual_id from mongoose.individual where name = @name",
                new {name = message.Name});
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
                new {packId, individualId}).FirstOrDefault();
            // if one exists
            if (packHistory?.pack_history_id != null)
            {
                packHistoryId = packHistory.pack_history_id;

                if (packHistory.date_joined == null || dateOfInteraction < packHistory.date_joined)
                {
                    conn.Execute("Update mongoose.pack_history set date_joined = @date_joined",
                        new {date_joined = dateOfInteraction});
                }
            }

            // if no pack history then add one.
            return packHistoryId ?? conn.ExecuteScalar<int>(
                       "Insert into mongoose.pack_history (pack_id, individual_id, date_joined) values (@pack_id, @individual_id, @date_joined) RETURNING pack_history_id",
                       new {pack_id = packId, individual_id = individualId, date_joined = dateOfInteraction});
        }

        private int? InsertLitter(string litterName, int packId, IDbConnection conn)
        {
            if (string.IsNullOrEmpty(litterName))
            {
                return null;
            }

            var litterId = conn.ExecuteScalar<int?>("select litter_id from mongoose.litter where name = @name",
                new {name = litterName});

            return litterId ?? conn.ExecuteScalar<int>(
                       "Insert into mongoose.litter (name, pack_id) values (@name, @pack_id) RETURNING litter_id",
                       new {name = litterName, pack_id = packId});
        }

        private void InsertIndividualBorn(int packHistoryId, IndividualCreated message, IDbConnection conn)
        {
            var eventIdBorn =
                conn.ExecuteScalar<int?>(
                    "Select individual_event_code_id from mongoose.individual_event_code where code = @code",
                    new {code = "BORN"});
            if (eventIdBorn == null)
            {
                eventIdBorn = conn.ExecuteScalar<int>(
                    "Insert into mongoose.individual_event_code (code) values (@born) RETURNING individual_event_code_id",
                    new {born = "BORN"});
            }

            var eventExists = conn.ExecuteScalar<int?>(
                @"select individual_event_id from mongoose.individual_event where individual_event_code_id = @individual_event_code_id and                                      
                  pack_history_id = @pack_history_id and date = @date",
                new
                {
                    individual_event_code_id = eventIdBorn,
                    pack_history_id = packHistoryId,
                    date = message.DateOfBirth
                });
            if (!eventExists.HasValue)
            {
                conn.Execute(@"Insert into mongoose.individual_event (individual_event_code_id, pack_history_id, date)
                                                          values (@individual_event_code_id, @pack_history_id, @date)",
                    new
                    {
                        individual_event_code_id = eventIdBorn,
                        pack_history_id = packHistoryId,
                        date = message.DateOfBirth
                    });
            }
        }
    }
}