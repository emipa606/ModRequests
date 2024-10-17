using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace Metro
{
    public class JobDriver_EntangleTargetWithCocoon : JobDriver_Mutant
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(this.TargetA, job);
        }

        protected Thing Takee
        {
            get
            {
                return this.job.GetTarget(TargetIndex.A).Thing;
            }
        }

        public override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(TargetIndex.A);
            this.FailOnAggroMentalState(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.OnCell);
            yield return Toils_General.Wait(100, TargetIndex.A);
            yield return Toils_Haul.StartCarryThing(TargetIndex.A, false, false, false);
            yield return new Toil
            {
                initAction = delegate ()
                {
                    this.pawn.jobs.curJob.count = 1;
                    var cocoon = (Cocoon)ThingMaker.MakeThing(MetroDefOf.Metro_Cocoon);
                    GenSpawn.Spawn(cocoon, this.pawn.Position, this.pawn.Map);
                    cocoon.TryAcceptThing(Takee);
                    var spotCandidates = this.pawn.GetHiveCells().Where(x => x.Walkable(this.pawn.Map) && x.GetFirstThing(pawn.Map, MetroDefOf.Metro_Cocoon) == null);

                    if (spotCandidates.Count() > 0)
                    {
                        this.pawn.jobs.curJob.targetA = cocoon;
                        this.pawn.jobs.curJob.targetB = spotCandidates.RandomElement();
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
            yield return Toils_Haul.StartCarryThing(TargetIndex.A, false, false, false);
            yield return Toils_Goto.Goto(TargetIndex.B, PathEndMode.OnCell);
            yield return PlaceCarriedThingInCell(TargetIndex.B);
            yield break;
        }

        public Toil PlaceCarriedThingInCell(TargetIndex facingTargetInd)
        {
            Toil toil = new Toil();
            toil.initAction = delegate
            {
                Pawn actor = toil.actor;
                if (actor.carryTracker.CarriedThing == null)
                {
                    Log.Error(string.Concat(actor, " tried to place hauled thing in facing cell but is not hauling anything."));
                }
                else
                {
                    IntVec3 dropLoc = actor.Position;
                    if (!actor.carryTracker.TryDropCarriedThing(dropLoc, ThingPlaceMode.Direct, out Thing _))
                    {
                        actor.jobs.EndCurrentJob(JobCondition.Incompletable);
                    }
                }
            };
            return toil;
        }
    }
}