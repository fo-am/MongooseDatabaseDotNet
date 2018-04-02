using System;
using System.Collections.Generic;
using DataReciever.Main.Model;
using DataReciever.Main.Model.LifeHistory;
using DataReciever.Main.Model.Oestrus;
using DataReciever.Main.Model.PupFocal;

namespace DataReciever.Main.Handlers
{
    internal class GetHandler
    {
        private readonly Dictionary<Type, dynamic> handlers = new Dictionary<Type, dynamic>();

        public GetHandler()
        {
            handlers.Add(typeof(WeightMeasure), new WeightHandler());

            handlers.Add(typeof(PackCreated), new PackCreatedHandler());
            handlers.Add(typeof(PackMove), new PackMoveEventHandler());
            handlers.Add(typeof(EndPackEvent), new EndPackEventHandler());
            handlers.Add(typeof(LostPackEvent), new LostPackEventHandler());
            handlers.Add(typeof(FoundPackEvent), new FoundPackEventHandler());

            handlers.Add(typeof(IndividualCreated), new IndividualCreatedHandler());
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

            handlers.Add(typeof(LitterCreated), new LitterCreatedEventHandler());
            handlers.Add(typeof(ShortLivedLitterEvent), new ShortLivedLitterEventHandler());
            handlers.Add(typeof(SuccessfulLitterEvent), new SuccessfulLitterEventHandler());
            handlers.Add(typeof(UnsuccessfulLitterEvent), new UnsuccessfulLitterEventHandler());
            handlers.Add(typeof(InterGroupInteractionEvent), new InterGroupInteractionEventHandler());
            handlers.Add(typeof(GroupAlarmEvent), new GroupAlarmEventHandler());
            handlers.Add(typeof(GroupMoveEvent), new GroupMoveEventHandler());
            handlers.Add(typeof(OestrusEvent), new OestrusEventHandler());
            handlers.Add(typeof(PupFocal), new PupFocalHandler());
            
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