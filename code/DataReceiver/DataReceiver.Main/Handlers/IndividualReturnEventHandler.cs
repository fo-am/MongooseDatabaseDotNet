using DataReceiver.Main.Data;
using DataReceiver.Main.Interfaces;
using DataReceiver.Main.Model.LifeHistory;

namespace DataReceiver.Main.Handlers
{
    internal class IndividualReturnEventHandler : IHandle<IndividualReturnEvent>
    {
        private IPgRepository data;

        public IndividualReturnEventHandler(IPgRepository data)
        {
            this.data = data;
        }

        public void HandleMessage(IndividualReturnEvent message)
        {
            
            data.NewIndividualEvent(message);
        }
    }
}