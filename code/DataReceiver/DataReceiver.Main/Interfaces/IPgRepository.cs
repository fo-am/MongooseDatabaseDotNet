using System;

using DataReceiver.Main.Model;
using DataReceiver.Main.Model.LifeHistory;
using DataReceiver.Main.Model.Oestrus;
using DataReceiver.Main.Model.PregnancyFocal;
using DataReceiver.Main.Model.PupFocal;

namespace DataReceiver.Main.Interfaces
{
    public interface IPgRepository
    {
        int FailedToHandleMessage(int entityId, Exception ex);
        void HandleGroupComposition(GroupComposition message);
        void HandleInterGroupInteraction(InterGroupInteractionEvent message);
        void HandlePregnancyFocal(PregnancyFocal message);
        void HandlePupFocal(PupFocal message);
        void InsertGroupAlarm(GroupAlarmEvent message);
        void InsertNewGroupMoveEvent(GroupMoveEvent message);
        void InsertNewIndividual(IndividualCreated message);
        void InsertNewLitter(LitterCreated message);
        void InsertNewLitterEvent(LifeHistoryEvent message);
        void InsertNewPack(PackCreated message);
        void InsertNewWeight(WeightMeasure message);
        void InsertOestrusEvent(OestrusEvent message);
        void MessageHandledOk(int entityId);
        void NewIndividualEvent(LifeHistoryEvent message);
        void PackEvent(LifeHistoryEvent message);
        void PackMove(PackMove message);
        int StoreMessage(string fullName, string message, string messageId);
    }
}