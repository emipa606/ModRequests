using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Sound;
using static System.String;

namespace Hospitality.Utilities;

public static class GuestUtility
{
    public const int InteractIntervalAbsoluteMin = 360; // changed from 120
    public const int MaxOpinionForEnemy = -20;
    public static readonly DutyDef relaxDef = DefDatabase<DutyDef>.GetNamed("Relax");
    private static readonly TraderKindDef traderKindDef = DefDatabase<TraderKindDef>.GetNamed("Guest");
    private static readonly JobDef therapyJobDef = DefDatabase<JobDef>.GetNamedSilentFail("ReceiveTherapy");

    private static readonly string labelRecruitSuccess = "LetterLabelMessageRecruitSuccess".Translate(); // from core
    private static readonly string labelRecruitFactionAnger = "LetterLabelRecruitFactionAnger".Translate();
    private static readonly string labelRecruitFactionPlease = "LetterLabelRecruitFactionPlease".Translate();
    private static readonly string labelRecruitFactionChiefAnger = "LetterLabelRecruitFactionChiefAnger".Translate();
    private static readonly string labelRecruitFactionChiefPlease = "LetterLabelRecruitFactionChiefPlease".Translate();
    private static readonly string txtRecruitSuccess = "MessageGuestRecruitSuccess";
    private static readonly string txtForcedRecruit = "MessageGuestForcedRecruit";
    private static readonly string txtRecruitFactionAnger = "RecruitFactionAnger".Translate();
    private static readonly string txtRecruitFactionPlease = "RecruitFactionPlease".Translate();
    private static readonly string txtRecruitFactionAngerLeaderless = "RecruitFactionAngerLeaderless".Translate();
    private static readonly string txtRecruitFactionPleaseLeaderless = "RecruitFactionPleaseLeaderless".Translate();
    private static readonly string txtLostGroupFactionAnger = "LostGroupFactionAnger".Translate();
    private static readonly string txtLostGroupFactionAngerLeaderless = "LostGroupFactionAngerLeaderless".Translate();

    private static readonly StatDef statRecruitRelationshipDamage = StatDef.Named("RecruitRelationshipDamage");
    private static readonly StatDef statForcedRecruitRelationshipDamage = StatDef.Named("ForcedRecruitRelationshipDamage");
    private static readonly StatDef statRecruitEffectivity = StatDef.Named("RecruitEffectivity");

    private static readonly SimpleCurve recruitChanceOpinionCurve = [new CurvePoint(0f, 5), new CurvePoint(0.5f, 20), new CurvePoint(1f, 30)];

    private static readonly Dictionary<int, bool> relatedCache = new();
    private static int relatedCacheNextClearTick;

    private static RoyalTitleDef[] titleDefs;

    private static readonly HediffDef hediffDeathAcidifier = DefDatabase<HediffDef>.GetNamedSilentFail("DeathAcidifier");

    private static FieldInfo _fieldPriorities = typeof(Pawn_WorkSettings).GetField("priorities", BindingFlags.NonPublic | BindingFlags.Instance);

    public static RoyalTitleDef[] AllTitles
    {
        get
        {
            if (titleDefs == null)
            {
                Initialize();
            }

            return titleDefs;
        }
        private set => titleDefs = value;
    }

    public static Faction[] DistinctFactions { get; private set; }

    /// <summary>
    ///     For things that need to be loaded at map start
    /// </summary>
    public static void Initialize()
    {
        List<Faction> factions = [];
        List<RoyalTitleDef> titles = [];
        foreach (var faction in Find.FactionManager.AllFactions.Where(f => f.def.HasRoyalTitles))
        {
            factions.Add(faction);
            foreach (var titleDef in faction.def.RoyalTitlesAllInSeniorityOrderForReading)
            {
                titles.Add(titleDef);
            }
        }

        DistinctFactions = factions.GroupBy(f => f.def).Select(x => x.First()).ToArray();
        AllTitles = titles.Distinct().ToArray();
        //Log.Message($"Hospitality: Read {DistinctFactions.Length} factions with royal titles, {AllTitles.Length} royal titles.");
    }

    public static bool IsRelaxing(this Pawn pawn)
    {
        return pawn.mindState.duty != null && pawn.mindState.duty.def == relaxDef;
    }

    public static bool IsTraveling(this Pawn pawn)
    {
        return pawn.mindState.duty != null && pawn.mindState.duty.def == DutyDefOf.TravelOrLeave;
    }

    public static bool MayBuy(this Pawn pawn)
    {
        var guestComp = pawn.CompGuest();
        return guestComp?.ShoppingArea != null;
    }

    /// <summary>
    ///     If this method returns false `comp` *CAN* be null.
    /// </summary>
    public static bool IsArrivedGuest(this Pawn pawn, out CompGuest comp)
    {
        comp = null;
        if (!IsGuest(pawn)) return false;

        comp = pawn.CompGuest();

        return comp.arrived;
    }

    public static bool IsGuest(this Pawn pawn)
    {
        if (pawn == null || pawn.mapIndexOrState < 0) return false;

        var cachedComponent = pawn.GetMapComponent();
        return cachedComponent?.PresentGuests.Contains(pawn) == true;
    }

    public static bool IsTrader(this Pawn pawn, bool makeValidPawnCheck = true)
    {
        if (pawn == null) return false;
        try
        {
            if (makeValidPawnCheck && !IsValidPawn(pawn)) return false;
            return pawn.IsInTraderState();
        }
        catch (Exception e)
        {
            Log.Warning(e.Message);
            return false;
        }
    }

