using DataReciever.Main.Data;
using DataReciever.Main.Interfaces;
using DataReciever.Main.Model.PupFocal;

namespace DataReciever.Main.Handlers
{
    internal class PupFocalHandler : IHandle<PupFocal>
    {
        public void HandleMessage(PupFocal message)
        {
            var data = new PgRepository();
            data.HandlePupFocal(message);
        }
    }
}