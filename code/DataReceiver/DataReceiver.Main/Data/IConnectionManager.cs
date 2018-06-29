using System.Data;

namespace DataReceiver.Main.Data
{
    public interface IConnectionManager
    {
        IDbConnection GetConn();
    }
}