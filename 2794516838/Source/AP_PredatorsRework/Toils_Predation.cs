using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using AP_PredatorsReworked;
using AnimalBehaviours;

namespace AP_PredatorsRework
{
    public class Toils_Predation
    {
        public static Toil StartHauling(JobDriver_PredatorHunt jobinstance)
        {
            Toil startHauling = new Toil();
            Pawn actor = jobinstance.pawn;
            Pawn prey = jobinstance.Prey;
            startHauling.initAction = delegate
            {
                PLog.M("startHauling initiated.");
                if ((!PredatorUtil.ValidCorpse(prey) && jobinstance.Prey == null) || (!actor.CanReserveAndReach(prey, PathEndMode.InteractionCell, Danger.Deadly) && !actor.CanReserveAndReach(prey.Corpse, PathEndMode.ClosestTouch, Danger.Deadly)))
                {
                    if (PredatorUtil.ValidCorpse(prey))
                    {
                        Log.Message("[DEBUG]double check: validcorpse: true");
                    }
                    if (PredatorUtil.ValidDownedPrey(prey))
                    {
                        Log.Message("[DEBUG]double check: validdowned: true");
                    }
                    if (actor.CanReserveAndReach(prey, PathEndMode.InteractionCell, Danger.Deadly))
                    {
                        Log.Message("[DEBUG]double check: can reserve and reach PREY: true");
                    }
                    if (actor.CanReserveAndReach(prey.Corpse, PathEndMode.InteractionCell, Danger.Deadly))
                    {
                        Log.Message("[DEBUG]double check: can reserve and reach CORPSE: true");
                    }
                    PLog.M("StartHauling toil ended with JobCondition Incompletable.");
                    actor.jobs.EndCurrentJob(JobCondition.Incompletable);
                    return;
                }
                //change target from Pawn to corpse
                if (PredatorUtil.ValidCorpse(prey))
                {
                    PLog.M("StartHauling toil: Changed target from Pawn to Corpse.");
                    Corpse corpse = prey.Corpse;
                    actor.CurJob.SetTarget(TargetIndex.A, (Thing)prey.Corpse);
                    actor.Reserve(corpse, jobinstance.pawn.jobs.curJob);

                }
                Building_Bed ownedBed = actor.ownership.OwnedBed;
                if (ownedBed == null)
                {
                    PLog.M("StartHauling toil: No Bed found, making one.");
                    //reachable cell validator
                    Predicate<IntVec3> validator = ((IntVec3 x) => x.Walkable(startHauling.actor.Map) && x.IsValid&&!x.Fogged(startHauling.actor.Map) && startHauling.actor.CanReach(x, PathEndMode.Touch, Danger.Some));
                    //
                    Thing newBed = ThingMaker.MakeThing(Defs.AP_Building_PredatorSleepingSpot);
                    newBed.def.building.claimable = false;
                    newBed.def.building.alwaysDeconstructible = false;
                    IntVec3 edgeCell;
                    CellFinder.TryFindRandomEdgeCellWith(validator,startHauling.actor.Map ,0f,out edgeCell);
                    if(edgeCell.x==0)
                    {
                        edgeCell.x += Rand.Range(1, 3);
                    }
                    else if(edgeCell.x==startHauling.actor.Map.Size.x-1)
                    {
                        edgeCell.x -= Rand.Range(1, 3);
                    }
                    else if(edgeCell.z == 0)
                    {
                        edgeCell.z += Rand.Range(1, 3);
                    }
                    else if(edgeCell.z == startHauling.actor.Map.Size.z - 1)
                    {
                        edgeCell.z -= Rand.Range(1, 3);
                    }
                    bool findReachableCell = CellFinder.TryFindRandomReachableCellNear(edgeCell, actor.Map, 3f, TraverseParms.For(TraverseMode.NoPassClosedDoors), null, null, out edgeCell);
                    if(findReachableCell==false)
                    {
                        PLog.M("StartHauling toil. Couldn't find reachable cell for *edgecell*");
                        startHauling.actor.jobs.EndCurrentJob(JobCondition.Incompletable);
                    }
                    GenSpawn.Spawn(newBed, edgeCell,startHauling.actor.Map);
                    ownedBed = (Building_Bed)newBed;
                    actor.ownership.ClaimBedIfNonMedical(ownedBed);
                }
                //haul to bed
                IntVec3 haulingCell;
                PLog.M("StartHauling toil. Trying to find reachable cell.");
                bool flag = CellFinder.TryFindRandomReachableCellNear(ownedBed.Position, actor.Map, 5f, TraverseParms.For(TraverseMode.NoPassClosedDoors), null, null, out haulingCell);
                if (flag == false)
                {
                    PLog.M("StartHauling toil. Couldn't find reachable cell near bed. Finishing job.");
                    startHauling.actor.jobs.EndCurrentJob(JobCondition.Incompletable);
                    return;
                }
                actor.CurJob.SetTarget(TargetIndex.B, haulingCell);
                actor.CurJob.count = 1;
                jobinstance.pawn.CurJob.haulMode = HaulMode.ToCellNonStorage;
                PLog.M("StartHauling toil ended sucessfully.");
                jobinstance.ReadyForNextToil();
            };
            return startHauling;
        }

