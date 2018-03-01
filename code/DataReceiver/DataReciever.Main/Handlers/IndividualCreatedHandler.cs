using DataReciever.Main.Data;
using DataReciever.Main.Interfaces;
using DataReciever.Main.Model;
using DataReciever.Main.Model.LifeHistory;

namespace DataReciever.Main.Handlers
{
    internal class IndividualCreatedHandler : IHandle<IndividualCreated>
    {
        public void HandleMessage(IndividualCreated message)
        {
            var data = new PgRepository();
            data.InsertNewIndividual(message);
        }
    }
}