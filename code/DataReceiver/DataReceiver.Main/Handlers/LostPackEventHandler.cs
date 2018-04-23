using DataReceiver.Main.Data;
using DataReceiver.Main.Interfaces;
using DataReceiver.Main.Model.LifeHistory;

namespace DataReceiver.Main.Handlers
{
    internal class LostPackEventHandler : IHandle<LostPackEvent>
    {
        private IPgRepository data;

        public LostPackEventHandler(IPgRepository data)
        {
            this.data = data;
        }

        public void HandleMessage(LostPackEvent message)
        {
            
            data.PackEvent(message);
        }
    }
}