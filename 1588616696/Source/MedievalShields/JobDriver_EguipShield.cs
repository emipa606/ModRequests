﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using Verse.Sound;


namespace Shield
{

}/*
    class JobDriver_EguipShield: JobDriver
    {

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            //Reservation logic when dealing with queues.
            return this.pawn.Reserve(this.job.targetA, this.job, 1, -1, null);
        }

        [DebuggerHidden]
        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Reserve.Reserve(TargetIndex.A, 1, -1, null);
            yield return new Toil
            {
                initAction = delegate
                {
                    Pawn owner = this.pawn;

                    if (PawnPickedUpAShield(newEq))
                    {
                        // picked up a shield
                        if (PawnHasShieldEquiped(owner) || PawnHasShieldInInvnetory(owner))
                        {
                            // has a shield already 
                            if (PawnHasShieldEquiped(owner))
                            {
                                // rimworlds apparel system takes care of this we just exit
                                // return;
                            }
                            if (PawnHasShieldInInvnetory(owner))
                            {
                                foreach (Thing a in owner.inventory.innerContainer)
                                {
                                    foreach (ThingCategoryDef tgd in a.def.thingCategories)
                                    {
                                        // we have a shield in the inventory
                                        if (tgd.defName == "Shield")
                                        {
                                            // drop the shield in inventory transfer the one we just picked up.
                                            owner.inventory.innerContainer.TryDrop(a, ThingPlaceMode.Direct, out Thing thing, null, null);
                                            owner.inventory.innerContainer.TryAddOrTransfer(newEq as Thing);
                                        }
                                    }
                                }
                            }
                        }
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
        }
    }
}

/*
 * 
 ThingWithComps thingWithComps = (ThingWithComps)this.job.targetA.Thing;
                    ThingWithComps thingWithComps2;
                    if (thingWithComps.def.stackLimit > 1 && thingWithComps.stackCount > 1)
                    {
                        thingWithComps2 = (ThingWithComps)thingWithComps.SplitOff(1);
                    }
                    else
                    {
                        thingWithComps2 = thingWithComps;
                        thingWithComps2.DeSpawn();
                    }
                    //this.pawn.equipment.MakeRoomFor(thingWithComps2);
                    //this.pawn.equipment.AddEquipment(thingWithComps2);
                    bool success = this.pawn.inventory.innerContainer.TryAdd(thingWithComps2);
                    if (thingWithComps.def.soundInteract != null)
                    {
                        thingWithComps.def.soundInteract.PlayOneShot(new TargetInfo(this.pawn.Position, this.pawn.Map, false));
                    }
                    if (success)
                    {
                        GoldfishModule pawnMemory = GoldfishModule.GetGoldfishForPawn(this.pawn);
                        if (pawnMemory == null)
                            return;
                        pawnMemory.AddSidearm(thingWithComps2.def);
                    }
 * 
 */

