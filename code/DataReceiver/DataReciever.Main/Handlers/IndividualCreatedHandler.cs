using DataReciever.Main.Data;
using DataReciever.Main.Interfaces;
using DataReciever.Main.Model;

namespace DataReciever.Main.Handlers
{
    internal class IndividualCreatedHandler : IHandle<IndividualCreated>
    {
        public static string DbFile => GetAppSettings.Get().PostgresConnection;

        public void HandleMessage(IndividualCreated message)
        {
            var data = new PgRepository();
            data.InsertNewIndividual(message);
        }
    }
}