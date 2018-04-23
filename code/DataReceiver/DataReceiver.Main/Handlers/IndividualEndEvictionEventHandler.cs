using DataReceiver.Main.Data;
using DataReceiver.Main.Interfaces;
using DataReceiver.Main.Model.LifeHistory;

namespace DataReceiver.Main.Handlers
{
    internal class IndividualEndEvictionEventHandler : IHandle<IndividualEndEvictionEvent>
    {
        private IPgRepository data;

        public IndividualEndEvictionEventHandler(IPgRepository data)
        {
            this.data = data;
        }

        public void HandleMessage(IndividualEndEvictionEvent message)
        {
            
            data.NewIndividualEvent(message);
        }
    }
}