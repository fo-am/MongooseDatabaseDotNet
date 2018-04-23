using DataReceiver.Main.Data;
using DataReceiver.Main.Interfaces;
using DataReceiver.Main.Model.LifeHistory;

namespace DataReceiver.Main.Handlers
{
    internal class ShortLivedLitterEventHandler : IHandle<ShortLivedLitterEvent>
    {
        private IPgRepository data;

        public ShortLivedLitterEventHandler(IPgRepository data)
        {
            this.data = data;
        }

        public void HandleMessage(ShortLivedLitterEvent message)
        {
            
            data.InsertNewLitterEvent(message);
        }
    }
}