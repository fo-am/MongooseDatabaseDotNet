using DataReceiver.Main.Data;
using DataReceiver.Main.Interfaces;
using DataReceiver.Main.Model.LifeHistory;

namespace DataReceiver.Main.Handlers
{
    internal class IndividualImmigrateEventHandler : IHandle<IndividualImmigrateEvent>
    {
        private IPgRepository data;

        public IndividualImmigrateEventHandler(IPgRepository data)
        {
            this.data = data;
        }

        public void HandleMessage(IndividualImmigrateEvent message)
        {
            
            data.NewIndividualEvent(message);
        }
    }
}