using DataReceiver.Main.Interfaces;
using DataReceiver.Main.Model.LifeHistory;

namespace DataReceiver.Main.Handlers
{
    internal class EndPackEventHandler : IHandle<EndPackEvent>
    {
        private readonly IPgRepository _data;

        public EndPackEventHandler(IPgRepository data)
        {
            _data = data;
        }

        public void HandleMessage(EndPackEvent message)
        {
            _data.PackEvent(message);
        }
    }
}