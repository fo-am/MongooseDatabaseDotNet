using DataReciever.Main.Data;
using DataReciever.Main.Interfaces;
using DataReciever.Main.Model.LifeHistory;

namespace DataReciever.Main.Handlers
{
    internal class IndividualBirthEventHandler : IHandle<IndividualBirthEvent>
    {
        public void HandleMessage(IndividualBirthEvent message)
        {
            var data = new PgRepository();
            // data.InsertNewIndividual(message);
        }
    }
}