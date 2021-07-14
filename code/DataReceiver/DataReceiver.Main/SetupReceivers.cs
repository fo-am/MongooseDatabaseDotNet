using System;

using NLog;

using DataReceiver.Main.Interfaces;
using DataReceiver.Main.Model;
using DataReceiver.Main.Model.LifeHistory;
using DataReceiver.Main.Model.Oestrus;
using DataReceiver.Main.Model.PregnancyFocal;
using DataReceiver.Main.Model.PupFocal;

using RabbitMQ.Client;

namespace DataReceiver.Main
{
    public class SetupReceivers : ISetupReceivers
    {
        private readonly ILogger logger;
        private readonly IConnection connection;
        private readonly IPgRepository data;
        private readonly IGetHandler handler;

        public SetupReceivers(ILogger logger, IConnection connection, IPgRepository data, IGetHandler handler)
        {
            this.logger = logger;
            this.connection = connection;
            this.data = data;
            this.handler = handler;
        }

        public void DoWork()
        {
            var receiver = new Receiver(logger, connection, data, handler);

            receiver.Receive<PackCreated>();
            receiver.Receive<IndividualCreated>();
            receiver.Receive<WeightMeasure>();

            receiver.Receive<EndPackEvent>();
            receiver.Receive<LostPackEvent>();
            receiver.Receive<FoundPackEvent>();
            receiver.Receive<UnsuccessfulLitterEvent>();
            receiver.Receive<ShortLivedLitterEvent>();
            receiver.Receive<SuccessfulLitterEvent>();
            receiver.Receive<IndividualAssumedDeadEvent>();
            receiver.Receive<IndividualDiedEvent>();
            receiver.Receive<IndividualLastSeenEvent>();
            receiver.Receive<IndividualFirstSeenEvent>();
            receiver.Receive<IndividualStartEvictionEvent>();
            receiver.Receive<IndividualEndEvictionEvent>();
            receiver.Receive<IndividualLeaveEvent>();
            receiver.Receive<IndividualReturnEvent>();
            receiver.Receive<IndividualImmigrateEvent>();
            receiver.Receive<IndividualEmmigrateEvent>();
            receiver.Receive<IndividualPregnantEvent>();
            receiver.Receive<IndividualAbortEvent>();
            receiver.Receive<IndividualBirthEvent>();

            receiver.Receive<PackMove>();
            receiver.Receive<LitterCreated>();

            receiver.Receive<InterGroupInteractionEvent>();
            receiver.Receive<GroupAlarmEvent>();
            receiver.Receive<GroupMoveEvent>();

            receiver.Receive<OestrusEvent>();
            receiver.Receive<PupFocal>();

            receiver.Receive<PregnancyFocal>();
            receiver.Receive<GroupComposition>();

            Console.WriteLine("Waiting for values. Return to Exit.");
            Console.ReadLine();
        }
    }
}