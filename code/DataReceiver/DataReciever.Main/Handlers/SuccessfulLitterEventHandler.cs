using DataReciever.Main.Data;
using DataReciever.Main.Interfaces;
using DataReciever.Main.Model.LifeHistory;

namespace DataReciever.Main.Handlers
{
    internal class SuccessfulLitterEventHandler : IHandle<SuccessfulLitterEvent>
    {
        public void HandleMessage(SuccessfulLitterEvent message)
        {
            var data = new PgRepository();
            data.InsertNewLitterEvent(message);
        }
    }
}