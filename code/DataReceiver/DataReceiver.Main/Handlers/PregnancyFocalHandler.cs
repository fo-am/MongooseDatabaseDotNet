using DataReceiver.Main.Data;
using DataReceiver.Main.Interfaces;
using DataReceiver.Main.Model.PregnancyFocal;

namespace DataReceiver.Main.Handlers
{
    internal class PregnancyFocalHandler : IHandle<PregnancyFocal>
    {
        private readonly IPgRepository data;

        public PregnancyFocalHandler(IPgRepository data)
        {
            this.data = data;

        }

        public void HandleMessage(PregnancyFocal message)
        {
            data.HandlePregnancyFocal(message);
        }
    }
}
