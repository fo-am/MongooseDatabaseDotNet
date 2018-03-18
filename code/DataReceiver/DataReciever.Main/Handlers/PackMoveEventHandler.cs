using DataReciever.Main.Data;
using DataReciever.Main.Interfaces;
using DataReciever.Main.Model;

namespace DataReciever.Main.Handlers
{
    internal class PackMoveEventHandler : IHandle<PackMove>
    {
        public void HandleMessage(PackMove message)
        {
            var data = new PgRepository();
            data.PackMove(message);
        }
    }
}