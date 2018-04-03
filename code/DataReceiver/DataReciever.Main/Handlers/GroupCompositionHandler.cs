using DataReciever.Main.Data;
using DataReciever.Main.Interfaces;
using DataReciever.Main.Model;
using DataReciever.Main.Model.LifeHistory;

namespace DataReciever.Main.Handlers
{
    internal class GroupCompositionHandler : IHandle<GroupComposition>
    {
        public void HandleMessage(GroupComposition message)
        {
            var data = new PgRepository();
            data.HandleGroupComposition(message);
        }
    }
}