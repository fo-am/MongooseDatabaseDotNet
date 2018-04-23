using DataReceiver.Main.Interfaces;
using DataReceiver.Main.Model;

namespace DataReceiver.Main.Handlers
{
    internal class GroupCompositionHandler : IHandle<GroupComposition>
    {
        private readonly IPgRepository data;

        public GroupCompositionHandler(IPgRepository data)
        {
            this.data = data;
        }

        public void HandleMessage(GroupComposition message)
        {
            data.HandleGroupComposition(message);
        }
    }
}