using DataReceiver.Main.Data;
using DataReceiver.Main.Interfaces;
using DataReceiver.Main.Model.LifeHistory;

namespace DataReceiver.Main.Handlers
{
    internal class IndividualAssumedDeadEventHandler : IHandle<IndividualAssumedDeadEvent>
    {
        private IPgRepository data;

        public IndividualAssumedDeadEventHandler(IPgRepository data)
        {
            this.data = data;
        }

        public void HandleMessage(IndividualAssumedDeadEvent message)
        {
            
            data.NewIndividualEvent(message);
        }
    }
}