    public static bool IsValidPawn(this Pawn pawn)
    {
        if (pawn == null) return false;
        if (pawn.Destroyed) return false;
        if (!pawn.Spawned) return false;
        if (pawn.thingIDNumber == 0) return false;
        if (pawn.Name == null) return false;
        if (pawn.Dead) return false;
        if (pawn.RaceProps?.Humanlike != true) return false;
        if (pawn.guest == null) return false;
        if (pawn.guest.HostFaction != null && pawn.guest.HostFaction != Faction.OfPlayer && pawn.Map.ParentFaction != Faction.OfPlayer) return false;
        if (pawn.Faction == null) return false;
        if (pawn.IsPrisonerOfColony || pawn.Faction.IsPlayer) return false;
        if (pawn.IsEntity) return false;
        if (pawn.HostileTo(Faction.OfPlayer)) return false;
        return true;
    }

    public static int RecruitPenalty(this Pawn guest)
    {
        return Mathf.RoundToInt(guest.GetStatValue(statRecruitRelationshipDamage));
    }

    public static int ForcedRecruitPenalty(this Pawn guest)
    {
        return Mathf.RoundToInt(guest.GetStatValue(statForcedRecruitRelationshipDamage));
    }

    public static int GetFriendsInColony(this Pawn guest)
    {
        return guest.GetMapComponent()?.RelationsCache.GetSetFor(guest).friends ?? 0;
    }

    public static int GetFriendsSeniorityInColony(this Pawn guest)
    {
        return guest.GetMapComponent()?.RelationsCache.GetSetFor(guest).friendsSeniority ?? 0;
    }

    public static bool IsRelated(Pawn pawn, Pawn guest)
    {
        // Clear cache
        if (GenTicks.TicksGame >= relatedCacheNextClearTick)
        {
            relatedCache.Clear();
            relatedCacheNextClearTick = GenTicks.TicksGame + GenDate.TicksPerHour * 3;
        }

        if (!relatedCache.TryGetValue(pawn.thingIDNumber, out var isRelated))
        {
            isRelated = guest.relations.RelatedPawns.Any(rel => rel == pawn);
            relatedCache.Add(pawn.thingIDNumber, isRelated);
        }

        return isRelated;
    }

    public static int GetEnemiesInColony(this Pawn guest)
    {
        return guest.GetMapComponent()?.RelationsCache.GetSetFor(guest).enemies ?? 0;
    }

    public static int GetRoyalEnemiesSeniorityInColony(this Pawn guest)
    {
        return guest.GetMapComponent()?.RelationsCache.GetSetFor(guest).enemiesSeniority ?? 0;
    }

    public static int GetMinRecruitOpinion(this Pawn guest)
    {
        var difficulty = guest.RecruitDifficulty();

        return Mathf.RoundToInt(AdjustDifficulty(difficulty));
    }

    private static float RecruitDifficulty(this Pawn guest)
    {
        // From Pawn_GuestTracker.SetGuestStatus, added default value if not set (for mod compatibility)
        var num = guest.kindDef.initialResistanceRange?.Average ?? 20;
        if (guest.royalty != null)
        {
            var mostSeniorTitle = guest.royalty.MostSeniorTitle;
            if (mostSeniorTitle != null)
            {
                num += mostSeniorTitle.def.recruitmentResistanceOffset;
            }
        }

        return num;
    }

    private static float AdjustDifficulty(float difficulty)
    {
        return recruitChanceOpinionCurve.Evaluate(difficulty);
    }

    public static bool ImproveRelationship(this Pawn guest)
    {
        var guestComp = guest.CompGuest();
        return guestComp?.entertain == true;
    }

    public static bool MakeFriends(this Pawn guest)
    {
        var guestComp = guest.CompGuest();
        return guestComp?.makeFriends == true;
    }

    public static bool CanTalkTo(this Pawn talker, Pawn talkee)
    {
        return talker.MapHeld == talkee.MapHeld && InteractionUtility.CanInitiateInteraction(talker) && InteractionUtility.CanReceiveInteraction(talkee) && CanSee(talker, talkee);
    }

    private static bool CanSee(Pawn talker, Pawn talkee)
    {
        var distance = (talker.Position - talkee.Position).LengthHorizontalSquared;
        if (distance <= 2) return true;
        if (distance > 36) return false;
        return GenSight.LineOfSight(talker.Position, talkee.Position, talker.MapHeld, true);
    }

    public static bool IsArrived(this Pawn guest)
    {
        var guestComp = guest.CompGuest();
        if (guestComp == null) return false;
        return guestComp.arrived;
    }

    public static bool ViableGuestTarget(Pawn guest, bool sleepingIsOk = false)
    {
        return guest.IsArrivedGuest(out _) && !guest.Downed && (sleepingIsOk || guest.Awake()) && !guest.HasDismissiveThought() && !IsInTherapy(guest) && !IsTired(guest) && !IsEating(guest) && !CantBeInterrupted(guest);
    }

    private static bool CantBeInterrupted(Pawn guest)
    {
        return guest.CurJob?.def.casualInterruptible == false;
    }

    private static bool IsEating(Pawn guest)
    {
        return guest.CurJobDef == JobDefOf.Ingest;
    }

    public static bool IsTired(this Pawn pawn)
    {
        return pawn?.needs?.rest?.CurCategory >= RestCategory.VeryTired;
    }

