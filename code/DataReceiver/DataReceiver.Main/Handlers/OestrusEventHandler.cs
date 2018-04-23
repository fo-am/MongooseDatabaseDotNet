using DataReceiver.Main.Data;
using DataReceiver.Main.Interfaces;
using DataReceiver.Main.Model.Oestrus;

namespace DataReceiver.Main.Handlers
{
    public class OestrusEventHandler : IHandle<OestrusEvent> {
        private readonly IPgRepository data;

        public OestrusEventHandler(IPgRepository data)
        {
            this.data = data;
       
        }

        public void HandleMessage(OestrusEvent message)
        {
            data.InsertOestrusEvent(message);
        }
    }
}