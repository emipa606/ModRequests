using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace Diaper
{

    public enum BathroomDesireCategory : byte
    {
        Going,
        NeedsToGo,
        Fine,
    }

    public enum DiaperDesireCategory : byte
    {
        Trashed,
        Spent,
        Used,
        Clean,
    }

    public class JobGiver_DiaperChange : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            Thing diaper = FindDiaper(pawn);
            if(diaper == null || !pawn.CanReserveAndReach(diaper, PathEndMode.Touch, Danger.Deadly,1,-1,null,false) || !pawn.workSettings.WorkIsActive(WorkTypeDefOf.Doctor))
            {
                return null;
            }
            return new Job(DiaperChangie.DiaperChange, diaper);
        }

        public static Thing FindDiaper(Pawn pawn)
        {
            return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(Diapee.Diaper), PathEndMode.Touch, TraverseParms.For(pawn, Danger.Deadly));
        }

    }

    public class JobGiver_LieDown : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            Thing table = FindTable(pawn);
            if(table == null || !pawn.CanReserveAndReach(table, PathEndMode.OnCell, Danger.Deadly))
            {
                Log.Message("No table found.");
                    return null;
            }
            return new Job(DiaperChangie.GetReadyForChange, table);
        }

        public static Thing FindTable(Pawn pawn)
        {
            if (pawn.CurrentBed() != null)
                return pawn.CurrentBed();
            else if (pawn.Map.spawnedThings.Contains(Diapee.Table))
            {
                return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(Diapee.Table), PathEndMode.OnCell, TraverseParms.For(pawn));
            }
            else
                return null;
        }
    }

    [DefOf]
    public class DiaperChangie
    {
        public static JobDef DiaperChange;
        public static JobDef GetReadyForChange;
        public static JobDef DiaperChangeOther;
        public static WorkGiverDef DiaperChangeOthers;
        public static SoundDef Pee;
        public static SoundDef Poop;
        public static HediffDef DiaperRash;
    }

    [DefOf]
    public class Diapee
    {
        public static ThingDef Diaper;
        public static ThingDef Table;
        public static ThingCategoryDef Diapers;
    }

    public class WorkGiver_DiaperChange : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest
        {
            get
            {
                return ThingRequest.ForGroup(ThingRequestGroup.Pawn);
            }
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Pawn pawn2 = t as Pawn;
            Thing thing = JobGiver_DiaperChange.FindDiaper(pawn2);
            return pawn2 != null && !pawn.WorkTypeIsDisabled(WorkTypeDefOf.Doctor) && pawn2.RaceProps.Humanlike && (pawn2.GetPosture() == PawnPosture.LayingOnGroundNormal || pawn2.InBed()) && pawn != pawn2 && pawn2.needs.TryGetNeed<Need_Diaper>().RashCounter >= 125 && pawn.CanReserve(pawn2, 1, -1, null, forced) && (pawn2.Faction.AllyOrNeutralTo(pawn.Faction) || pawn2.IsPrisonerOfColony) && !pawn.Drafted && pawn.CanReserveAndReach(pawn2, PathEndMode.Touch, Danger.Deadly) && pawn.CanReserveAndReach(thing, PathEndMode.Touch, Danger.Deadly);
        }

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            return pawn.Map.mapPawns.AllPawns;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Pawn pawn2 = t as Pawn;
            Thing thing = JobGiver_DiaperChange.FindDiaper(pawn2);
            Job job = JobMaker.MakeJob(DiaperChangie.DiaperChangeOther, pawn2, thing);
            if (thing != null)
            {
                return job;
            }
            return null;
        }
    }

    public class JobDriver_ReadyForChange : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoThing(TargetIndex.A,PathEndMode.OnCell);
            Toil liedown = new Toil();
            liedown.initAction = delegate
            {
                liedown.actor.jobs.posture = PawnPosture.LayingOnGroundNormal;
                liedown.actor.jobs.curDriver.asleep = false;
            };
            liedown.tickAction = delegate
            {
                if(liedown.actor.needs.TryGetNeed<Need_Diaper>().RashCounter == 0)
                {
                    liedown.actor.jobs.EndCurrentJob(JobCondition.Succeeded);
                }
            };
            liedown.defaultCompleteMode = ToilCompleteMode.Never;
            yield return liedown;
        }
    }

    public class JobDriver_DiaperChange : JobDriver
    {
        Thing thing;
        IntVec3 thingLoc;
        Pawn actor;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if (pawn.CanReserveAndReach(TargetA, PathEndMode.ClosestTouch, Danger.Deadly) && pawn.needs.TryGetNeed<Need_Diaper>().ShowOnNeedList)
            {
                return pawn.Reserve(TargetA, job);
            }
            return false;
        }

        public static void ErrorCheck(Pawn pawn, Thing haulThing)
        {
            if (!haulThing.Spawned)
            {
                Log.Message(string.Concat(new object[]
                {
                    pawn,
                    " tried to start carry",
                    haulThing,
                    " which isn't spawned."
                }));
                pawn.jobs.EndCurrentJob(JobCondition.Incompletable, true);
            }
            if (haulThing.stackCount == 0)
            {
                Log.Message(string.Concat(new object[]
                {
                    pawn,
                    " tried to start carry ",
                    haulThing,
                    " which had stackcount 0."
                }));
                pawn.jobs.EndCurrentJob(JobCondition.Incompletable, true);
            }
            if (pawn.jobs.curJob.count <= 0)
            {
                Log.Error(string.Concat(new object[]
                {
                    "Invalid count: ",
                    pawn.jobs.curJob.count,
                    ", setting to 1. Job was ",
                    pawn.jobs.curJob
                }));
                pawn.jobs.curJob.count = 1;
            }
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);

            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            Toil diaperchange = new Toil();
            diaperchange.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
            diaperchange.FailOnDestroyedOrNull(TargetIndex.A);
            diaperchange.initAction = delegate
            {
                actor = diaperchange.actor;
                thing = TargetThingA;
                thingLoc = thing.Position;
                if (thing.def.defName != "Diaper")
                {
                    actor.jobs.EndCurrentJob(JobCondition.Incompletable, true);
                    Log.Message("You can't change your diaper into something that's not a diaper, what're you doing?");
                }
            };
            diaperchange.PlaySoundAtStart(SoundDef.Named("Tapes"));
            diaperchange.tickAction = delegate
            {
                actor = diaperchange.actor;
                thing = TargetThingA;
                thingLoc = thing.Position;
                if (thing.Position != thingLoc || thing.Destroyed)
                {
                    actor.jobs.EndCurrentJob(JobCondition.Incompletable, true);
                    Log.Message("Where'd the diaper go?");
                }
            };
            diaperchange.WithProgressBar(TargetIndex.A, delegate
            {
                if (thing == null)
                {
                    return 1f;
                }
                return 1f - (float)diaperchange.actor.jobs.curDriver.ticksLeftThisToil / 100;
            }, false, 0f);
            diaperchange.defaultCompleteMode = ToilCompleteMode.Delay;
            diaperchange.defaultDuration = 100;
            diaperchange.AddFinishAction(delegate
            {
                if (diaperchange.actor.jobs.curDriver.ticksLeftThisToil < 1)
                {
                    Need_Diaper changies = actor.needs.TryGetNeed<Need_Diaper>();
                    changies.CurLevel = 1f;
                    changies.RashCounter = 0;
                    thing.stackCount--;
                    if(thing.stackCount <= 0)
                    {
                        thing.Destroy();
                    }
                }
            });
            yield return diaperchange;
        }
    }

    public class JobDriver_DiaperChangeOther : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if (!(pawn.jobs.jobQueue.Count() > 0))
            {
                return pawn.Reserve(TargetB, job) && pawn.Reserve(TargetA, job);
            }
            return false;
        }

        public static void ErrorCheck(Pawn pawn, Thing haulThing)
        {
            if (!haulThing.Spawned)
            {
                Log.Message(string.Concat(new object[]
                {
                    pawn,
                    " tried to start carry",
                    haulThing,
                    " which isn't spawned."
                }));
                pawn.jobs.EndCurrentJob(JobCondition.Incompletable, true);
            }
            if (haulThing.stackCount == 0)
            {
                Log.Message(string.Concat(new object[]
                {
                    pawn,
                    " tried to start carry ",
                    haulThing,
                    " which had stackcount 0."
                }));
                pawn.jobs.EndCurrentJob(JobCondition.Incompletable, true);
            }
            if (pawn.jobs.curJob.count <= 0)
            {
                Log.Error(string.Concat(new object[]
                {
                    "Invalid count: ",
                    pawn.jobs.curJob.count,
                    ", setting to 1. Job was ",
                    pawn.jobs.curJob
                }));
                pawn.jobs.curJob.count = 1;
            }
        }

        public static Toil GrabDiaper(TargetIndex ind, Pawn stinker)
        {
            Toil toil = new Toil();
                toil.initAction = delegate
                {
                    Pawn actor = toil.actor;
                    Job curJob = actor.jobs.curJob;
                    Thing thing = curJob.GetTarget(ind).Thing;
                    actor.carryTracker.TryStartCarry(thing, 1, true);
                    curJob.SetTarget(ind, actor.carryTracker.CarriedThing);
                };
            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            return toil;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.Touch);
            yield return GrabDiaper(TargetIndex.B, TargetA.Pawn);
            Toil goto2 = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            goto2.AddFinishAction(delegate
            {
                pawn.carryTracker.TryDropCarriedThing(TargetLocA, ThingPlaceMode.Near, out Thing pamp);
            });
            yield return goto2; 
            Toil diaperchange = new Toil();
            diaperchange.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
            diaperchange.FailOnDestroyedOrNull(TargetIndex.A);
            diaperchange.PlaySoundAtStart(SoundDef.Named("Tapes"));
            diaperchange.WithProgressBar(TargetIndex.A, delegate
            {
                if (TargetB == null)
                {
                    return 1f;
                }
                return 1f - (float)diaperchange.actor.jobs.curDriver.ticksLeftThisToil / 100;
            }, false, 0f);
            diaperchange.defaultCompleteMode = ToilCompleteMode.Delay;
            diaperchange.defaultDuration = 100;
            diaperchange.AddFinishAction(delegate
            {
                if (diaperchange.actor.jobs.curDriver.ticksLeftThisToil < 1)
                {
                    Need_Diaper changies = TargetA.Pawn.needs.TryGetNeed<Need_Diaper>();
                    changies.CurLevel = 1f;
                    changies.RashCounter = 0;
                    if (TargetB.Thing.stackCount <= 1)
                    {
                        TargetB.Thing.Destroy();
                    }
                    else
                    {
                        TargetB.Thing.stackCount--;
                    }
                }
            });
            yield return diaperchange;
        }
    }

    public class ThoughtWorker_Diaper : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (p.needs.TryGetNeed<Need_Diaper>() == null){
                return ThoughtState.Inactive;
            }
            switch (p.needs.TryGetNeed<Need_Diaper>().CurCategory)
            {
                case DiaperDesireCategory.Clean:
                    return ThoughtState.Inactive;
                case DiaperDesireCategory.Used:
                    if (p.story.traits.HasTrait(TraitDef.Named("Potty_Rebel")))
                    {
                        return ThoughtState.ActiveAtStage(3);
                    }
                    else
                    {
                        return ThoughtState.ActiveAtStage(0);
                    }
                case DiaperDesireCategory.Spent:
                    if (p.story.traits.HasTrait(TraitDef.Named("Potty_Rebel")))
                    {
                        return ThoughtState.ActiveAtStage(4);
                    }
                    else
                    {
                        return ThoughtState.ActiveAtStage(1);
                    }
                case DiaperDesireCategory.Trashed:
                    if (p.story.traits.HasTrait(TraitDef.Named("Potty_Rebel")))
                    {
                        return ThoughtState.ActiveAtStage(5);
                    }
                    else
                    {
                        return ThoughtState.ActiveAtStage(2);
                    }
                default:
                    throw new NotImplementedException();
            }
        }
    }   

    public class ThoughtWorker_Stink : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            bool smell = false;
            if (p.Spawned)
            {
                foreach (Pawn pawn in p.Map.mapPawns.AllPawnsSpawned)
                {
                    if (pawn.needs.TryGetNeed<Need_Diaper>() != null)
                    {
                        if (IntVec3Utility.DistanceTo(p.Position, pawn.Position) <= 10 && pawn.needs.TryGetNeed<Need_Diaper>().CurLevel <= 0.8f)
                        {
                            smell = true;
                        }
                    }
                }
                if (p.story.traits.HasTrait(TraitDef.Named("Potty_Rebel")))
                {
                    if (smell == true)
                        return ThoughtState.ActiveAtStage(1);
                    else
                        return ThoughtState.Inactive;
                }
                else
                {
                    if (smell == true)
                        return ThoughtState.ActiveAtStage(0);
                    else
                        return ThoughtState.Inactive;
                }
            }
            else {
                return ThoughtState.Inactive;
            }
        }
    }

    public class Need_Bladder : Need
    {
        public bool peeing = false;

        private Trait DiaperDependence
        {
            get
            {
                return pawn.story.traits.GetTrait(TraitDef.Named("Incontinent"));
            }
        }
        private Trait DiaperDesire
        {
            get
            {
                return pawn.story.traits.GetTrait(TraitDef.Named("Potty_Rebel"));
            }
        }

        private bool Disabled
        {
            get
            {
                return DiaperDesire == null && DiaperDependence == null;
            }
        }

        public override int GUIChangeArrow
        {
            get
            {
                return -1;
            }
        }

        public BathroomDesireCategory CurCategory
        {
            get
            {
                if (CurLevel > 0.3f)
                {
                    return BathroomDesireCategory.Fine;
                }
                if (CurLevel < 0.05f)
                {
                    return BathroomDesireCategory.Going;
                }
                return BathroomDesireCategory.NeedsToGo;
            }
        }

        public override float CurLevel
        {
            get => base.CurLevel;
            set
            {
                BathroomDesireCategory curCategory = CurCategory;
                base.CurLevel = value;
                if (CurCategory != curCategory)
                {
                    this.CategoryChanged();
                }
            }
        }

        public Need_Bladder(Pawn pawn) : base(pawn)
        {
            threshPercents = new List<float>();
            threshPercents.Add(0.3f);
            threshPercents.Add(0.05f);
        }

        private float BladderFillPerTick
        {
            get
            {
                return def.fallPerDay / 60000f;
            }
        }

        public override void SetInitialLevel()
        {
            base.CurLevelPercentage = Rand.Range(0.35f, 1f);
        }

        public override void NeedInterval()
        {
            if (Disabled)
            {
                this.SetInitialLevel();
                return;
            }
            if (!this.IsFrozen)
            {
                CurLevel -= BladderFillPerTick * 250f;
                if (CurLevel <= 0.05f)
                {
                    peeing = true;
                    SoundStarter.PlayOneShotOnCamera(DiaperChangie.Pee, pawn.Map);
                }
                if (CurLevel >= 0.95f)
                {
                    peeing = false;
                }
                if (peeing == true)
                {
                    CurLevel += 0.20f;
                    pawn.needs.TryGetNeed<Need_Diaper>().CurLevel -= 0.04f;
                }
            }
        }

        private void CategoryChanged()
        {
        }
    }
    public class Need_Bowel : Need
    {
        public bool peeing = false;

        private Trait DiaperDependence
        {
            get
            {
                return pawn.story.traits.GetTrait(TraitDef.Named("Incontinent"));
            }
        }
        private Trait DiaperDesire
        {
            get
            {
                return pawn.story.traits.GetTrait(TraitDef.Named("Potty_Rebel"));
            }
        }

        private bool Disabled
        {
            get
            {
                return DiaperDesire == null && DiaperDependence == null;
            }
        }


        public override int GUIChangeArrow
        {
            get
            {
                return -1;
            }
        }

        public BathroomDesireCategory CurCategory
        {
            get
            {
                if (CurLevel > 0.3f)
                {
                    return BathroomDesireCategory.Fine;
                }
                if (CurLevel < 0.05f)
                {
                    return BathroomDesireCategory.Going;
                }
                return BathroomDesireCategory.NeedsToGo;
            }
        }

        public override float CurLevel
        {
            get => base.CurLevel;
            set
            {
                BathroomDesireCategory curCategory = CurCategory;
                base.CurLevel = value;
                if (CurCategory != curCategory)
                {
                    this.CategoryChanged();
                }
            }
        }

        public Need_Bowel(Pawn pawn) : base(pawn)
        {
            threshPercents = new List<float>();
            threshPercents.Add(0.3f);
            threshPercents.Add(0.05f);
        }

        private float BladderFillPerTick
        {
            get
            {
                return def.fallPerDay / 60000f;
            }
        }

        public override void SetInitialLevel()
        {
            base.CurLevelPercentage = Rand.Range(0.35f, 1f);
        }

        public override void NeedInterval()
        {
            if (Disabled)
            {
                this.SetInitialLevel();
                return;
            }
            if (!this.IsFrozen)
            {
                CurLevel -= BladderFillPerTick * 250f;
                if (CurLevel <= 0.05f)
                {
                    peeing = true;
                    SoundStarter.PlayOneShotOnCamera(DiaperChangie.Poop, pawn.Map);
                }
                if (CurLevel >= 0.95f)
                {
                    peeing = false;
                }
                if (peeing == true)
                {
                    CurLevel += 0.15f;
                    pawn.needs.TryGetNeed<Need_Diaper>().CurLevel -= 0.05f;
                }
            }
        }

        private void CategoryChanged()
        {
        }
    }
    public class Need_Diaper : Need
    {
        public int RashCounter = 0;
        private Trait DiaperDependence
        {
            get
            {
                return pawn.story.traits.GetTrait(TraitDef.Named("Incontinent"));
            }
        }
        private Trait DiaperDesire
        {
            get
            {
                return pawn.story.traits.GetTrait(TraitDef.Named("Potty_Rebel"));
            }
        }

        public override bool ShowOnNeedList
        {
            get
            {
                return !Disabled;
            }
        }

        private bool Disabled
        {
            get
            {
                return DiaperDesire == null && DiaperDependence == null;
            }
        }
        public override int GUIChangeArrow
        {
            get
            {
                return -1;
            }
        }

        public DiaperDesireCategory CurCategory
        {
            get
            {
                if (CurLevel > 0.8f)
                {
                    return DiaperDesireCategory.Clean;
                }
                if (CurLevel > 0.2f && CurLevel < 0.5f)
                {
                    return DiaperDesireCategory.Spent;
                }
                if (CurLevel < 0.2f)
                {
                    return DiaperDesireCategory.Trashed;
                }
                return DiaperDesireCategory.Used;
            }
        }

        public override float CurLevel
        {
            get => base.CurLevel;
            set
            {
                DiaperDesireCategory curCategory = CurCategory;
                base.CurLevel = value;
                if (CurCategory != curCategory)
                {
                    this.CategoryChanged();
                }
            }
        }

        public Need_Diaper(Pawn pawn) : base(pawn)
        {
            threshPercents = new List<float>();
            threshPercents.Add(0.8f);
            threshPercents.Add(0.5f);
            threshPercents.Add(0.2f);
        }

        private float BladderFillPerTick
        {
            get
            {
                return def.fallPerDay / 60000f;
            }
        }

        public override void SetInitialLevel()
        {
            base.CurLevelPercentage = 1f;
        }

        public override void NeedInterval()
        {
            if (Disabled)
            {
                SetInitialLevel();
                return;
            }
            if (!this.IsFrozen)
            {
                CurLevel -= BladderFillPerTick * 150f;
            }
            if(CurLevel < 1f)
            {
                RashCounter++;
            }
            if(RashCounter >= 125)
            {
                if (pawn.needs.TryGetNeed<Need_Bladder>().peeing == false && pawn.needs.TryGetNeed<Need_Bowel>().peeing == false)
                {
                    if (pawn.Map != null && pawn.CurJobDef != DiaperChangie.DiaperChange && pawn.CurJobDef != DiaperChangie.GetReadyForChange && !pawn.InBed() && !(pawn.jobs.jobQueue.Count() > 0))
                    {
                        if (!pawn.Drafted)
                        {
                            if (JobGiver_DiaperChange.FindDiaper(pawn) != null && pawn.workSettings.WorkIsActive(WorkTypeDefOf.Doctor))
                                pawn.jobs.TryTakeOrderedJob_NewTemp(JobMaker.MakeJob(DiaperChangie.DiaperChange, JobGiver_DiaperChange.FindDiaper(pawn)), 0, true);
                            else if (pawn.Map.spawnedThings.Contains(Diapee.Diaper))
                                pawn.jobs.TryTakeOrderedJob_NewTemp(JobMaker.MakeJob(DiaperChangie.GetReadyForChange, JobGiver_LieDown.FindTable(pawn)));
                        }
                    }
                    else if (pawn.IsCaravanMember() && CaravanInventoryUtility.HasThings(pawn.GetCaravan(), Diapee.Diaper, 1))
                    {
                        CaravanInventoryUtility.TryGetThingOfDef(pawn.GetCaravan(), Diapee.Diaper, out Thing stuff, out Pawn uh);
                        stuff.stackCount--;
                        CurLevel = 1f;
                        RashCounter = 0;
                    }
                }
            }
            if(RashCounter >= 250 && !pawn.health.hediffSet.HasHediff(DiaperChangie.DiaperRash))
            {
                pawn.health.AddHediff(DiaperChangie.DiaperRash);
            }
        }

        private void CategoryChanged()
        {
        }
    }
}
