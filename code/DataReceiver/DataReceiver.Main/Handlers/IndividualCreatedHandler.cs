using DataReceiver.Main.Data;
using DataReceiver.Main.Interfaces;
using DataReceiver.Main.Model;

namespace DataReceiver.Main.Handlers
{
    internal class IndividualCreatedHandler : IHandle<IndividualCreated>
    {
        private IPgRepository data;

        public IndividualCreatedHandler(IPgRepository data)
        {
            this.data = data;
        }

        public void HandleMessage(IndividualCreated message)
        {
            
            data.InsertNewIndividual(message);
        }
    }
}