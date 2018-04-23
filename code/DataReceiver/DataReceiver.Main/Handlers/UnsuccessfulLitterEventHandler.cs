using DataReceiver.Main.Data;
using DataReceiver.Main.Interfaces;
using DataReceiver.Main.Model.LifeHistory;

namespace DataReceiver.Main.Handlers
{
    internal class UnsuccessfulLitterEventHandler : IHandle<UnsuccessfulLitterEvent>
    {
        private IPgRepository data;

        public UnsuccessfulLitterEventHandler(IPgRepository data)
        {
            this.data = data;
        }

        public void HandleMessage(UnsuccessfulLitterEvent message)
        {
            
            data.InsertNewLitterEvent(message);
        }
    }
}