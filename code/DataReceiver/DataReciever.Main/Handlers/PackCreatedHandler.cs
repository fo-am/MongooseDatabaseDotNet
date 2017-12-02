using DataReciever.Main.Data;
using DataReciever.Main.Interfaces;
using DataReciever.Main.Model;

namespace DataReciever.Main.Handlers
{
    public class PackCreatedHandler : IHandle<PackCreated>
    {
        public void HandleMessage(PackCreated message)
        {
            var data = new PgRepository();
            data.InsertNewPack(message);
        }
    }
}