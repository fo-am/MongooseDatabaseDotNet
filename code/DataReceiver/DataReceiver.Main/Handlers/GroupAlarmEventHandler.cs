using DataReceiver.Main.Data;
using DataReceiver.Main.Interfaces;
using DataReceiver.Main.Model;

namespace DataReceiver.Main.Handlers
{
    internal class GroupAlarmEventHandler : IHandle<GroupAlarmEvent>
    {
        private IPgRepository data;

        public GroupAlarmEventHandler(IPgRepository data)
        {
            this.data = data;
        }

        public void HandleMessage(GroupAlarmEvent message)
        {
            
            data.InsertGroupAlarm(message);
        }
    }
}