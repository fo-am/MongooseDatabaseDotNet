using DataReciever.Main.Data;
using DataReciever.Main.Interfaces;
using DataReciever.Main.Model.LifeHistory;

namespace DataReciever.Main.Handlers
{
    internal class IndividualDiedEventHandler : IHandle<IndividualDiedEvent>
    {
        public void HandleMessage(IndividualDiedEvent message)
        {
            var data = new PgRepository();
            // data.InsertNewIndividual(message);
        }
    }
}