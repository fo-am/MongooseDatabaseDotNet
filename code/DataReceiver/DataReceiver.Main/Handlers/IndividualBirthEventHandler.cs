using DataReceiver.Main.Data;
using DataReceiver.Main.Interfaces;
using DataReceiver.Main.Model.LifeHistory;

namespace DataReceiver.Main.Handlers
{
    internal class IndividualBirthEventHandler : IHandle<IndividualBirthEvent>
    {
        private IPgRepository data;

        public IndividualBirthEventHandler(IPgRepository data)
        {
            this.data = data;
        }

        public void HandleMessage(IndividualBirthEvent message)
        {
            
            data.NewIndividualEvent(message);
        }
    }
}