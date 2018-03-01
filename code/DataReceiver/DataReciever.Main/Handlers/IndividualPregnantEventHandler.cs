using DataReciever.Main.Data;
using DataReciever.Main.Interfaces;
using DataReciever.Main.Model.LifeHistory;

namespace DataReciever.Main.Handlers
{
    internal class IndividualPregnantEventHandler : IHandle<IndividualPregnantEvent>
    {
        public void HandleMessage(IndividualPregnantEvent message)
        {
            var data = new PgRepository();
            data.NewIndividualEvent(message);
        }
    }
}