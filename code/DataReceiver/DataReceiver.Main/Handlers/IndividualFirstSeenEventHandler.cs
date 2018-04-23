using DataReceiver.Main.Data;
using DataReceiver.Main.Interfaces;
using DataReceiver.Main.Model.LifeHistory;

namespace DataReceiver.Main.Handlers
{
    internal class IndividualFirstSeenEventHandler : IHandle<IndividualFirstSeenEvent>
    {
        private IPgRepository data;

        public IndividualFirstSeenEventHandler(IPgRepository data)
        {
            this.data = data;
        }

        public void HandleMessage(IndividualFirstSeenEvent message)
        {
            
            data.NewIndividualEvent(message);
        }
    }
}