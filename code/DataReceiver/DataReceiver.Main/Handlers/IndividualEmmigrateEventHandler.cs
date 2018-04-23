using DataReceiver.Main.Data;
using DataReceiver.Main.Interfaces;
using DataReceiver.Main.Model.LifeHistory;

namespace DataReceiver.Main.Handlers
{
    internal class IndividualEmmigrateEventHandler : IHandle<IndividualEmmigrateEvent>
    {
        private IPgRepository data;

        public IndividualEmmigrateEventHandler(IPgRepository data)
        {
            this.data = data;
        }

        public void HandleMessage(IndividualEmmigrateEvent message)
        {
            
            data.NewIndividualEvent(message);
        }
    }
}