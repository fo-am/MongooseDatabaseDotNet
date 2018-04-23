using DataReceiver.Main.Data;
using DataReceiver.Main.Interfaces;
using DataReceiver.Main.Model.LifeHistory;

namespace DataReceiver.Main.Handlers
{
    internal class IndividualPregnantEventHandler : IHandle<IndividualPregnantEvent>
    {
        private IPgRepository data;

        public IndividualPregnantEventHandler(IPgRepository data)
        {
            this.data = data;
        }

        public void HandleMessage(IndividualPregnantEvent message)
        {
            
            data.NewIndividualEvent(message);
        }
    }
}