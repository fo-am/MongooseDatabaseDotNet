using DataReceiver.Main.Data;
using DataReceiver.Main.Interfaces;
using DataReceiver.Main.Model.LifeHistory;

namespace DataReceiver.Main.Handlers
{
    internal class IndividualAbortEventHandler : IHandle<IndividualAbortEvent>
    {
        private IPgRepository data;

        public IndividualAbortEventHandler(IPgRepository data)
        {
            this.data = data;
        }

        public void HandleMessage(IndividualAbortEvent message)
        {
            
            data.NewIndividualEvent(message);
        }
    }
}