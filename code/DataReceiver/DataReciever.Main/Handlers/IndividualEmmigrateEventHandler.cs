using DataReciever.Main.Data;
using DataReciever.Main.Interfaces;
using DataReciever.Main.Model.LifeHistory;

namespace DataReciever.Main.Handlers
{
    internal class IndividualEmmigrateEventHandler : IHandle<IndividualEmmigrateEvent>
    {
        public void HandleMessage(IndividualEmmigrateEvent message)
        {
            var data = new PgRepository();
            data.NewIndividualEvent(message);
        }
    }
}