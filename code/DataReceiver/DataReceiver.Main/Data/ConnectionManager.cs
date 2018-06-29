using System.Data;
using Npgsql;

namespace DataReceiver.Main.Data
{
    public class ConnectionManager : IConnectionManager
    {
        public IDbConnection GetConn()
        {
            return new NpgsqlConnection(GetAppSettings.Get().PostgresConnection);
        }
    }
}