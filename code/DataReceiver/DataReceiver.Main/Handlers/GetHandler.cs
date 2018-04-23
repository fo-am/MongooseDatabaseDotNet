using System;
using System.Collections.Generic;

using DataReceiver.Main.Interfaces;
using DataReceiver.Main.Model;
using DataReceiver.Main.Model.LifeHistory;
using DataReceiver.Main.Model.Oestrus;
using DataReceiver.Main.Model.PregnancyFocal;
using DataReceiver.Main.Model.PupFocal;

namespace DataReceiver.Main.Handlers
{
    internal class GetHandler : IGetHandler
    {
        private readonly Dictionary<Type, dynamic> handlers = new Dictionary<Type, dynamic>();

        public GetHandler(IPgRepository data)
        {
            handlers.Add(typeof(WeightMeasure), new WeightHandler(data));

            handlers.Add(typeof(PackCreated), new PackCreatedHandler(data));
            handlers.Add(typeof(PackMove), new PackMoveEventHandler(data));
            handlers.Add(typeof(EndPackEvent), new EndPackEventHandler(data));
            handlers.Add(typeof(LostPackEvent), new LostPackEventHandler(data));
            handlers.Add(typeof(FoundPackEvent), new FoundPackEventHandler(data));

            handlers.Add(typeof(IndividualCreated), new IndividualCreatedHandler(data));
            handlers.Add(typeof(IndividualAssumedDeadEvent), new IndividualAssumedDeadEventHandler(data));
            handlers.Add(typeof(IndividualDiedEvent), new IndividualDiedEventHandler(data));
            handlers.Add(typeof(IndividualLastSeenEvent), new IndividualLastSeenEventHandler(data));
            handlers.Add(typeof(IndividualFirstSeenEvent), new IndividualFirstSeenEventHandler(data));
            handlers.Add(typeof(IndividualStartEvictionEvent), new IndividualStartEvictionEventHandler(data));
            handlers.Add(typeof(IndividualEndEvictionEvent), new IndividualEndEvictionEventHandler(data));
            handlers.Add(typeof(IndividualLeaveEvent), new IndividualLeaveEventHandler(data));
            handlers.Add(typeof(IndividualReturnEvent), new IndividualReturnEventHandler(data));
            handlers.Add(typeof(IndividualImmigrateEvent), new IndividualImmigrateEventHandler(data));
            handlers.Add(typeof(IndividualEmmigrateEvent), new IndividualEmmigrateEventHandler(data));
            handlers.Add(typeof(IndividualPregnantEvent), new IndividualPregnantEventHandler(data));
            handlers.Add(typeof(IndividualAbortEvent), new IndividualAbortEventHandler(data));
            handlers.Add(typeof(IndividualBirthEvent), new IndividualBirthEventHandler(data));

            handlers.Add(typeof(LitterCreated), new LitterCreatedEventHandler(data));
            handlers.Add(typeof(ShortLivedLitterEvent), new ShortLivedLitterEventHandler(data));
            handlers.Add(typeof(SuccessfulLitterEvent), new SuccessfulLitterEventHandler(data));
            handlers.Add(typeof(UnsuccessfulLitterEvent), new UnsuccessfulLitterEventHandler(data));
            handlers.Add(typeof(InterGroupInteractionEvent), new InterGroupInteractionEventHandler(data));
            handlers.Add(typeof(GroupAlarmEvent), new GroupAlarmEventHandler(data));
            handlers.Add(typeof(GroupMoveEvent), new GroupMoveEventHandler(data));
            handlers.Add(typeof(OestrusEvent), new OestrusEventHandler(data));
            handlers.Add(typeof(PupFocal), new PupFocalHandler(data));
            handlers.Add(typeof(PregnancyFocal), new PregnancyFocalHandler(data));
            handlers.Add(typeof(GroupComposition), new GroupCompositionHandler(data));
            

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