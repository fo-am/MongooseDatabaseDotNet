using System;
using System.Collections.Generic;
using DataReciever.Main.Model;
using DataReciever.Main.Model.LifeHistory;

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
            handlers.Add(typeof(EndPackEvent), new EndPackEventHandler());
            handlers.Add(typeof(LostPackEvent), new LostPackEventHandler());
            handlers.Add(typeof(FoundPackEvent), new FoundPackEventHandler());
            handlers.Add(typeof(UnsuccessfulLitterEvent), new UnsuccessfulLitterEventHandler());
            handlers.Add(typeof(ShortLivedLitterEvent), new ShortLivedLitterEventHandler());
            handlers.Add(typeof(SuccessfulLitterEvent), new SuccessfulLitterEventHandler());
            handlers.Add(typeof(IndividualAssumedDeadEvent), new IndividualAssumedDeadEventHandler());
            handlers.Add(typeof(IndividualDiedEvent), new IndividualDiedEventHandler());
            handlers.Add(typeof(IndividualLastSeenEvent), new IndividualLastSeenEventHandler());
            handlers.Add(typeof(IndividualFirstSeenEvent), new IndividualFirstSeenEventHandler());
            handlers.Add(typeof(IndividualStartEvictionEvent), new IndividualStartEvictionEventHandler());
            handlers.Add(typeof(IndividualEndEvictionEvent), new IndividualEndEvictionEventHandler());
            handlers.Add(typeof(IndividualLeaveEvent), new IndividualLeaveEventHandler());
            handlers.Add(typeof(IndividualReturnEvent), new IndividualReturnEventHandler());
            handlers.Add(typeof(IndividualImmigrateEvent), new IndividualImmigrateEventHandler());
            handlers.Add(typeof(IndividualEmmigrateEvent), new IndividualEmmigrateEventHandler());
            handlers.Add(typeof(IndividualPregnantEvent), new IndividualPregnantEventHandler());
            handlers.Add(typeof(IndividualAbortEvent), new IndividualAbortEventHandler());
            handlers.Add(typeof(IndividualBirthEvent), new IndividualBirthEventHandler());

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