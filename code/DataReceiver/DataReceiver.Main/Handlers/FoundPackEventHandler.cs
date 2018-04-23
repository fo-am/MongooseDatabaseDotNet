using DataReceiver.Main.Data;
using DataReceiver.Main.Interfaces;
using DataReceiver.Main.Model.LifeHistory;

namespace DataReceiver.Main.Handlers
{
    internal class FoundPackEventHandler : IHandle<FoundPackEvent>
    {
        private IPgRepository data;

        public FoundPackEventHandler(IPgRepository data)
        {
            this.data = data;
        }

        public void HandleMessage(FoundPackEvent message)
        {
            
            data.PackEvent(message);
           

        
        }
    }
}