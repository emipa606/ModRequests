using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using HarmonyLib;
using UnityEngine;
using System.Reflection;
using Verse.AI;
using AP_PredatorsRework;
using AnimalBehaviours;
using RimWorld.Planet;

namespace AP_PredatorsReworked
{
    [StaticConstructorOnStartup]
    public class AP_PredatorsReworkedHarPatch : Mod
    {
        public static AP_PredatorsSettings settings;
        public AP_PredatorsReworkedHarPatch(ModContentPack content)
          : base(content)
        {
            new Harmony("AP_PredatorsReworkedHarPatch").PatchAll();
            AP_PredatorsReworkedHarPatch.settings = base.GetSettings<AP_PredatorsSettings>();
            AP_PredatorsSettings.ApplySettings();
        }
        public override string SettingsCategory()
        {
            return "[AP] Predators Unleashed";
        }
        public override void DoSettingsWindowContents(Rect inRect)
        {
            AP_PredatorsSettings.DoWindowWithContents(inRect);
        }
    }

    public class AP_PredatorsSettings :ModSettings
    {
        public static bool predHuntMoreColonists = false;
        public static bool disableNotificationOnPredHunting = false;
        public static bool predDebug = false;
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref predHuntMoreColonists, "predHuntMoreColonists", false, true);
            Scribe_Values.Look<bool>(ref predDebug, "predDebug", false, true);
            Scribe_Values.Look<bool>(ref disableNotificationOnPredHunting, "disableNotificationOnPredHunting", false, true);
        }
        public static void DoWindowWithContents(Rect inRect)
        {

            Listing_Standard listing_Standard = new Listing_Standard();
            listing_Standard.Begin(inRect);
            listing_Standard.Gap();
            Text.Font = GameFont.Medium;
            listing_Standard.Label("Hard Mode");
            Text.Font = GameFont.Small;
            listing_Standard.Label("These options are meant to make the game more difficult and challenging.");
            listing_Standard.GapLine();
            listing_Standard.CheckboxLabeled("Predators hunt colonists as likely as animals: ",ref predHuntMoreColonists, "In Vanilla, colonists rarely are targeted by predators, this option reverse that by making them equally likely to be targeted as prey.");
            listing_Standard.Label("Note: The Titanboa will always target colonists slightly more often than other animals.");
            listing_Standard.Gap();
            listing_Standard.CheckboxLabeled("Disable notification letter when any predator hunts a colonist", ref disableNotificationOnPredHunting, "In Vanilla, you get a notification letter as soon as a predator chooses to hunt any colonist. This option prevents you from getting it when turned on.");
            listing_Standard.GapLine();
            listing_Standard.CheckboxLabeled("Enable debug logging: ", ref predDebug, "If you find any errors or bugs, enable this option and share what the console says. It will give me even more detailed information about what happened.");
            listing_Standard.End();
            Rect rect = inRect.BottomPart(0.1f).LeftPart(0.1f);
            Rect rect2 = inRect.BottomPart(0.1f).RightPart(0.1f);
            bool flag = Widgets.ButtonText(rect, "Apply Settings", true, true, true);
            bool flag2 = flag;
            if (flag2)
            {
                ApplySettings();
            }
            bool flag3 = Widgets.ButtonText(rect2, "Reset Settings", true, true, true);
            bool flag4 = flag3;
            if (flag4)
            {
                ResetFactor();
            }
        }
        public static void ApplySettings()
        {
            AP_GetPreyScoreFor_HarPatch.predHuntMoreColonists =predHuntMoreColonists;
            HarmonyPatches.disableNotificationOnPredHunting = disableNotificationOnPredHunting;
            PLog.predDebug = predDebug;
        }
        private static void ResetFactor()
        {
            predHuntMoreColonists = false;
            disableNotificationOnPredHunting = false;
            predDebug = false;
        }
    }

    public static class PredatorUtil
    {
        public static bool ValidDownedPreyToSkipSwallow(Pawn p)
        {
            if (p != null && p.Spawned && (p.Downed || p.Corpse != null))
            {
                return true;
            }
            return false;
        }
        public static bool ValidDownedPrey(Pawn p)
        {
            if (p == null || !p.Spawned || !p.Downed)
            {
                return false;
            }

            return true;
        }
        public static bool ValidCorpse(Pawn p)
        {
            Corpse c = p.Corpse;
            if (c == null || !c.Spawned)
            {
                return false;
            }
            return true;
        }
        public static bool InRangeOfBed(Pawn prey, Pawn predator)
        {
            if (predator.ownership.OwnedBed == null)
            {
                return false;
            }
            if (prey.Position.DistanceTo(predator.ownership.OwnedBed.Position) <= 7.0f)
            {
                return true;
            }
            return false;
        }
        public static void CheckWarnPlayer(Pawn prey, Pawn pawn)
        {
            if (prey.Spawned&&!prey.Downed && prey.Faction == Faction.OfPlayer && Find.TickManager.TicksGame > pawn.mindState.lastPredatorHuntingPlayerNotificationTick + 4000 && prey.Position.InHorDistOf(pawn.Position, 60f))
            {

                if (prey.RaceProps.Humanlike)
                {
                    Find.LetterStack.ReceiveLetter("LetterLabelPredatorHuntingColonist".Translate(pawn.LabelShort, prey.LabelDefinite(), pawn.Named("PREDATOR"), prey.Named("PREY")).CapitalizeFirst(), "LetterPredatorHuntingColonist".Translate(pawn.LabelIndefinite(), prey.LabelDefinite(), pawn.Named("PREDATOR"), prey.Named("PREY")).CapitalizeFirst(), LetterDefOf.ThreatBig, pawn);
                }
                else
                {
                    Messages.Message((prey.Name.Numerical ? "LetterPredatorHuntingColonist" : "MessagePredatorHuntingPlayerAnimal").Translate(pawn.Named("PREDATOR"), prey.Named("PREY")), pawn, MessageTypeDefOf.ThreatBig);
                }
                pawn.mindState.Notify_PredatorHuntingPlayerNotification();
            }
        }
        public static Verb GetSwallowVerb(Pawn boa)
        {
            List<Verb> list = new List<Verb>();
            list.AddRange(boa.VerbTracker.AllVerbs.Where(v => v.tool.power == 10f));
            ; return list.First();

        }
        public static bool HasThreeColonists(Pawn boa)
        {
            int humans = 0;
            Pawn_SwallowWhole boaSwallow2 = (Pawn_SwallowWhole)boa;
            for (int i = 0; i < boaSwallow2.innerContainer.Count; i++)
            {
                if (boaSwallow2.innerContainer[i].def.race.Humanlike)
                {
                    humans++;
                }
            }
            if (humans >= 3)
            {
                return true;
            }
            return false;
        }

        public static void ChangeTextures(Pawn p)
        {
            Pawn_SwallowWhole ps = (Pawn_SwallowWhole)p;
            if (ps.innerContainer.Count >= 1)
            {
                p.kindDef = Defs.AP_TitanboaSwallowForm;
                p.Drawer.renderer.graphics.ResolveAllGraphics();
            }
            else if (ps.innerContainer.Count == 0)
            {
                p.kindDef = Defs.AP_TitanboaBaseForm;
                p.Drawer.renderer.graphics.ResolveAllGraphics();
            }
        }
        public static bool IsPet(Pawn p)
        {
            //unused
            if (p.Faction == null)
            {
                return false;
            }
            else if (p.Faction.IsPlayer)
            {
                Log.Message("is faction player");
                return true;
            }
            return false;

        }
    }

    [HarmonyPatch]
    public static class HarmonyPatches
    {
        public static bool disableNotificationOnPredHunting = false;

        public static MethodInfo myMethodInfo = HarmonyLib.AccessTools.FindIncludingInnerTypes(typeof(JobDriver_PredatorHunt),
            (type) => HarmonyLib.AccessTools.FirstMethod(type, (method) => method.Name.Contains("MakeNewToils")));

        [HarmonyTargetMethod]
        private static IEnumerable<MethodInfo> TargetMethods()
        {
            MethodInfo m = myMethodInfo;
            yield return m;
            m = (MethodInfo)null;
        }

        public static IEnumerable<Toil> Postfix(IEnumerable<Toil> __result, JobDriver_PredatorHunt __instance)
        {
            PLog.M("Predator hunt started.");
            JobDriver_PredatorHunt predDriver = __instance;
            __instance.job.killIncappedTarget = false;
            predDriver.AddFinishAction(() => __instance.pawn.Map.attackTargetsCache.UpdateTarget((IAttackTarget)__instance.pawn));
            //Titanboa Behavior
            if (predDriver.pawn.def.defName == "AP_Titanboa")
            {
                __instance.job.killIncappedTarget = true;
                predDriver.AddFinishAction(
                    delegate
                    {
                        PredatorUtil.ChangeTextures(__instance.pawn);
                    }
                    );
                Pawn targetPrey = (Pawn)predDriver.job.GetTarget(TargetIndex.A);
                Pawn_SwallowWhole boaSwallow = (Pawn_SwallowWhole)__instance.pawn;

                yield return Toils_General.DoAtomic((Action)(() => __instance.pawn.Map.attackTargetsCache.UpdateTarget((IAttackTarget)__instance.pawn)));

                Toil toilSwallow = Toils_Combat.FollowAndMeleeAttack(TargetIndex.A, (Action)(() =>
                {
                    
                    Pawn prey = __instance.Prey;
                    Pawn boa = __instance.pawn;
                    Pawn_SwallowWhole swBoa = (Pawn_SwallowWhole)boa;
                    float curFoodLevel = boa.needs.food.CurLevel;
                    bool hit = __instance.pawn.meleeVerbs.TryMeleeAttack((Thing)prey, PredatorUtil.GetSwallowVerb(boa), true);
                    if (swBoa.innerContainer.Count >= 3)
                    {
                        boa.needs.food.CurLevelPercentage = 1f;
                    }
                    else
                    {
                        boa.needs.food.CurLevel = curFoodLevel;
                    }
                    if (!PredatorUtil.ValidDownedPreyToSkipSwallow(__instance.Prey))
                    {
                        boa.kindDef = Defs.AP_TitanboaSwallowAnimation;
                        boa.Drawer.renderer.graphics.ResolveAllGraphics();
                        predDriver.pawn.Map.attackTargetsCache.UpdateTarget((IAttackTarget)__instance.pawn);
                    }
                    predDriver.ReadyForNextToil();
                }));
                Toil doNothing = Toils_General.Wait(5);
                toilSwallow.initAction = delegate
                {
                    if (disableNotificationOnPredHunting == false)
                    {
                        PredatorUtil.CheckWarnPlayer(__instance.Prey, toilSwallow.actor);
                    }
                };
                yield return toilSwallow;
                yield return Toils_General.Wait(180);
                yield return Toils_Predation.Jitter(__instance, true).JumpIf(() => PredatorUtil.ValidDownedPreyToSkipSwallow(__instance.Prey), doNothing);
                yield return Toils_Predation.Jitter(__instance, false).JumpIf(() => PredatorUtil.ValidDownedPreyToSkipSwallow(__instance.Prey), doNothing);
                yield return Toils_Predation.Jitter(__instance, true).JumpIf(() => PredatorUtil.ValidDownedPreyToSkipSwallow(__instance.Prey), doNothing);
                yield return Toils_Predation.Jitter(__instance, false).JumpIf(() => PredatorUtil.ValidDownedPreyToSkipSwallow(__instance.Prey), doNothing);
                yield return Toils_General.Wait(180).JumpIf(() => PredatorUtil.ValidDownedPreyToSkipSwallow(__instance.Prey), doNothing);
                Toil escape = new Toil();
                escape.initAction = delegate
                {
                    PredatorUtil.ChangeTextures(escape.actor);
                    if (!PredatorUtil.HasThreeColonists(__instance.pawn))
                    {
                        predDriver.ReadyForNextToil();
                    }
                    IntVec3 exit;
                    RCellFinder.TryFindBestExitSpot(__instance.pawn, out exit, TraverseMode.PassAllDestroyableThings);
                    escape.actor.pather.StartPath(exit, PathEndMode.OnCell);
                };
                escape.defaultCompleteMode = ToilCompleteMode.PatherArrival;
                escape.AddPreTickAction(delegate
                {
                    if (!PredatorUtil.HasThreeColonists(__instance.pawn))
                    {
                        predDriver.ReadyForNextToil();
                    }
                    if (__instance.pawn.Map.exitMapGrid.IsExitCell(__instance.pawn.Position))
                    {
                        IncidentParms inc = new IncidentParms();
                        inc.points = 100f;
                        Quest quest = QuestUtility.GenerateQuestAndMakeAvailable(Defs.AP_TitanboaHide, inc.points);
                        if (!quest.hidden && Defs.AP_TitanboaHide.sendAvailableLetter)
                        {
                            QuestUtility.SendLetterQuestAvailable(quest);
                        }
                        __instance.pawn.ExitMap(allowedToJoinOrCreateCaravan: true, CellRect.WholeMap(__instance.pawn.Map).GetClosestEdge(__instance.pawn.Position));
                    }
                });
                Toil escape2 = new Toil();
                escape2.initAction = delegate
                {
                    PredatorUtil.ChangeTextures(escape.actor);
                    if (!PredatorUtil.HasThreeColonists(__instance.pawn))
                    {
                        predDriver.ReadyForNextToil();
                    }
                    if (__instance.pawn.Position.OnEdge(__instance.pawn.Map) || __instance.pawn.Map.exitMapGrid.IsExitCell(__instance.pawn.Position))
                    {
                        IncidentParms inc = new IncidentParms();
                        inc.points = 100f;
                        Quest quest = QuestUtility.GenerateQuestAndMakeAvailable(Defs.AP_TitanboaHide, inc.points);
                        if (!quest.hidden && Defs.AP_TitanboaHide.sendAvailableLetter)
                        {
                            QuestUtility.SendLetterQuestAvailable(quest);
                        }
                        __instance.pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.ManhunterPermanent);
                        __instance.pawn.DeSpawn();
                        Find.WorldPawns.PassToWorld(__instance.pawn, PawnDiscardDecideMode.Decide);
                    }
                };
                yield return escape;
                yield return escape2;
                yield return doNothing;
                yield break;
            }
            else if (predDriver.pawn.Faction == null)
            {
                //Regular predator behavior
                //Toil to finish job
                Toil finishToil = new Toil();
                finishToil.AddPreInitAction(delegate
                {
                    finishToil.defaultCompleteMode = ToilCompleteMode.Instant;
                    __instance.pawn.jobs.EndCurrentJob(JobCondition.Succeeded);
                });
                //Toil #1/
                yield return Toils_General.DoAtomic((Action)(() => __instance.pawn.Map.attackTargetsCache.UpdateTarget((IAttackTarget)__instance.pawn)));
                Toil gotoPawnOrCorpse2 = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
                Toil toil = Toils_Combat.FollowAndMeleeAttack(TargetIndex.A, (Action)(() =>
                {
                    Pawn prey = __instance.Prey;
                    bool surpriseAttack = !prey.IsColonist;
                    
                    if (!__instance.pawn.meleeVerbs.TryMeleeAttack((Thing)prey, __instance.job.verbToUse, surpriseAttack))
                        return;
                    Hediff hiddenCoagulator = (Hediff)HediffMaker.MakeHediff(Defs.AP_HiddenCoagulator, prey, prey.health.hediffSet.GetBrain());
                    hiddenCoagulator.Severity = 0.1f;
                    prey.health.AddHediff(hiddenCoagulator);
                    __instance.pawn.Map.attackTargetsCache.UpdateTarget((IAttackTarget)__instance.pawn);
                }));
                toil.initAction = delegate
                {
                    PLog.M("Toil 1 initiated.");
                    if (disableNotificationOnPredHunting == false)
                    {
                        PLog.M("Notifications enabled. Trying to warn player.");
                        PredatorUtil.CheckWarnPlayer(__instance.Prey, toil.actor);
                    }
                };
                toil.AddPreTickAction(delegate
                {
                    __instance.job.locomotionUrgency = LocomotionUrgency.Walk;
                    Pawn predator = __instance.pawn;
                    Pawn prey = __instance.Prey;
                    if (prey != null && prey.Position.InHorDistOf(predator.Position, 10f) && __instance.job.locomotionUrgency != LocomotionUrgency.Sprint)
                    {
                        __instance.job.locomotionUrgency = LocomotionUrgency.Sprint;
                    }
                    else if (prey != null && prey.Position.InHorDistOf(predator.Position, 20f) && __instance.job.locomotionUrgency != LocomotionUrgency.Jog)
                    {
                        __instance.job.locomotionUrgency = LocomotionUrgency.Jog;
                    }
                    if ((PredatorUtil.ValidDownedPrey(prey) || PredatorUtil.ValidCorpse(prey)))
                    {
                        __instance.ReadyForNextToil();
                    }
                }
                );
                //__instance.pawn.mindState.Notify_PredatorHuntingPlayerNotification();
                //Toil #2
                yield return toil;
                Toil startHauling = Toils_Predation.StartHauling(predDriver);
                //Toil #3
                yield return startHauling;
                Toil gotoPawnOrCorpse = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch);
                //Toil #4
                yield return gotoPawnOrCorpse;
                Toil startCarryThing = Toils_Haul.StartCarryThing(TargetIndex.A);
                Toil jumping = Toils_Jump.JumpIf(gotoPawnOrCorpse2, () => PredatorUtil.InRangeOfBed(predDriver.Prey, predDriver.pawn));
                //Toil #5
                yield return jumping;
                //Toil #6
                yield return startCarryThing;

                Toil carryToCell = Toils_Haul.CarryHauledThingToCell(TargetIndex.B);
                carryToCell.AddPreTickAction(delegate
                {
                    if (PredatorUtil.InRangeOfBed(predDriver.Prey, predDriver.pawn))
                    {
                        predDriver.ReadyForNextToil();
                    }
                });
                carryToCell.JumpIf(() => !PawnUtility.IsCarryingPawn(__instance.pawn) && !PawnUtility.IsCarryingThing(__instance.pawn, __instance.Prey.Corpse), startHauling);
                //Toil #6
                yield return carryToCell;
                //
                Toil placeHauledThingInCell = Toils_Haul.PlaceHauledThingInCell(TargetIndex.B, carryToCell, storageMode: true);
                placeHauledThingInCell.AddPreTickAction(delegate
                {
                    if (PredatorUtil.InRangeOfBed(predDriver.Prey, predDriver.pawn))
                    {
                        predDriver.ReadyForNextToil();
                    }
                });
                //Toil #7
                yield return placeHauledThingInCell;
                //Toil #8
                yield return gotoPawnOrCorpse2;
                //Toil #9
                yield return Toils_Predation.ChewPrey(predDriver);
                //Toil #10
                yield return Toils_Predation.IngestPrey(predDriver);

                //Toil #11
                yield return finishToil;
                yield break;
            }
            else //PETS BEHAVIOR
            {
                __instance.job.killIncappedTarget = true;
                int startTick = Find.TickManager.TicksGame;
                Toil prepareToEatCorpse = new Toil();
                prepareToEatCorpse.initAction = delegate
                {
                    Pawn actor = prepareToEatCorpse.actor;
                    Corpse corpse = __instance.job.GetTarget(TargetIndex.A).Thing as Corpse;
                    if (corpse == null)
                    {
                        Pawn prey2 = __instance.Prey;
                        if (prey2 == null)
                        {
                            actor.jobs.EndCurrentJob(JobCondition.Incompletable);
                            return;
                        }
                        corpse = prey2.Corpse;
                        if (corpse == null || !corpse.Spawned)
                        {
                            actor.jobs.EndCurrentJob(JobCondition.Incompletable);
                            return;
                        }
                    }
                    if (actor.Faction == Faction.OfPlayer)
                    {
                        corpse.SetForbidden(value: false, warnOnFail: false);
                    }
                    else
                    {
                        corpse.SetForbidden(value: true, warnOnFail: false);
                    }
                    actor.CurJob.SetTarget(TargetIndex.A, corpse);
                };
                yield return Toils_General.DoAtomic(delegate
                {
                    __instance.pawn.Map.attackTargetsCache.UpdateTarget(__instance.pawn);
                });
                Action hitAction = delegate
                {
                    Pawn prey = __instance.Prey;
                    if (__instance.pawn.meleeVerbs.TryMeleeAttack(prey, __instance.job.verbToUse, true))
                    {
                        __instance.pawn.Map.attackTargetsCache.UpdateTarget(__instance.pawn);
                    }
                };
                Toil toil = Toils_Combat.FollowAndMeleeAttack(TargetIndex.A, hitAction).JumpIfDespawnedOrNull(TargetIndex.A, prepareToEatCorpse).JumpIf(() => __instance.Prey.Corpse != null, prepareToEatCorpse)
                    .FailOn(() => Find.TickManager.TicksGame > startTick + 5000 && (float)(__instance.job.GetTarget(TargetIndex.A).Cell - __instance.pawn.Position).LengthHorizontalSquared > 4f);
                yield return toil;
                yield return prepareToEatCorpse;
                Toil gotoCorpse = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
                yield return gotoCorpse;
                float durationMultiplier = 1f / __instance.pawn.GetStatValue(StatDefOf.EatingSpeed);
                yield return Toils_Ingest.ChewIngestible(__instance.pawn, durationMultiplier, TargetIndex.A).FailOnDespawnedOrNull(TargetIndex.A).FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
                yield return Toils_Ingest.FinalizeIngest(__instance.pawn, TargetIndex.A);
                yield return Toils_Jump.JumpIf(gotoCorpse, () => __instance.pawn.needs.food.CurLevelPercentage < 0.9f);
            }
        }

    }
}
