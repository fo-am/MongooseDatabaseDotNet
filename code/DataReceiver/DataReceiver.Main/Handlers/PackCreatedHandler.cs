using DataReceiver.Main.Data;
using DataReceiver.Main.Interfaces;
using DataReceiver.Main.Model;

namespace DataReceiver.Main.Handlers
{
    public class PackCreatedHandler : IHandle<PackCreated>
    {
        private IPgRepository data;

        public PackCreatedHandler(IPgRepository data)
        {
            this.data = data;
        }

        public void HandleMessage(PackCreated message)
        {
            
            data.InsertNewPack(message);
        }
    }
}