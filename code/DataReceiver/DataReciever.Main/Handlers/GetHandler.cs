using System;
using System.Collections.Generic;

namespace DataReciever.Main.Handlers
{
    internal class GetHandler
    {
        public GetHandler()
        {
            handlers.Add(typeof(IndividualCreated), new IndividualCreatedHandler());
        }

        private readonly Dictionary<Type, dynamic> handlers = new Dictionary<Type, dynamic>();

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