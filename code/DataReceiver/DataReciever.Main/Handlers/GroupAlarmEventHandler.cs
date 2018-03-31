using DataReciever.Main.Data;
using DataReciever.Main.Interfaces;
using DataReciever.Main.Model;

namespace DataReciever.Main.Handlers
{
    internal class GroupAlarmEventHandler : IHandle<GroupAlarmEvent>
    {
        public void HandleMessage(GroupAlarmEvent message)
        {
            var data = new PgRepository();
            data.InsertGroupAlarm(message);
        }
    }
}