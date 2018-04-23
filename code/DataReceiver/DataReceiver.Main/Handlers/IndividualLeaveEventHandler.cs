using DataReceiver.Main.Data;
using DataReceiver.Main.Interfaces;
using DataReceiver.Main.Model.LifeHistory;

namespace DataReceiver.Main.Handlers
{
    internal class IndividualLeaveEventHandler : IHandle<IndividualLeaveEvent>
    {
        private IPgRepository data;

        public IndividualLeaveEventHandler(IPgRepository data)
        {
            this.data = data;
        }

        public void HandleMessage(IndividualLeaveEvent message)
        {
            
            data.NewIndividualEvent(message);
        }
    }
}