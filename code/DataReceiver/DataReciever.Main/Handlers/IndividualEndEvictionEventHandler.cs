using DataReciever.Main.Data;
using DataReciever.Main.Interfaces;
using DataReciever.Main.Model.LifeHistory;

namespace DataReciever.Main.Handlers
{
    internal class IndividualEndEvictionEventHandler : IHandle<IndividualEndEvictionEvent>
    {
        public void HandleMessage(IndividualEndEvictionEvent message)
        {
            var data = new PgRepository();
            // data.InsertNewIndividual(message);
        }
    }
}