using DataReceiver.Main.Data;
using DataReceiver.Main.Interfaces;
using DataReceiver.Main.Model.PupFocal;

namespace DataReceiver.Main.Handlers
{
    internal class PupFocalHandler : IHandle<PupFocal>
    {
        private readonly IPgRepository data;

        public PupFocalHandler(IPgRepository data)
        {
            this.data = data;
        }

        public void HandleMessage(PupFocal message)
        {
         
            data.HandlePupFocal(message);
        }
    }
}