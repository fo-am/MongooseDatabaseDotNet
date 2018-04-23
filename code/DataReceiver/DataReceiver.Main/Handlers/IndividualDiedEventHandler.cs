using DataReceiver.Main.Data;
using DataReceiver.Main.Interfaces;
using DataReceiver.Main.Model.LifeHistory;

namespace DataReceiver.Main.Handlers
{
    internal class IndividualDiedEventHandler : IHandle<IndividualDiedEvent>
    {
        private IPgRepository data;

        public IndividualDiedEventHandler(IPgRepository data)
        {
            this.data = data;
        }

        public void HandleMessage(IndividualDiedEvent message)
        {
            
            data.NewIndividualEvent(message);
        }
    }
}