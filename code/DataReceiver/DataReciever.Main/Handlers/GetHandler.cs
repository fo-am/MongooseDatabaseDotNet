using System;
using System.Collections.Generic;
using DataReciever.Main.Model;

namespace DataReciever.Main.Handlers
{
    internal class GetHandler
    {
        private readonly Dictionary<Type, dynamic> handlers = new Dictionary<Type, dynamic>();

        public GetHandler()
        {
            handlers.Add(typeof(IndividualCreated), new IndividualCreatedHandler());
            handlers.Add(typeof(PackCreated), new PackCreatedHandler());
            handlers.Add(typeof(WeightMeasure), new WeightHandler());
        }

        public void Handle<T>(T output)
        {
            if (handlers.ContainsKey(typeof(T)))
            {
                var handler = handlers[typeof(T)];
                handler.HandleMessage(output);
            }
            else
            {
                throw new Exception($"No handler found for type: {typeof(T)}");
            }
        }
    }
}