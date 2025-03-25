using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace SimpleSlaveryCollars.Jobs
{
    internal class JobDriver_SetSlaveCollar : JobDriver
    {
        const int enslaveDuration = 300;

        private Pawn Victim
        {
            get
            {
                return (Pawn)this.job.GetTarget(TargetIndex.A).Thing;
            }
        }

        private Apparel SlaveCollar
        {
            get
            {
                return (Apparel)this.job.GetTarget(TargetIndex.B).Thing;
            }
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(Victim, job, 1, -1, null) && pawn.Reserve(SlaveCollar, job, 1, -1, null);
        }

        //[DebuggerHidden]
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            this.FailOnDestroyedOrNull(TargetIndex.B);
            this.FailOnForbidden(TargetIndex.B);
            yield return Toils_Reserve.Reserve(TargetIndex.A);
            yield return Toils_Reserve.Reserve(TargetIndex.B);
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.Touch);
            yield return new Toil
            {
                initAction = delegate
                {
                    pawn.jobs.curJob.count = 1;
                }
            };
            yield return Toils_Haul.StartCarryThing(TargetIndex.B);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            yield return Toils_General.WaitWith(TargetIndex.A, enslaveDuration, true);
            yield return new Toil
            {
                initAction = delegate
                {
                    Thing slaveCollar = null;
                    pawn.carryTracker.TryDropCarriedThing(pawn.PositionHeld, ThingPlaceMode.Direct, out slaveCollar, null);
                    if (slaveCollar != null)
                    {
                        var collar = (Apparel)slaveCollar;
                        // Do enslave attempt
                        bool success = true;

                        if (!Victim.jobs.curDriver.asleep &&
                            !Victim.story.traits.HasTrait(TraitDef.Named("Wimp")) &&
                            !Victim.InMentalState &&
                            !Victim.Downed
                        )
                        {
                            if (Victim.story.traits.HasTrait(TraitDefOf.Nerves) &&
                                (Victim.story.traits.GetTrait(TraitDefOf.Nerves).Degree == -2 && Rand.Value > 0.66f) ||
                                                                Victim.needs.mood.CurInstantLevelPercentage < Rand.Range(0f, 0.33f)
                                                        )
                                success = false;
                        }
                        if (success)
                        {
                            SlaveUtility.GiveSlaveCollar(Victim, collar);
                            Messages.Message("TargetSetSlaveCollar".Translate(pawn.Name.ToStringShort, Victim.Name.ToStringShort), MessageTypeDefOf.PositiveEvent); //Z- NameStringShort -> Name.ToStringShort
                            AddEndCondition(() => JobCondition.Succeeded);
                        }
                        else
                        {
                            Victim.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Berserk, "ReasonFailedSetSlaveCollar".Translate(pawn.Name.ToStringShort, Victim.Name.ToStringShort)); //Z- NameStringShort -> Name.ToStringShort
                            AddEndCondition(() => JobCondition.Incompletable);
                        }
                    }
                    else
                        AddEndCondition(() => JobCondition.Incompletable);
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
        }
    }
}
