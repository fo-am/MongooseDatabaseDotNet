using DataReciever.Main.Data;
using DataReciever.Main.Interfaces;
using DataReciever.Main.Model;

namespace DataReciever.Main.Handlers
{
    internal class LitterCreatedEventHandler : IHandle<LitterCreated>
    {
        public void HandleMessage(LitterCreated message)
        {
            {
                var data = new PgRepository();
                data.InsertNewLitter(message);
            }
        }
    }
}