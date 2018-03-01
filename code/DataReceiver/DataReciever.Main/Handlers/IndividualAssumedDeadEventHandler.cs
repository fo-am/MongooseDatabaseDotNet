using DataReciever.Main.Data;
using DataReciever.Main.Interfaces;
using DataReciever.Main.Model.LifeHistory;

namespace DataReciever.Main.Handlers
{
    internal class IndividualAssumedDeadEventHandler : IHandle<IndividualAssumedDeadEvent>
    {
        public void HandleMessage(IndividualAssumedDeadEvent message)
        {
            var data = new PgRepository();
            data.NewIndividualEvent(message);
        }
    }
}