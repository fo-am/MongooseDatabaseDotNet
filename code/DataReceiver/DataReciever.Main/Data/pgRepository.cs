using System;
using System.Data;
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
                using (conn.BeginTransaction())
                {
                    var packId = InsertPack(message, conn);
                    var individualId = InsertIndividual(message, conn);
                    InsertPackHistory(packId, individualId, message.DateOfBirth, conn);
                    InsertLitter(packId, individualId, message.LitterCode, conn);
                    InsertIndividualBorn(individualId, message, conn);
                }
            }
        }

        private int InsertPack(IndividualCreated message, IDbConnection conn)
        {
            var packId = conn.ExecuteScalar<int?>("select pack_id from mongoose.pack where name = @name",
                new {name = message.PackCode});

            return packId ?? conn.ExecuteScalar<int>("Insert into pack set pack_id = @name RETURNING pack_id");
        }

        private int InsertIndividual(IndividualCreated message, IDbConnection conn)
        {
            throw new System.NotImplementedException();
        }

        private void InsertPackHistory(int packId, int individualId, DateTime? messageDateOfBirth, IDbConnection conn)
        {
            throw new NotImplementedException();
        }

        private void InsertLitter(int packId, int individualId, string messageLitterCode, IDbConnection conn)
        {
            throw new NotImplementedException();
        }

        private void InsertIndividualBorn(int individualId, IndividualCreated message, IDbConnection conn)
        {
            throw new NotImplementedException();
        }
    }
}