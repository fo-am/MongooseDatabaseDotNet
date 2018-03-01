using DataReciever.Main.Data;
using DataReciever.Main.Interfaces;
using DataReciever.Main.Model.LifeHistory;

namespace DataReciever.Main.Handlers
{
    internal class IndividualImmigrateEventHandler : IHandle<IndividualImmigrateEvent>
    {
        public void HandleMessage(IndividualImmigrateEvent message)
        {
            var data = new PgRepository();
            data.NewIndividualEvent(message);
        }
    }
}