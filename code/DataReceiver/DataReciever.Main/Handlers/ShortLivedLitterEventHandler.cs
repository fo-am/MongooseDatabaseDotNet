using DataReciever.Main.Data;
using DataReciever.Main.Interfaces;
using DataReciever.Main.Model.LifeHistory;

namespace DataReciever.Main.Handlers
{
    internal class ShortLivedLitterEventHandler : IHandle<ShortLivedLitterEvent>
    {
        public void HandleMessage(ShortLivedLitterEvent message)
        {
            var data = new PgRepository();
            // data.InsertNewIndividual(message);
        }
    }
}