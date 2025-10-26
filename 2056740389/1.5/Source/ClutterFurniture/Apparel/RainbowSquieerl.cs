using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld;


namespace ApparelColorChange
{
    class RainbowSquieerl : Building
    {
        private CompPowerTrader comptrader;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
           comptrader = this.TryGetComp<CompPowerTrader>();

        }
           

        public void HairStyler(Pawn pawn)
        {
            Find.WindowStack.Add((Window)new HairStyling.Dialog_HairStyling(pawn));
        }

        public void ColorChanger(Pawn pawn)
        {
            //Dat is where color menu is called
            Find.WindowStack.Add((Window)new Dialog_ColorChanger(pawn));
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


                if (myPawn.apparel.WornApparelCount <= 0)
                {
                    FloatMenuOption item3 = new FloatMenuOption("ClutterImNaked".Translate(), null);
                    return new List<FloatMenuOption>
                {
                    item3
                };
                }
                
                if(comptrader==null || !comptrader.PowerOn)
                {
                    FloatMenuOption item4 = new FloatMenuOption("ClutterFurntureNoPower".Translate(), null);
                    return new List<FloatMenuOption>
                {
                    item4
                };

                }


                Action action1 = delegate
                {
                    // IntVec3 InteractionSquare = (this.Position + new IntVec3(0, 0, 1)).RotatedBy(this.Rotation);
                    Job ShallWalkTheValleyOfSquierrls = new Job(DefDatabase<JobDef>.GetNamed("ClutterColorChanger"), this, InteractionCell);

                    myPawn.jobs.TryTakeOrderedJob(ShallWalkTheValleyOfSquierrls);
                   

                };
                list.Add(new FloatMenuOption("ClutterColorCloths".Translate(), action1));



            }
            return list;
        }

    }
}
