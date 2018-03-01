using DataReciever.Main.Data;
using DataReciever.Main.Interfaces;
using DataReciever.Main.Model.LifeHistory;

namespace DataReciever.Main.Handlers
{
    internal class IndividualStartEvictionEventHandler : IHandle<IndividualStartEvictionEvent>
    {
        public void HandleMessage(IndividualStartEvictionEvent message)
        {
            var data = new PgRepository();
            data.NewIndividualEvent(message);
        }
    }
}