        public static Toil ChewPrey(JobDriver_PredatorHunt jobinstance)
        {
            float durationMultiplier = 1f / jobinstance.pawn.GetStatValue(StatDefOf.EatingSpeed);
            Toil ChewPrey = new Toil();
            ChewPrey.initAction = delegate
            {
                Pawn actor = jobinstance.pawn;
                Thing thing = actor.CurJob.GetTarget(TargetIndex.A).Thing;
                Pawn prey = actor.CurJob.GetTarget(TargetIndex.A).Thing as Pawn;
                Corpse corpse = actor.CurJob.GetTarget(TargetIndex.A).Thing as Corpse;
                bool alive = false;
                if (prey == null)
                {
                    if (corpse == null)
                    {
                        jobinstance.pawn.jobs.EndCurrentJob(JobCondition.Incompletable);
                    }
                    else
                    {
                        alive = false;
                    }
                }
                else
                {
                    alive = true;
                }
                if (thing.HasAttachment(ThingDefOf.Fire))
                {
                    jobinstance.pawn.jobs.EndCurrentJob(JobCondition.Incompletable);
                }
                else
                {
                    if (alive == true)
                    {
                        actor.jobs.curDriver.ticksLeftThisToil = 180;
                    }
                    else
                    {
                        actor.jobs.curDriver.ticksLeftThisToil = Mathf.RoundToInt((float)thing.def.ingestible.baseIngestTicks * durationMultiplier);
                    }
                    if (thing != null)
                    {
                        if (alive == true)
                        {
                            actor.Map.physicalInteractionReservationManager.Reserve(actor, actor.CurJob, prey);
                        }
                        else
                        {
                            actor.Map.physicalInteractionReservationManager.Reserve(actor, actor.CurJob, corpse);
                        }
                    }
                }
            };
            ChewPrey.tickAction = delegate
            {
                Pawn actor = jobinstance.pawn;
                if (jobinstance.pawn != actor)
                {
                    actor.rotationTracker.FaceCell(jobinstance.pawn.Position);
                }
                else
                {
                    Thing thing3 = actor.CurJob.GetTarget(TargetIndex.A).Thing;
                    if (thing3 != null && thing3.Spawned)
                    {
                        actor.rotationTracker.FaceCell(thing3.Position);
                    }
                }
            };
            ChewPrey.defaultCompleteMode = ToilCompleteMode.Delay;
            ChewPrey.FailOnDestroyedOrNull(TargetIndex.A);
            ChewPrey.AddFinishAction(delegate
            {
                Pawn actor = jobinstance.pawn;
                if (actor != null && actor.CurJob != null)
                {
                    Thing thing = actor.CurJob.GetTarget(TargetIndex.A).Thing;
                    if (thing != null && actor.Map.physicalInteractionReservationManager.IsReservedBy(actor, thing))
                    {
                        actor.Map.physicalInteractionReservationManager.Release(actor, actor.CurJob, thing);
                    }
                }
            });
            ChewPrey.handlingFacing = true;
            ChewPrey.WithEffect(delegate
            {
                LocalTargetInfo preyTarget = ChewPrey.actor.CurJob.GetTarget(TargetIndex.A);
                if (!preyTarget.HasThing)
                {
                    return null;
                }
                Thing thing = ChewPrey.actor.CurJob.GetTarget(TargetIndex.A).Thing;
                Pawn prey = ChewPrey.actor.CurJob.GetTarget(TargetIndex.A).Thing as Pawn;
                Corpse corpse = ChewPrey.actor.CurJob.GetTarget(TargetIndex.A).Thing as Corpse;
                bool alive = false;
                if (prey == null)
                {
                    if (corpse == null)
                    {
                        jobinstance.pawn.jobs.EndCurrentJob(JobCondition.Incompletable);
                    }
                    else
                    {
                        alive = false;
                    }
                }
                else
                {
                    alive = true;
                }
                EffecterDef result = new EffecterDef();
                if (alive == false)
                {
                    result = preyTarget.Thing.def.ingestible.ingestEffect;
                    if ((int)ChewPrey.actor.RaceProps.intelligence < 1 && preyTarget.Thing.def.ingestible.ingestEffectEat != null)
                    {
                        result = preyTarget.Thing.def.ingestible.ingestEffectEat;
                    }
                }
                else
                {
                    result = preyTarget.Pawn.RaceProps.corpseDef.ingestible.ingestEffect;
                    if ((int)ChewPrey.actor.RaceProps.intelligence < 1 && preyTarget.Pawn.RaceProps.corpseDef.ingestible.ingestEffectEat != null)
                    {
                        result = preyTarget.Pawn.RaceProps.corpseDef.ingestible.ingestEffectEat;
                    }
                }
                return result;
            }, delegate
            {
                if (!ChewPrey.actor.CurJob.GetTarget(TargetIndex.A).HasThing)
                {
                    return null;
                }
                Thing thing = ChewPrey.actor.CurJob.GetTarget(TargetIndex.A).Thing;
                if (ChewPrey.actor != jobinstance.pawn)
                {
                    return ChewPrey.actor;
                }
                return ((LocalTargetInfo)thing);
            });
            ChewPrey.defaultCompleteMode = ToilCompleteMode.Delay;
            return ChewPrey;
        }

