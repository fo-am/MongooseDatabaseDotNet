using DataReciever.Main.Data;
using DataReciever.Main.Interfaces;
using DataReciever.Main.Model.LifeHistory;

namespace DataReciever.Main.Handlers
{
    internal class UnsuccessfulLitterEventHandler : IHandle<UnsuccessfulLitterEvent>
    {
        public void HandleMessage(UnsuccessfulLitterEvent message)
        {
            var data = new PgRepository();
            data.InsertNewLitterEvent(message);
        }
    }
}