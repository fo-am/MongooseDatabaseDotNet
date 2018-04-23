using DataReceiver.Main.Data;
using DataReceiver.Main.Interfaces;
using DataReceiver.Main.Model;

namespace DataReceiver.Main.Handlers
{
    internal class GroupMoveEventHandler : IHandle<GroupMoveEvent>
    {
        private IPgRepository data;

        public GroupMoveEventHandler(IPgRepository data)
        {
            this.data = data;
        }

        public void HandleMessage(GroupMoveEvent message)
        {
            data.InsertNewGroupMoveEvent(message);
        }
    }
}