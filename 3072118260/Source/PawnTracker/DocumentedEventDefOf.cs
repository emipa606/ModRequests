using Verse;
using RimWorld;
using Verse.AI;

namespace PawnTrackerMain
{
    [DefOf]
    public static class DocumentedEventDefOf
    {
        public static JoinEventDef UnknownJoin;
        public static JoinEventDef WandererJoins;
        public static JoinEventDef StartingColonist;
        [MayRequireBiotech]
        public static JoinEventDef BornToColonist;
        
        [MayRequireBiotech]
        public static JoinEventDef BornToColonistSickly;
        public static JoinEventDef Bought;
        public static JoinEventDef Recruited;
        public static JoinEventDef Tamed;
        [MayRequireBiotech]
        public static JoinEventDef Adopted;
        public static JoinEventDef JoinAfterRescue;
        public static JoinEventDef StrangerInBlackJoin;
        public static JoinEventDef RansomReturn;
        public static JoinEventDef QuestReward;
        public static JoinEventDef RescuedQuest;
        public static JoinEventDef GuestRecruited;
        public static JoinEventDef GuestRecruitedForced;

        public static ArriveEventDef UnknownArrive;
        public static ArriveEventDef WildManWanderedIn;
        public static ArriveEventDef Raid;
        public static ArriveEventDef TransportPodCrash;
        public static ArriveEventDef ShuttleCrashSoldier;
        public static ArriveEventDef ShuttleCrashCivilian;
        public static ArriveEventDef SpaceBattleCrash;
        [MayRequireBiotech]
        public static ArriveEventDef BornToPrisoner;
        [MayRequireBiotech]
        public static ArriveEventDef BornToPrisonerSickly;
        public static ArriveEventDef TradeCaravanArrived;
        public static ArriveEventDef PassingThroughArrived;
        public static ArriveEventDef FriendliesArrived;
        public static ArriveEventDef IncidentMapHostile;
        public static ArriveEventDef IncidentMapFriendly;
        public static ArriveEventDef IncidentMapCaptive;
        
        public static ArriveEventDef ArrivedAsGuest;
        public static ArriveEventDef IncidentMapNeedRescue;
        public static ArriveEventDef VisitorGroupArrived;
        public static ArriveEventDef HuntingPartyArrived;
        public static ArriveEventDef VisitForTreatment;

        public static LeaveEventDef UnknownLeave;
        public static LeaveEventDef Kidnapped;
        public static LeaveEventDef Sold;
        public static LeaveEventDef PrisonerEscaped;
        public static LeaveEventDef PrisonerReleased;
        public static LeaveEventDef Lost;
        public static LeaveEventDef MiscLeaveKnown;
        public static LeaveEventDef FinishedRaid;
        public static LeaveEventDef TradeCaravanLeft;
        public static LeaveEventDef PassingThroughLeft;
        public static LeaveEventDef FriendliesLeft;
        public static LeaveEventDef FinishVisitAsGuest;
        public static LeaveEventDef VisitorGroupLeft;
        public static LeaveEventDef HuntingPartyLeft;
        public static LeaveEventDef NoJoinAfterRescue;
        public static LeaveEventDef LeaveAfterTreatment;

        public static DeathEventDef UnknownDeath;
        public static DeathEventDef Killed;
        public static DeathEventDef Died;
        public static DeathEventDef Executed;
        [MayRequireBiotech]
        public static DeathEventDef Stillborn;
        [MayRequireBiotech]
        public static DeathEventDef DiedInChildbirth;
        public static DeathEventDef GameEventInjuries;

        public static LifeEventDef NewLovers;
        public static LifeEventDef GotMarried;
        public static LifeEventDef LostBodyPart;
        public static LifeEventDef BodyPartRemoved;
        public static LifeEventDef InfectedBodyPartRemoved;
        public static LifeEventDef BodyPartAmputated;
        public static LifeEventDef BodyPartHarvested;
        [MayRequireBiotech]
        public static LifeEventDef GaveBirthHealthy;
        [MayRequireBiotech]
        public static LifeEventDef GaveBirthSickly;
        [MayRequireBiotech]
        public static LifeEventDef GaveBirthStillborn;
        public static LifeEventDef ExLovers;
        public static LifeEventDef Divorced;
        public static LifeEventDef BecameMechinator;
        public static LifeEventDef PawnDied;
        public static LifeEventDef BecameUncleOrAunt;
        public static LifeEventDef BecameFather;
        public static LifeEventDef BecameGrandparent;
        public static LifeEventDef BecameSibling;
        public static LifeEventDef BecameHalfSibling;
        public static LifeEventDef BecameGreatGrandparent;
        public static LifeEventDef GotEngaged;
        public static LifeEventDef NewBodyPart;
        public static LifeEventDef Miscarried;
        public static LifeEventDef PartnerMiscarried;
        public static LifeEventDef TerminatedPregnancy;
        public static LifeEventDef Pregnant;
        public static LifeEventDef PartnerPregnant;
        public static LifeEventDef BecameGuest;
        public static LifeEventDef Captured;
        public static LifeEventDef CapturedQuest;
        public static LifeEventDef CapturedQuestReward;
        public static LifeEventDef CapturedCaravanAmbush;
        public static LifeEventDef CapturedAbroad;
        public static LifeEventDef Rescued;
        public static LifeEventDef GaveUpAndLeft;
        public static LifeEventDef DecidedToStay;
        public static ArriveEventDef Resurrected;

        static DocumentedEventDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof (DocumentedEventDefOf));
    }
}