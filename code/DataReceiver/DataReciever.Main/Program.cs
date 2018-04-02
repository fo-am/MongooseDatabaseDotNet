using System;

using DataReciever.Main.Model;
using DataReciever.Main.Model.LifeHistory;
using DataReciever.Main.Model.Oestrus;
using DataReciever.Main.Model.PregnancyFocal;
using DataReciever.Main.Model.PupFocal;

using NLog;
using NLog.Config;

namespace DataReciever.Main
{
    public class Program
    {
        private static void Main(string[] args)
        {
            LogManager.Configuration = new XmlLoggingConfiguration("nlog.config");

            var reciever = new Reciever();

            reciever.Recieve<PackCreated>();
            reciever.Recieve<IndividualCreated>();
            reciever.Recieve<WeightMeasure>();

            reciever.Recieve<EndPackEvent>();
            reciever.Recieve<LostPackEvent>();
            reciever.Recieve<FoundPackEvent>();
            reciever.Recieve<UnsuccessfulLitterEvent>();
            reciever.Recieve<ShortLivedLitterEvent>();
            reciever.Recieve<SuccessfulLitterEvent>();
            reciever.Recieve<IndividualAssumedDeadEvent>();
            reciever.Recieve<IndividualDiedEvent>();
            reciever.Recieve<IndividualLastSeenEvent>();
            reciever.Recieve<IndividualFirstSeenEvent>();
            reciever.Recieve<IndividualStartEvictionEvent>();
            reciever.Recieve<IndividualEndEvictionEvent>();
            reciever.Recieve<IndividualLeaveEvent>();
            reciever.Recieve<IndividualReturnEvent>();
            reciever.Recieve<IndividualImmigrateEvent>();
            reciever.Recieve<IndividualEmmigrateEvent>();
            reciever.Recieve<IndividualPregnantEvent>();
            reciever.Recieve<IndividualAbortEvent>();
            reciever.Recieve<IndividualBirthEvent>();

            reciever.Recieve<PackMove>();
            reciever.Recieve<LitterCreated>();

            reciever.Recieve<InterGroupInteractionEvent>();
            reciever.Recieve<GroupAlarmEvent>();
            reciever.Recieve<GroupMoveEvent>();

            reciever.Recieve<OestrusEvent>();
            reciever.Recieve<PupFocal>();

            reciever.Recieve<PregnancyFocal>();

            Console.WriteLine("Waiting for values. Return to Exit.");
            Console.ReadLine();
        }
    }
}