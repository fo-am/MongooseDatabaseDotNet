using DataReciever.Main.Data;
using DataReciever.Main.Interfaces;
using DataReciever.Main.Model;

namespace DataReciever.Main.Handlers
{
    internal class GroupMoveEventHandler : IHandle<GroupMoveEvent>
    {
        public void HandleMessage(GroupMoveEvent message)
        {
            var data = new PgRepository();
            data.InsertNewGroupMoveEvent(message);
        }
    }
}