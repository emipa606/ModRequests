using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Therapy
{
    public class JobDriver_ReceiveTherapy : JobDriver
    {
        private const float MoodBoostSpeed = 1f / 50000;

        private ThoughtDef thoughtDefRelieved;
        public Thought_Memory CurrentTreatedMemory { get; private set; }
        public int CurrentHealAmount { get; private set; }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.GetTarget(TargetIndex.A), job, 1, -1, null, errorOnFailed);
        }

        public override string GetReport()
        {
            if (CurrentTreatedMemory == null) return base.GetReport();
            return "ReceiveTherapyReport".Translate();
        }

        public override IEnumerable<Toil> MakeNewToils()
        {
            CurrentTreatedMemory = null;
            CurrentHealAmount = 0;
            bool hasCouch = pawn.CurJob.GetTarget(TargetIndex.A).HasThing;
            //Log.Message("has couch? "+hasCouch +": "+pawn.CurJob.GetTarget(TargetIndex.A).Thing.LabelCap);
            if (!hasCouch) yield break;

            KeepLyingDown(TargetIndex.A, true);
            //yield return Toils_Reserve.Reserve(TargetIndex.A, 1, 0);
            yield return GotoCouch(TargetIndex.A);
            Toil wait = Toils_LayDownCouch.LayDown(TargetIndex.A, true); // depends on being therapized?


            //Toil wait = Toils_General.Wait(CurJob.def.joyDuration);
            wait.FailOnCouchNoLongerUsable(TargetIndex.A);
            wait.FailOn(() => pawn.GetRoom() == null); // || TherapistNotActive);
            wait.FailOnTimeTable();

            yield return wait;
        }

        private void KeepLyingDown(TargetIndex index, bool lookForOtherJobs)
        {
            AddFinishAction(delegate {

                //// Forced? Will stay on forever
                //if (pawn.CurJob?.playerForced == true)
                //{
                //    pawn.jobs.jobQueue.EnqueueFirst(new Job(MainUtility.patientJobDef, pawn.CurJob.GetTarget(index)), null);
                //    return;
                //}

                // Stay
                if (!pawn.Drafted && !lookForOtherJobs)
                {
                    pawn.jobs.jobQueue.EnqueueFirst(new Job(MainUtility.patientJobDef, pawn.CurJob.GetTarget(index)));
                }
            });
        }

        public static Toil GotoCouch(TargetIndex couchIndex)
        {
            Toil gotoCouch = new Toil();
            gotoCouch.initAction = delegate {
                Pawn actor = gotoCouch.actor;
                Building_Couch couch = (Building_Couch) actor.CurJob.GetTarget(couchIndex).Thing;
                IntVec3 slotPos = couch.GetRestingSlotPos();
                if (actor.Position == slotPos)
                {
                    actor.jobs.curDriver.ReadyForNextToil();
                    //Log.Message(actor.NameStringShort + ": is at couch");
                }
                else
                {
                    actor.pather.StartPath(couch.GetRestingSlotPos(), PathEndMode.OnCell);
                }
            };
            gotoCouch.tickAction = delegate {
                Pawn actor = gotoCouch.actor;
                Building_Couch couch = (Building_Couch) actor.CurJob.GetTarget(couchIndex).Thing;
                Pawn occupant = couch.CurOccupant;
                if (occupant != null && occupant != actor)
                {
                    actor.pather.StartPath(couch.GetRestingSlotPos(), PathEndMode.OnCell);
                    Log.Message(actor.Name.ToStringShort + ": what is the point of this?");
                }
            };
            //gotoCouch.AddFinishAction(delegate { Log.Message(gotoCouch.actor.NameStringShort + ": arrived at couch"); });

            gotoCouch.defaultCompleteMode = ToilCompleteMode.PatherArrival;
            gotoCouch.FailOnCouchNoLongerUsable(couchIndex);
            return gotoCouch;
        }

        public void HealMemories(int amount)
        {
            if (amount < 0)
            {
                CurrentTreatedMemory = null;
                CurrentHealAmount = 0;
                return;
            }

            if (IsSuitable(CurrentTreatedMemory))
            {
                CurrentHealAmount = amount;
                CurrentTreatedMemory.age += amount;
                MoodBoost(amount);
                return;
            }

            var memories = pawn.needs.mood.thoughts.memories.Memories.Where(IsSuitable).ToArray();
            if (memories.Any())
            {
                CurrentTreatedMemory = memories.MaxBy(m => -m.MoodOffset());
                CurrentHealAmount = amount;
                CurrentTreatedMemory.age += amount;
                MoodBoost(amount);
                return;
                //Log.Message(patient.NameStringShort + " receives therapy: " + amount + " from " + memory.LabelCap);
            }

            // Done
            CurrentTreatedMemory = null;
            pawn.jobs.EndCurrentJob(JobCondition.Succeeded);
        }

        private void MoodBoost(int amount)
        {
            thoughtDefRelieved ??= ThoughtDef.Named("TherapyRelieved");

            var thoughtMemory = (Thought_Memory) ThoughtMaker.MakeThought(thoughtDefRelieved);
            thoughtMemory.moodPowerFactor = 0;
            pawn.needs.mood.thoughts.memories.TryGainMemory(thoughtMemory);

            thoughtMemory = pawn.needs.mood.thoughts.memories.GetFirstMemoryOfDef(thoughtDefRelieved);
            thoughtMemory.moodPowerFactor = Mathf.Clamp01(thoughtMemory.moodPowerFactor + MoodBoostSpeed * amount);
        }

        private static bool IsSuitable(Thought_Memory m)
        {
            return m != null && m.def.IsMemory && m.VisibleInNeedsTab && m.MoodOffset() < 0 && m.def.durationDays > 0;
        }

        public void Dismiss()
        {
            //Log.Message($"Therapy: {pawn.NameShortColored} got dismissed.");
            CurrentTreatedMemory = null;
            CurrentHealAmount = 0;
            pawn.jobs.EndCurrentJob(JobCondition.Succeeded);
        }
    }
}
