using DataReciever.Main.Data;
using DataReciever.Main.Interfaces;
using DataReciever.Main.Model.LifeHistory;

namespace DataReciever.Main.Handlers
{
    internal class IndividualLeaveEventHandler : IHandle<IndividualLeaveEvent>
    {
        public void HandleMessage(IndividualLeaveEvent message)
        {
            var data = new PgRepository();
            data.NewIndividualEvent(message);
        }
    }
}