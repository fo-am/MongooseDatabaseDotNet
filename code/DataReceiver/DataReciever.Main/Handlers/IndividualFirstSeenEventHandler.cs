using DataReciever.Main.Data;
using DataReciever.Main.Interfaces;
using DataReciever.Main.Model.LifeHistory;

namespace DataReciever.Main.Handlers
{
    internal class IndividualFirstSeenEventHandler : IHandle<IndividualFirstSeenEvent>
    {
        public void HandleMessage(IndividualFirstSeenEvent message)
        {
            var data = new PgRepository();
            // data.InsertNewIndividual(message);
        }
    }
}