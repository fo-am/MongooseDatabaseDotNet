using DataReceiver.Main.Data;
using DataReceiver.Main.Interfaces;
using DataReceiver.Main.Model;

namespace DataReceiver.Main.Handlers
{
    internal class InterGroupInteractionEventHandler : IHandle<InterGroupInteractionEvent>
    {
        private IPgRepository data;

        public InterGroupInteractionEventHandler(IPgRepository data)
        {
            this.data = data;
        }

        public void HandleMessage(InterGroupInteractionEvent message)
        {
            
             data.HandleInterGroupInteraction(message);
        }
    }
}