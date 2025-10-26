using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld;


namespace HairStyling
{
    class Hairstylering : Building
    {

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

        }

        public void HairStyler(Pawn pawn)
        {
            Find.WindowStack.Add((Window)new HairStyling.Dialog_HairStyling(pawn));
        }
        
        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn myPawn)
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();

            {
                if (!myPawn.CanReserve(this))
                {

                    FloatMenuOption item = new FloatMenuOption("CannotUseReserved".Translate(), null);
                    return new List<FloatMenuOption>
                {
                    item
                };
                }
                if (!myPawn.CanReach(this, PathEndMode.Touch, Danger.Some))
                {
                    FloatMenuOption item2 = new FloatMenuOption("CannotUseNoPath".Translate(), null);
                    return new List<FloatMenuOption>
                {
                    item2
                };

                }

                Action action2 = delegate
                {
                    // IntVec3 InteractionSquare = (this.Position + new IntVec3(0, 0, 1)).RotatedBy(this.Rotation);
                    Job ShallWalkTheValleyOfSquierrls = new Job(DefDatabase<JobDef>.GetNamed("HairStyleChanger"), this, InteractionCell);
                   
                        myPawn.jobs.TryTakeOrderedJob(ShallWalkTheValleyOfSquierrls);
                   

                };
                list.Add(new FloatMenuOption("ClutterStylizeHair".Translate(), action2));
            }
            return list;
        }

    }
}
