using DataReceiver.Main.Data;
using DataReceiver.Main.Interfaces;
using DataReceiver.Main.Model;

namespace DataReceiver.Main.Handlers
{
    internal class PackMoveEventHandler : IHandle<PackMove>
    {
        private IPgRepository data;

        public PackMoveEventHandler(IPgRepository data)
        {
            this.data = data;
        }

        public void HandleMessage(PackMove message)
        {
            
            data.PackMove(message);
        }
    }
}