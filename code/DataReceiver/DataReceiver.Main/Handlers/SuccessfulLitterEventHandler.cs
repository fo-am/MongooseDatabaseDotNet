using DataReceiver.Main.Data;
using DataReceiver.Main.Interfaces;
using DataReceiver.Main.Model.LifeHistory;

namespace DataReceiver.Main.Handlers
{
    internal class SuccessfulLitterEventHandler : IHandle<SuccessfulLitterEvent>
    {
        private IPgRepository data;

        public SuccessfulLitterEventHandler(IPgRepository data)
        {
            this.data = data;
        }

        public void HandleMessage(SuccessfulLitterEvent message)
        {
            
            data.InsertNewLitterEvent(message);
        }
    }
}