    public static void Arrive(this Pawn pawn)
    {
        try
        {
            pawn.PocketHeadgear();
        }
        catch (Exception e)
        {
            Log.Error($"Failed to pocket headgear:\n{e}");
        }

        // Save trader info
        var trader = pawn.mindState?.wantsToTradeWithColony == true;
        var traderKindDef = trader ? pawn.trader.traderKind : null;

        pawn.guest?.SetGuestStatus(Faction.OfPlayer);

        // Restore trader info
        if (trader)
        {
            pawn.mindState.wantsToTradeWithColony = trader;
            PawnComponentsUtility.AddAndRemoveDynamicComponents(pawn);
            pawn.trader.traderKind = traderKindDef;
        }

        pawn.CompGuest()?.Arrive();
    }

    public static bool GetVisitScore(this Pawn pawn, out float score)
    {
        var lord = pawn.GetLord();
        if (lord?.CurLordToil is LordToil_VisitPoint lordToil && pawn.Faction != null)
        {
            score = lordToil.GetVisitScore(pawn);
            return true;
        }

        score = 0;
        return false;
    }

    public static void Leave(this Pawn pawn)
    {
        try
        {
            pawn.WearHeadgear();
        }
        catch (Exception e)
        {
            Log.Error($"Failed to wear headgear:\n{e.Message}");
        }

        pawn.needs.AddOrRemoveNeedsAsAppropriate();

        pawn.guest?.SetGuestStatus(null);

        pawn.CompGuest()?.Leave(false);

        //var reservationManager = pawn.MapHeld.reservationManager;
        //var allReservedThings = reservationManager.AllReservedThings().ToArray();
        //foreach (var t in allReservedThings)
        //{
        //    if (reservationManager.ReservedBy(t, pawn)) reservationManager.Release(t, pawn);
        //}
    }

    private static bool IsInTraderState(this Pawn pawn)
    {
        var compGuest = pawn.CompGuest();
        var lord = compGuest?.lord;
        //if (!pawn.Map.lordManager.lords.Contains(lord)) return false; // invalid lord
        var job = lord?.LordJob;
        return job is LordJob_TradeWithColony;
    }

    public static bool HasDismissiveThought(this Pawn guest)
    {
        return guest.needs.mood.thoughts.memories.Memories.Any(t => t.def.defName == "GuestDismissiveAttitude");
    }

    public static IEnumerable<Pawn> GetAllGuests(Map map)
    {
        return map.GetMapComponent().PresentGuests;
    }

    public static void AddNeedJoy(Pawn pawn)
    {
        if (pawn.needs.joy == null)
        {
            pawn.needs.AddNeed(DefDatabase<NeedDef>.GetNamed("Joy"));
        }

        pawn.needs.joy.CurLevel = Rand.Range(0, 0.5f);
    }

    public static void AddNeedComfort(Pawn pawn)
    {
        if (pawn.needs.comfort == null)
        {
            var addNeed = typeof(Pawn_NeedsTracker).GetMethod("AddNeed", BindingFlags.Instance | BindingFlags.NonPublic);
            addNeed.Invoke(pawn.needs, [DefDatabase<NeedDef>.GetNamed("Comfort")]);
        }

        pawn.needs.comfort.CurLevel = Rand.Range(0, 0.5f);
    }

    public static void FixTimetable(this Pawn pawn)
    {
        pawn.mindState ??= new Pawn_MindState(pawn);
        pawn.timetable = new Pawn_TimetableTracker(pawn) { times = new List<TimeAssignmentDef>(24) };
        for (var i = 0; i < 24; i++)
        {
            var def = TimeAssignmentDefOf.Anything;
            pawn.timetable.times.Add(def);
        }
    }

    public static void FixDrugPolicy(this Pawn pawn)
    {
        //if (pawn.drugs == null) 
        pawn.drugs = new Pawn_DrugPolicyTracker(pawn) { CurrentPolicy = pawn.Map.GetMapComponent().GetDrugPolicy() };
    }

    public static void TryImproveFriendship(this Pawn guest, Pawn recruiter, List<RulePackDef> extraSentencePacks)
    {
        if (!guest.MakeFriends()) return;

        var friendPercentage = GetFriendPercentage(guest);

        // Log.Message($"Recruiting {guest.NameShortColored}: friendPercentage: {friendPercentage} recruiter: {recruiter.NameShortColored}");
        TryPleaseGuest(recruiter, guest, friendPercentage < 1, extraSentencePacks);

        // Notify player if the guest can be recruited now
        if (friendPercentage < 1 && ModSettings_Hospitality.enableRecruitNotification)
        {
            var newFriendPercentage = GetFriendPercentage(guest);
            if (newFriendPercentage >= 1)
            {
                Messages.Message("GuestCanBeRecruitedNow".Translate(new NamedArgument { arg = guest, label = "PAWN" }).AdjustedFor(guest), guest, MessageTypeDefOf.NeutralEvent);
            }
        }
    }

    private static float GetFriendPercentage(Pawn guest)
    {
        var friends = guest.GetFriendsInColony();
        var friendsRequired = FriendsRequired(guest.MapHeld) + guest.GetEnemiesInColony();
        return 1f * friends / friendsRequired;
    }

    public static void Recruit(Pawn guest, int recruitPenalty, bool forced)
    {
        PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDef.Named("RecruitGuest"), KnowledgeAmount.Total);

        if (forced)
            GainThought(guest, ThoughtDef.Named("GuestRecruitmentForced"));

        Find.LetterStack.ReceiveLetter(labelRecruitSuccess, (forced ? txtForcedRecruit : txtRecruitSuccess).Translate(guest), LetterDefOf.PositiveEvent, guest, guest.Faction);

        AngerFactionMembers(guest);
        RecruitingSuccess(guest, recruitPenalty);

