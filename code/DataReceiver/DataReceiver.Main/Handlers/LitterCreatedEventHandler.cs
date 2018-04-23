using DataReceiver.Main.Interfaces;
using DataReceiver.Main.Model;

namespace DataReceiver.Main.Handlers
{
    internal class LitterCreatedEventHandler : IHandle<LitterCreated>
    {
        private readonly IPgRepository data;

        public LitterCreatedEventHandler(IPgRepository data)
        {
            this.data = data;
        }

        public void HandleMessage(LitterCreated message)
        {
            {
                data.InsertNewLitter(message);
            }
        }
    }
}