using DataReciever.Main.Data;
using DataReciever.Main.Interfaces;
using DataReciever.Main.Model;

namespace DataReciever.Main.Handlers
{
    internal class WeightHandler : IHandle<WeightMeasure>
    {
        public void HandleMessage(WeightMeasure message)
        {
            var data = new PgRepository();
            data.InsertNewWeight(message);
        }
    }
}