using DataReceiver.Main.Data;
using DataReceiver.Main.Interfaces;
using DataReceiver.Main.Model.LifeHistory;

namespace DataReceiver.Main.Handlers
{
    internal class IndividualLastSeenEventHandler : IHandle<IndividualLastSeenEvent>
    {
        private IPgRepository data;

        public IndividualLastSeenEventHandler(IPgRepository data)
        {
            this.data = data;
        }

        public void HandleMessage(IndividualLastSeenEvent message)
        {
            
            data.NewIndividualEvent(message);
        }
    }
}