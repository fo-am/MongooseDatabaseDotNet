using DataReceiver.Main.Data;
using DataReceiver.Main.Interfaces;
using DataReceiver.Main.Model;

namespace DataReceiver.Main.Handlers
{
    internal class WeightHandler : IHandle<WeightMeasure>
    {
        private IPgRepository data;

        public WeightHandler(IPgRepository data)
        {
            this.data = data;
        }

        public void HandleMessage(WeightMeasure message)
        {
            
            data.InsertNewWeight(message);
        }
    }
}