        if (guest.HasDeathAcidifier())
        {
            guest.TryTriggerDeathAcidifier();
        }
    }

    private static void RecruitingSuccess(Pawn guest, int recruitPenalty)
    {
        if (guest.Faction != Faction.OfPlayer)
        {
            if (guest.Faction != null)
            {
                guest.Faction.TryAffectGoodwillWith(Faction.OfPlayer, -recruitPenalty, false, true, null, guest);
                if (recruitPenalty >= 1)
                {
                    // TODO: Use Translate instead of Format
                    string message;
                    if (guest.Faction.leader != null && !PawnUtility.IsFactionLeader(guest))
                    {
                        message = Format(txtRecruitFactionAnger, guest.Faction.leader.Name, guest.Faction.Name, guest.Name.ToStringShort, GenText.ToStringByStyle(-recruitPenalty, ToStringStyle.Integer, ToStringNumberSense.Offset));
                        Find.LetterStack.ReceiveLetter(labelRecruitFactionChiefAnger, message, LetterDefOf.NegativeEvent, GlobalTargetInfo.Invalid, guest.Faction);
                    }
                    else
                    {
                        message = Format(txtRecruitFactionAngerLeaderless, guest.Faction.Name, guest.Name.ToStringShort, GenText.ToStringByStyle(-recruitPenalty, ToStringStyle.Integer, ToStringNumberSense.Offset));
                        Find.LetterStack.ReceiveLetter(labelRecruitFactionAnger, message, LetterDefOf.NegativeEvent, GlobalTargetInfo.Invalid, guest.Faction);
                    }
                }
                else if (recruitPenalty <= -1)
                {
                    // TODO: Use Translate instead of Format
                    string message;
                    if (guest.Faction.leader != null && !PawnUtility.IsFactionLeader(guest))
                    {
                        message = Format(txtRecruitFactionPlease, guest.Faction.leader.Name, guest.Faction.Name, guest.Name.ToStringShort, GenText.ToStringByStyle(-recruitPenalty, ToStringStyle.Integer, ToStringNumberSense.Offset));
                        Find.LetterStack.ReceiveLetter(labelRecruitFactionChiefPlease, message, LetterDefOf.PositiveEvent, GlobalTargetInfo.Invalid, guest.Faction);
                    }
                    else
                    {
                        message = Format(txtRecruitFactionPleaseLeaderless, guest.Faction.Name, guest.Name.ToStringShort, GenText.ToStringByStyle(-recruitPenalty, ToStringStyle.Integer, ToStringNumberSense.Offset));
                        Find.LetterStack.ReceiveLetter(labelRecruitFactionPlease, message, LetterDefOf.PositiveEvent, GlobalTargetInfo.Invalid, guest.Faction);
                    }
                }
            }

            guest.Adopt();
        }

        var taleParams = new object[] { guest.MapHeld.mapPawns.FreeColonistsSpawned.RandomElement(), guest };
        TaleRecorder.RecordTale(TaleDef.Named("Recruited"), taleParams);
    }

    public static void Adopt(this Pawn guest)
    {
        guest.TryGetComp<CompGuest>()?.Leave(true);

        // Clear mind
        guest.pather.StopDead();

        // Clear reservations
        Find.Maps.ForEach(m => m.reservationManager.ReleaseAllClaimedBy(guest));

        // Cancel jobs
        if (guest.jobs.jobQueue != null) guest.jobs.jobQueue = new JobQueue();
        guest.jobs.EndCurrentJob(JobCondition.InterruptForced);

        // Reset timetable to default
        guest.timetable = new Pawn_TimetableTracker(guest);

        var lord = guest.GetLord();
        if (lord?.ownedPawns.Count > 1)
        {
            // Inventory
            foreach (var item in guest.inventory.innerContainer.Reverse().ToArray())
            {
                var randomOther = lord.ownedPawns.Where(p => p != guest).RandomElement();
                guest.inventory.innerContainer.TryTransferToContainer(item, randomOther.inventory.innerContainer);
            }

            // Equipment
            for (var i = guest.equipment.AllEquipmentListForReading.Count - 1; i >= 0; i--)
            {
                var item = guest.equipment.AllEquipmentListForReading[i];
                var randomOther = lord.ownedPawns.Where(p => p != guest).RandomElement();
                guest.equipment.TryTransferEquipmentToContainer(item, randomOther.inventory.innerContainer);
            }
        }

        guest.inventory.innerContainer.TryDropAll(guest.Position, guest.MapHeld, ThingPlaceMode.Near);

        guest.ownership.UnclaimBed();
        guest.MapHeld.GetMapComponent().OnGuestAdopted(guest);

        var faction = guest.Faction;
        guest.SetFaction(Faction.OfPlayer);

        if (PawnUtility.IsFactionLeader(guest)) faction.Notify_LeaderLost();

        guest.mindState.exitMapAfterTick = -99999;
        guest.MapHeld.mapPawns.UpdateRegistryForPawn(guest);
        guest.playerSettings.medCare = MedicalCareCategory.Best;
        guest.playerSettings.allowedAreas = new Dictionary<Map, Area>();
        guest.foodRestriction.CurrentFoodPolicy = Current.Game.foodRestrictionDatabase.DefaultFoodRestriction();
        guest.drugs.CurrentPolicy = Current.Game.drugPolicyDatabase.DefaultDrugPolicy();

        guest.caller?.DoCall();
    }

    public static float AdjustPleaseChance(float pleaseChance, Pawn recruiter, Pawn target)
    {
        var opinion = target.relations.OpinionOf(recruiter);
        //Log.Message(String.Format("Opinion of {0} about {1}: {2}", target.NameStringShort,recruiter.NameStringShort, opinion));
        //Log.Message(String.Format("{0} + {1} = {2}", pleaseChance, opinion*0.01f, pleaseChance + opinion*0.01f));
        var difficultyOffset = GetRoyalDifficultyOffset(recruiter, target);
        return pleaseChance * 0.8f + opinion * 0.01f - difficultyOffset;
    }

    private static float GetRoyalDifficultyOffset(Pawn recruiter, Pawn target)
    {
        var title = target.royalty?.MostSeniorTitle;
        if (title == null) return 0f;
        var resistance = title.def.recruitmentResistanceOffset / 200f;

        // If the target has no royal title, the recruiter's royal title does not matter
        var rTitle = recruiter.royalty?.MostSeniorTitle;
        if (rTitle == null) return resistance;
        return resistance - rTitle.def.recruitmentResistanceOffset / 200f;
    }

    private static void GainSocialThought(Pawn initiator, Pawn target, ThoughtDef thoughtDef)
    {
        var impact = initiator.GetStatValue(StatDefOf.SocialImpact);
        var thoughtMemory = (Thought_Memory)ThoughtMaker.MakeThought(thoughtDef);
        thoughtMemory.moodPowerFactor = impact;

        if (thoughtMemory is Thought_MemorySocial thoughtSocialMemory)
        {
            thoughtSocialMemory.opinionOffset *= impact;
        }

        target.needs.mood.thoughts.memories.TryGainMemory(thoughtMemory, initiator);
    }

    public static void ThoughtAboutClaimedBed(this Pawn pawn, Building_GuestBed bed, int moneyBeforeClaiming)
    {
        var thoughtDef = ThoughtDef.Named("GuestClaimedBed");
        if (pawn == null || bed == null) return;

        var free = bed.RentalFee <= 0;
        var score = bed.CalculateBedValue(pawn, moneyBeforeClaiming);
        var stage = free
            ? score switch
            {
                >= 100 => 6, // 5
                >= 50 => 5, // 3
                _ => 0 // 2
            }
            : score switch
            {
                >= 100 => 6, // 5
                >= 50 => 5, // 3
                >= 0 => 4, // 1
                >= -35 => 3, // -2
                >= -60 => 2, // -5
                _ => 1 // -10
            };
        //Log.Message($"{pawn.LabelCap} claimed bed at {bed.Position}. It scored {score:F2} for them.");

        var thoughtMemory = ThoughtMaker.MakeThought(thoughtDef, stage);
        pawn.needs?.mood?.thoughts?.memories?.TryGainMemory(thoughtMemory); // *cough* Extra defensive
    }

    private static void GainThought(this Pawn target, ThoughtDef thoughtDef)
    {
        var thoughtMemory = (Thought_Memory)ThoughtMaker.MakeThought(thoughtDef);
        target?.needs?.mood?.thoughts?.memories?.TryGainMemory(thoughtMemory); // *cough* Extra defensive
    }

    public static bool ShouldMakeFriends(this Pawn pawn, Pawn guest)
    {
        if (!pawn.IsColonist) return false;
        if (!ViableGuestTarget(guest)) return false;
        if (!guest.MakeFriends()) return false;
        if (guest.InMentalState) return false;
        //if (guest.relations.OpinionOf(pawn) >= 100) return false;
        //if (guest.RelativeTrust() < 50) return false;
        if (guest.relations.OpinionOf(pawn) <= -10) return false;
        if (!InteractionUtility.CanInitiateInteraction(pawn)) return false;
        if (!InteractionUtility.CanReceiveInteraction(guest)) return false;
        if (!guest.CanCasuallyInteractNow()) return false;
        if (!JobDriver_GuestBase.JobIsSuspendable(guest)) return false;
        if (!pawn.HasReserved(guest) && !pawn.CanReserveAndReach(guest, PathEndMode.OnCell, pawn.NormalMaxDanger())) return false;

        return true;
    }

    public static bool ShouldImproveRelationship(this Pawn pawn, Pawn guest)
    {
        if (!pawn.IsColonist) return false;
        if (!ViableGuestTarget(guest)) return false;
        if (!guest.ImproveRelationship()) return false;
        //if (guest.Faction.ColonyGoodwill >= 100) return false;
        if (guest.relations.OpinionOf(pawn) >= 100) return false;
        if (guest.InMentalState) return false;
        if (!guest.IsInGuestZone(guest)) return false;
        if (!InteractionUtility.CanInitiateInteraction(pawn)) return false;
        if (!InteractionUtility.CanReceiveInteraction(guest)) return false;
        if (!guest.CanCasuallyInteractNow()) return false;
        if (!JobDriver_GuestBase.JobIsSuspendable(guest)) return false;
        if (!pawn.HasReserved(guest) && !pawn.CanReserveAndReach(guest, PathEndMode.OnCell, pawn.NormalMaxDanger())) return false;

        return true;
    }

    public static void Break(this Pawn pawn)
    {
        if (!pawn.Spawned || pawn.Dead || pawn.Downed || pawn.InMentalState) return;

        pawn.guest?.SetGuestStatus(null);
        var canFlee = pawn.Map.reachability.CanReachMapEdge(pawn.PositionHeld, TraverseParms.For(TraverseMode.NoPassClosedDoors));

        var mentalState = canFlee ? MentalStateDefOf.PanicFlee : MentalStateDefOf.ManhunterPermanent;

        pawn.mindState.mentalStateHandler.TryStartMentalState(mentalState);
    }

    public static Area GetGuestArea(this Pawn p)
    {
        return p?.CompGuest()?.GuestArea;
    }

    public static Area GetShoppingArea(this Pawn p)
    {
        return p?.CompGuest()?.ShoppingArea;
    }

    public static bool Bought(this Pawn pawn, Thing thing)
    {
        var comp = pawn.CompGuest();
        if (comp == null) return false;

        //Log.Message(pawn.NameStringShort+": bought "+thing.Label + "? " + (comp.boughtItems.Contains(thing.thingIDNumber) ? "Yes" : "No"));
        return comp.boughtItems.Contains(thing.thingIDNumber);
    }

    public static bool IsInGuestZone(this Pawn p, Thing s)
    {
        var area = p.GetGuestArea();
        if (area == null) return true;
        return area[s.Position];
    }

    public static bool IsInShoppingZone(this Pawn p, Thing s)
    {
        var area = p.GetShoppingArea();
        if (area == null) return false;
        return area[s.Position];
    }

    public static int FriendsRequired(Map map)
    {
        return map?.GetMapComponent()?.RelationsCache.friendsRequired ?? 0;
    }

    public static int RoyalFriendsSeniorityRequired(Pawn pawn)
    {
        var title = pawn.royalty?.MostSeniorTitle;
        if (title == null) return 100;
        return title.def.seniority + 100; // seniority can be 0!
    }

    public static void EndorseColonists(Pawn recruiter, Pawn guest)
    {
        if (guest.relations == null) return;
        if (recruiter.relations == null) return;

        var isRoyal = guest.royalty?.MostSeniorTitle != null;
        // If guest is royal then only endorse royal pawns
        var pawns = guest.MapHeld.mapPawns.FreeColonistsSpawned.Where(c => c != recruiter && recruiter.relations.OpinionOf(c) > 0 && !(isRoyal && c.royalty?.MostSeniorTitle == null)).ToArray();
        if (pawns.Length == 0) return;

        if (pawns.TryRandomElement(out var target))
        {
            GainSocialThought(target, guest, ThoughtDef.Named("EndorsedByRecruiter"));

            //Log.Message(recruiter.NameStringShort + " endorsed " + target + " to " + guest.Name);
        }
    }

    public static void TryPleaseGuest(Pawn recruiter, Pawn guest, bool focusOnRecruiting, List<RulePackDef> extraSentencePacks)
    {
        // TODO: pawn.records.Increment(RecordDefOf.GuestsCharmAttempts);
        recruiter.skills.Learn(SkillDefOf.Social, 25f);
        var pleaseChance = recruiter.GetStatValue(StatDefOf.NegotiationAbility);
        pleaseChance = AdjustPleaseChance(pleaseChance, recruiter, guest);
        pleaseChance = Mathf.Clamp01(pleaseChance);

        var failedCharms = guest.CompGuest().failedCharms;

        if (Rand.Value > pleaseChance)
        {
            var isAbrasive = recruiter.story.traits.HasTrait(TraitDefOf.Abrasive);
            var multiplier = isAbrasive ? 2 : 1;
            var multiplierText = multiplier > 1 ? " x" + multiplier : Empty;

            if (failedCharms.TryGetValue(recruiter, out var amount))
            {
                amount++;
                failedCharms[recruiter] = amount;
            }
            else
            {
                failedCharms.Add(recruiter, 1);
            }

            if (amount >= 3)
            {
                Messages.Message("RecruitAngerMultiple".Translate(recruiter.Name.ToStringShort, guest.Name.ToStringShort, amount), guest, MessageTypeDefOf.NegativeEvent);
            }

            extraSentencePacks.Add(RulePackDef.Named("Sentence_CharmAttemptRejected"));
            for (var i = 0; i < multiplier; i++)
            {
                GainSocialThought(recruiter, guest, ThoughtDef.Named("GuestOffendedRelationship"));
            }

            MoteMaker.ThrowText((recruiter.DrawPos + guest.DrawPos) / 2f, recruiter.Map, "TextMote_CharmFail".Translate() + multiplierText, 8f);
        }
        else
        {
            failedCharms.Remove(recruiter);

            var statValue = recruiter.GetStatValue(statRecruitEffectivity);
            var floor = Mathf.FloorToInt(statValue);
            var multiplier = floor + (Rand.Value < statValue - floor ? 1 : 0);

            // Multiplier is for what the focus is on
            for (var i = 0; i < multiplier; i++)
            {
                if (focusOnRecruiting)
                    EndorseColonists(recruiter, guest);
                else
                    GainSocialThought(recruiter, guest, ThoughtDef.Named("GuestPleasedRelationship"));
            }

            // And then one more of the other
            multiplier++;
            if (focusOnRecruiting)
                GainSocialThought(recruiter, guest, ThoughtDef.Named("GuestPleasedRelationship"));
            else
                EndorseColonists(recruiter, guest);

            extraSentencePacks.Add(RulePackDef.Named("Sentence_CharmAttemptAccepted"));

            var multiplierText = multiplier > 1 ? " x" + multiplier : Empty;
            MoteMaker.ThrowText((recruiter.DrawPos + guest.DrawPos) / 2f, recruiter.Map, "TextMote_CharmSuccess".Translate() + multiplierText, 8f);
        }

        GainThought(guest, ThoughtDef.Named("GuestDismissiveAttitude"));
    }

    public static void DoAllowedAreaSelectors(Rect rect, Func<Area, string> getLabel, ref Area currentArea, Map map)
    {
        var areas = GetAreas(map).ToArray();
        var num = areas.Length + 1;
        var num2 = rect.width / num;
        Text.WordWrap = false;
        Text.Font = GameFont.Tiny;
        var rect2 = new Rect(rect.x, rect.y, num2, rect.height);
        DoAreaSelector(rect2, null, getLabel, ref currentArea);
        var num3 = 1;
        foreach (var a in areas)
        {
            var num4 = num3 * num2;
            var rect3 = new Rect(rect.x + num4, rect.y, num2, rect.height);
            DoAreaSelector(rect3, a, getLabel, ref currentArea);
            num3++;
        }

        Text.WordWrap = true;
        Text.Font = GameFont.Small;
    }

    public static IEnumerable<Area> GetAreas(Map map)
    {
        return map.areaManager.AllAreas.Where(a => a.AssignableAsAllowed());
    }

    // From RimWorld.AreaAllowedGUI, modified
    private static void DoAreaSelector(Rect rect, Area area, Func<Area, string> getLabel, ref Area currentArea)
    {
        rect = rect.ContractedBy(1f);
        GUI.DrawTexture(rect, area == null ? BaseContent.GreyTex : area.ColorTexture);
        Text.Anchor = TextAnchor.MiddleLeft;
        var text = getLabel(area);
        var rect2 = rect;
        rect2.xMin += 3f;
        rect2.yMin += 2f;
        Widgets.Label(rect2, text);
        if (currentArea == area)
        {
            Widgets.DrawBox(rect, 2);
        }

        if (Mouse.IsOver(rect))
        {
            area?.MarkForDraw();
            if (Input.GetMouseButton(0) && currentArea != area)
            {
                currentArea = area;
                SoundDefOf.Designate_DragStandard_Changed.PlayOneShotOnCamera();
            }
        }

        Text.Anchor = TextAnchor.UpperLeft;
        TooltipHandler.TipRegion(rect, text);
    }

    // Compatibility fix to Therapy mod
    public static bool IsInTherapy(Pawn p)
    {
        return therapyJobDef != null && p.CurJob != null && p.CurJob.def == therapyJobDef;
    }

    public static bool GuestsShouldStayLonger(Lord lord)
    {
        var incapableToLeave = lord.ownedPawns.Where(p => !p.Dead && !p.IsPrisoner && (p.Downed || (p.MentalState != null && p.InMentalState)));
        var dayPercent = GenLocalDate.DayPercent(lord.Map);
        var isDayTime = dayPercent > 0.2f && dayPercent < 0.7f;
        return incapableToLeave.Any() || !isDayTime;
    }

    public static void OnLostEntireGroup(Lord lord)
    {
        // Check if we should get upset
        if (lord?.LordJob is LordJob_VisitColony job && !job.getUpsetWhenLost) return;
        Log.Message($"Lord lost entire group. LordJob = {lord?.LordJob?.GetType().Name}");

        const int penalty = -10;
        //Log.Message("Lost group");
        if (lord?.faction != null)
        {
            //Log.Message("Had lord and faction");
            lord.faction.TryAffectGoodwillWith(Faction.OfPlayer, penalty, false);
            if (lord.faction.leader == null)
            {
                var message = Format(txtLostGroupFactionAngerLeaderless, lord.faction.Name, GenText.ToStringByStyle(penalty, ToStringStyle.Integer, ToStringNumberSense.Offset));
                Find.LetterStack.ReceiveLetter(labelRecruitFactionAnger, message, LetterDefOf.NegativeEvent, GlobalTargetInfo.Invalid, lord.faction);
            }
            else
            {
                var message = Format(txtLostGroupFactionAnger, lord.faction.leader.Name, lord.faction.Name, GenText.ToStringByStyle(penalty, ToStringStyle.Integer, ToStringNumberSense.Offset));
                Find.LetterStack.ReceiveLetter(labelRecruitFactionChiefAnger, message, LetterDefOf.NegativeEvent, GlobalTargetInfo.Invalid, lord.faction);
            }
        }
    }

    private static void AngerFactionMembers(Pawn guest)
    {
        if (guest.Faction == null || guest.Faction.IsPlayer) return;

        var map = guest.MapHeld;
        var allies = map.mapPawns.PawnsInFaction(guest.Faction).ToArray();
        foreach (var ally in allies)
        {
            if (ally != guest && !ally.Dead && ally.Spawned)
            {
                GainThought(ally, ThoughtDef.Named("GuestAngered"));
                GainThought(ally, ThoughtDef.Named("GuestDismissiveAttitude"));
            }
        }
    }

    public static void GiveLordToRoguePawn(Pawn pawn)
    {
        var comp = pawn.CompGuest();
        if (comp == null || !comp.wasDowned || pawn?.jobs == null || pawn.Dead || pawn.Map == null || !pawn.RaceProps.Humanlike || pawn.IsEntity) return; // I don't think this ever happens...

        // Don't use this: Too generic, could conflict with all kinds of behaviors?
        //if(pawn.CurJob?.def == JobDefOf.Goto && pawn.CurJob.exitMapOnArrival)

        // This doesn't work anymore, recovered guests don't get a duty
        if (pawn.mindState.duty?.def == DutyDefOf.ExitMapBestAndDefendSelf) return;

        var lords = pawn.Map.lordManager.lords.Where(lord => lord.CurLordToil is LordToil_VisitPoint && lord.faction == pawn.Faction).ToArray();
        if (lords.Any())
        {
            JoinLord(lords.RandomElement(), pawn);
        }
        else CreateLordForPawn(pawn);

        comp.wasDowned = false;
        pawn.jobs.StopAll();
        pawn.pather.StopDead();
    }

    public static void CheckForRogueGuests(Map map)
    {
        if (ModSettings_Hospitality.disableGuests) return;
        var pawns = map.mapPawns.AllPawnsSpawned.Where(p => !HealthAIUtility.ShouldSeekMedicalRest(p) && !p.health.hediffSet.HasNaturallyHealingInjury())
            .Where(GuestHasNoLord);

        foreach (var pawn in pawns)
        {
            GiveLordToRoguePawn(pawn);
        }
    }

    private static void CreateLordForPawn([NotNull] Pawn pawn)
    {
        Log.Message($"Creating a temporary lord for {pawn.Label} of faction {(pawn.Faction != null ? pawn.Faction.Name : "null")}.");
        var desc = pawn.CompGuest().rescued ? "RescuedPawnBecameGuest" : "DownedPawnBecameGuest";
        Find.LetterStack.ReceiveLetter("LetterLabelPawnBecameGuest".Translate(new NamedArgument { arg = pawn, label = "PAWN" }), desc.Translate(new NamedArgument { arg = pawn, label = "PAWN" }),
            LetterDefOf.NeutralEvent, pawn, pawn.Faction);
        var duration = (int)(Rand.Range(0.5f, 1f) * GenDate.TicksPerDay);
        IncidentWorker_VisitorGroup.CreateLord(pawn.Faction, pawn.Position, [pawn], pawn.Map, false, false, duration);
    }

    public static bool GuestHasNoLord(Pawn pawn)
    {
        if (!IsValidPawn(pawn)) return false;
        if (IsInTraderState(pawn)) return false;

        var comp = pawn.CompGuest();
        if (comp == null) return false;

        var mapLord = pawn.GetLord(); // Expensive :(
        if (mapLord != null && mapLord.Map != pawn.Map) Log.Warning($"{pawn.Name.ToStringFull}'s lord is on a different map!");
        return mapLord == null;
    }

    private static void JoinLord(Lord lord, Pawn pawn)
    {
        if (lord.ownedPawns.Contains(pawn))
        {
            pawn.CompGuest().lord = lord;
            return;
        }

        if (lord.CurLordToil is not LordToil_VisitPoint lordToil) return;
        Log.Message($"{pawn.LabelShort}: Joined lord of faction {lord.faction?.Name}.");

        pawn.Map.GetMapComponent()?.OnGuestJoinedLate(pawn);

        var desc = pawn.CompGuest().rescued ? "RescuedPawnJoinedGroup" : "DownedPawnJoinedGroup";
        Find.LetterStack.ReceiveLetter("LetterLabelPawnJoinedGroup".Translate(new NamedArgument { arg = pawn, label = "PAWN" }), desc.Translate(new NamedArgument { arg = pawn, label = "PAWN" }),
            LetterDefOf.NeutralEvent, pawn, pawn.Faction);
        lordToil.JoinLate(pawn);
    }

    public static void ConvertToTrader(this Pawn pawn, bool actAsIfSpawned)
    {
        pawn.mindState.wantsToTradeWithColony = true;
        PawnComponentsUtility.AddAndRemoveDynamicComponents(pawn, actAsIfSpawned);
        pawn.trader.traderKind = traderKindDef;
    }

    public static bool IsGuestTrader([NotNull] this ITrader trader)
    {
        return trader.TraderKind == traderKindDef;
    }

    public static void OnLordDespawned(Lord lord)
    {
        lord.Map?.GetMapComponent()?.OnLordDespawned(lord);
        MainTabWindowUtility.NotifyAllPawnTables_PawnsChanged();
    }

    public static void OnLordSpawned(Lord lord)
    {
        lord.Map?.GetMapComponent()?.OnLordSpawned(lord);
        MainTabWindowUtility.NotifyAllPawnTables_PawnsChanged();
    }

    public static bool HasDeathAcidifier(this Pawn pawn)
    {
        // Guests with acidifier will trigger it when recruited
        return hediffDeathAcidifier != null && pawn.health.hediffSet.HasHediff(hediffDeathAcidifier);
    }

    public static void TryTriggerDeathAcidifier(this Pawn pawn)
    {
        if (hediffDeathAcidifier != null)
        {
            var acidifier = pawn.health.hediffSet.GetFirstHediffOfDef(hediffDeathAcidifier);
            acidifier.Notify_PawnDied(null);
            if (!pawn.Dead) acidifier.Notify_PawnKilled();
        }
    }

    public static bool WillOnlyJoinByForce(this Pawn pawn)
    {
        // Has acidifier? Then needs force to join, unless has relationship
        return HasDeathAcidifier(pawn) && !RelationUtility.GetRelationInfo(pawn).hasRelationship;
    }

    public static bool MayRecruitRightNow(this Pawn pawn)
    {
        return !pawn.InMentalState && pawn.CompGuest().arrived;
    }

    public static float GetRequiresFoodFactor(Pawn pawn)
    {
        var carriedNutrition = pawn.inventory.innerContainer.Where(thing => JoyGiver_BuyStuff.CanEat(thing, pawn)).Sum(t => RimWorld.FoodUtility.GetNutrition(pawn, t, t.def) * t.stackCount);

        var priority = GenMath.LerpDoubleClamped(2, 4, 1, 0, carriedNutrition);
        //Log.Message($"{pawn.NameShortColored} - wanna buy food: priority = {priority}, carriedNutrition = {carriedNutrition}");
        return priority;
    }

    public static void EnsureHasWorkSettings(Pawn pawn)
    {
        var priorities = _fieldPriorities.GetValue(pawn.workSettings);
        if (priorities == null)
        {
            pawn.workSettings.EnableAndInitialize();
        }
    }
}