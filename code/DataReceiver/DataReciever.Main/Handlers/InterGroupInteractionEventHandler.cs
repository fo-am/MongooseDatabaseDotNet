using DataReciever.Main.Data;
using DataReciever.Main.Interfaces;
using DataReciever.Main.Model;

namespace DataReciever.Main.Handlers
{
    internal class InterGroupInteractionEventHandler : IHandle<InterGroupInteractionEvent>
    {
        public void HandleMessage(InterGroupInteractionEvent message)
        {
            var data = new PgRepository();
             data.HandleInterGroupInteraction(message);
        }
    }
}