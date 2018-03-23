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
            data.NewIndividualEvent(message);
            // set the litter to born (how can I find out what litter we are in!?)
            // we know the individual giving birth... so we can look up the pack from that then see if they have a 'not born' litter and mark it born

        }
    }
}