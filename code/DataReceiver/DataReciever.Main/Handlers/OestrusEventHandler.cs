using DataReciever.Main.Data;
using DataReciever.Main.Interfaces;
using DataReciever.Main.Model.Oestrus;

namespace DataReciever.Main.Handlers
{
    public class OestrusEventHandler : IHandle<OestrusEvent> {
        public void HandleMessage(OestrusEvent message)
        {
            var data = new PgRepository();
            data.InsertOestrusEvent(message);
        }
    }
}