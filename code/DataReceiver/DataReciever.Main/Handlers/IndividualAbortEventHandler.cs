using DataReciever.Main.Data;
using DataReciever.Main.Interfaces;
using DataReciever.Main.Model.LifeHistory;

namespace DataReciever.Main.Handlers
{
    internal class IndividualAbortEventHandler : IHandle<IndividualAbortEvent>
    {
        public void HandleMessage(IndividualAbortEvent message)
        {
            var data = new PgRepository();
            // data.InsertNewIndividual(message);
        }
    }
}