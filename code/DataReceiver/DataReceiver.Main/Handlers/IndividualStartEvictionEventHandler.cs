using DataReceiver.Main.Data;
using DataReceiver.Main.Interfaces;
using DataReceiver.Main.Model.LifeHistory;

namespace DataReceiver.Main.Handlers
{
    internal class IndividualStartEvictionEventHandler : IHandle<IndividualStartEvictionEvent>
    {
        private IPgRepository data;

        public IndividualStartEvictionEventHandler(IPgRepository data)
        {
            this.data = data;
        }

        public void HandleMessage(IndividualStartEvictionEvent message)
        {
            
            data.NewIndividualEvent(message);
        }
    }
}