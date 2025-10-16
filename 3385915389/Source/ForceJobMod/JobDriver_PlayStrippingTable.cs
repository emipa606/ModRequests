using System.Collections.Generic;
using Verse;
using Verse.AI;
using RimWorld;
using UnityEngine;
using Verse.Sound;

namespace ForceJobMod
{
    public class JobDriver_PlayStrippingTable : JobDriver
    {
        private const int ShotDuration = 600;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, job.def.joyMaxParticipants, 0, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.EndOnDespawnedOrNull(TargetIndex.A);

            // Cast the target building to the custom class
            Building_StrippingTableTable table = TargetA.Thing as Building_StrippingTableTable;

            Toil chooseCell = Toils_Misc.FindRandomAdjacentReachableCell(TargetIndex.A, TargetIndex.B);
            yield return chooseCell;
            yield return Toils_Reserve.Reserve(TargetIndex.B);
            yield return Toils_Goto.GotoCell(TargetIndex.B, PathEndMode.OnCell);

            Toil toil = new Toil();
            toil.initAction = delegate
            {
                job.locomotionUrgency = LocomotionUrgency.Walk;

                // Set the table's isInUse flag to true
                if (table != null)
                {
                    table.isInUse = true;
                    

                }
            };
            toil.tickAction = delegate
            {
                pawn.rotationTracker.FaceCell(base.TargetA.Thing.OccupiedRect().ClosestCellTo(pawn.Position));
                if (ticksLeftThisToil == 300)
                {
                    // Play the billiards sound
                    SoundDefOf.PlayBilliards.PlayOneShot(new TargetInfo(pawn.Position, pawn.Map));

                    // Define a list of potential messages
                    List<string> messages = new List<string>
                    {
                    "Why is the pole so sticky?",
                    "My thighs hurt.",
                    "Smiling through the daddy issues.",
                    "This pole has seen things.",
                    "Do we have a therapist in our colony?",
                    "Step 1: Pole. Step 2: Therapy.",
                    "I'll sue OnlyPawns if they leak my messages.",
                    "Please don't break my leg, T.",
                    "I was told this would be empowering.",
                    "Whose sweat is this?"
                    };

                    // Choose a random message from the list
                    string randomMessage = messages.RandomElement();

                    // Show the random mote text
                    MoteMaker.ThrowText(pawn.Position.ToVector3(), pawn.Map, randomMessage, Color.green);
                }

                if (Find.TickManager.TicksGame > startTick + job.def.joyDuration)
                {
                    if (table != null)
                    {
                        table.isInUse = false;
                        
                    }
                    EndJobWith(JobCondition.Succeeded);
                }

                else
                {
                    JoyUtility.JoyTickCheckEnd(pawn, JoyTickFullJoyAction.EndJob, 1f, (Building)base.TargetThingA);
                }
            };
            toil.handlingFacing = true;
            toil.socialMode = RandomSocialMode.SuperActive;
            toil.defaultCompleteMode = ToilCompleteMode.Delay;
            toil.defaultDuration = 600;

            toil.AddFinishAction(delegate
            {
                if (table != null)
                {
                    // Adding a short delay before setting the flag to false
                    LongEventHandler.QueueLongEvent(() =>
                    {
                        if (table != null)
                        {
                            table.isInUse = false;
                            
                        }
                    }, "SettingTableInUseFalse", false, null);
                }

                DropRandomGearOrApparel(pawn);
                JoyUtility.TryGainRecRoomThought(pawn);
            });


            yield return toil;

            yield return Toils_Reserve.Release(TargetIndex.B);
            yield return Toils_Jump.Jump(chooseCell);
        }

        public override object[] TaleParameters()
        {
            return new object[2]
            {
                pawn,
                base.TargetA.Thing.def
            };
        }

        private void DropRandomGearOrApparel(Pawn pawn)
        {
            if (pawn.equipment != null && pawn.equipment.HasAnything())
            {
                ThingWithComps equipment = pawn.equipment.AllEquipmentListForReading.RandomElement();
                pawn.equipment.Remove(equipment);
                GenPlace.TryPlaceThing(equipment, pawn.Position, pawn.Map, ThingPlaceMode.Near);
            }
            else if (pawn.apparel != null && pawn.apparel.WornApparel.Any())
            {
                Apparel apparel = pawn.apparel.WornApparel.RandomElement();
                pawn.apparel.Remove(apparel);
                GenPlace.TryPlaceThing(apparel, pawn.Position, pawn.Map, ThingPlaceMode.Near);
            }
        }
    }
}