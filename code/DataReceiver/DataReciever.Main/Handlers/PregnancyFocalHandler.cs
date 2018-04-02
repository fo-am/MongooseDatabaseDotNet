using DataReciever.Main.Data;
using DataReciever.Main.Interfaces;
using DataReciever.Main.Model.PregnancyFocal;

namespace DataReciever.Main.Handlers
{
    internal class PregnancyFocalHandler : IHandle<PregnancyFocal>
    {
        public void HandleMessage(PregnancyFocal message)
        {
            var data = new PgRepository();
            data.HandlePregnancyFocal(message);
        }
    }
}