        public static Toil IngestPrey(JobDriver_PredatorHunt jobinstance)
        {
            Toil IngestPrey = new Toil();
            IngestPrey.initAction = delegate
            {
                Pawn actor = IngestPrey.actor;
                Job curJob = actor.jobs.curJob;
                Thing thing = curJob.GetTarget(TargetIndex.A).Thing;
                //
                Pawn prey = curJob.GetTarget(TargetIndex.A).Thing as Pawn;
                Corpse corpse = curJob.GetTarget(TargetIndex.A).Thing as Corpse;
                bool alive = false;
                if (prey == null)
                {
                    if (corpse == null)
                    {
                        jobinstance.pawn.jobs.EndCurrentJob(JobCondition.Incompletable);
                    }
                    else
                    {
                        alive = false;
                    }
                }
                else alive = true;
                //

                float num = actor.needs.food.NutritionWanted;
                if (curJob.overeat)
                {
                    num = Mathf.Max(num, 0.75f);
                }
                float num2 = 0f;
                if (alive == false)
                {
                    num2 = thing.Ingested(actor, num);
                }
                else
                {
                    //get some body part
                    BodyPartRecord chosedBodyPart = new BodyPartRecord();
                    IEnumerable<BodyPartRecord> source = from x in prey.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Outside, BodyPartTagDefOf.MovingLimbSegment, null)
                                                         select x;
                    IEnumerable<BodyPartRecord> source2 = from x in prey.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Outside, BodyPartTagDefOf.ManipulationLimbSegment, null)
                                                          select x;

                    if (source.Any())
                    {
                        chosedBodyPart = source.RandomElement();

                    }
                    else if (source2.Any())
                    {
                        chosedBodyPart = source2.RandomElement();
                    }
                    else chosedBodyPart = prey.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Outside, BodyPartTagDefOf.BreathingPathway, null).First();
                    //hurt that part
                    Hediff_MissingPart hediff_MissingPart = (Hediff_MissingPart)HediffMaker.MakeHediff(HediffDefOf.MissingBodyPart, prey, chosedBodyPart);
                    hediff_MissingPart.lastInjury = Defs.AP_Chew;
                    hediff_MissingPart.IsFresh = true;
                    prey.health.AddHediff(hediff_MissingPart);
                    //set num to some value to feed predator
                    num2 = prey.BodySize / 4f;
                }
                if (!actor.Dead)
                {
                    actor.needs.food.CurLevel += num2;
                }
                actor.records.AddTo(RecordDefOf.NutritionEaten, num2);
            };
            IngestPrey.defaultCompleteMode = ToilCompleteMode.Delay;
            return IngestPrey;
        }

        public static Toil Jitter(JobDriver_PredatorHunt jobinstance, bool side)
        {
            Toil jit = new Toil();
            jit.defaultCompleteMode = ToilCompleteMode.Delay;
            jit.initAction = delegate
            {
                var jitter = (JitterHandler)typeof(Pawn_DrawTracker).GetField("jitterer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(jobinstance.pawn.Drawer);
                if (side)
                {
                    jitter.AddOffset(0.2f, (jit.actor.Rotation.FacingCell - jit.actor.Position).AngleFlat);
                }
                else
                {
                    jitter.AddOffset(0.2f, -(jit.actor.Rotation.FacingCell - jit.actor.Position).AngleFlat);

                }
            };
            jit.defaultDuration = 60;
            return jit;
        }

    }
}
