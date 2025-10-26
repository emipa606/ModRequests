using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;



namespace Clutter_Furniture
{
    public class Trashbin : Building
    {
       // private CompGlower glowerComp;
        private CompPowerTrader powerComp;
        //private CompFlickable flickercomp;
        private static SoundDef SoundHiss;

        private float distance = 3.0f;

        // INFO: 60 Ticks = 1s // 1 TickRare = 250 Ticks // 20000 Ticks = 1 day
        private int countdown_max = 7000;
        private int countdown = 0;

        private int countdown_glowerOff_max = 120; // glow for 2s
        private int countdown_glowerOff = 0;

        private int counter_runticks = 0;

        private bool flagWorkDone = true;

        private int activeWorkItem = 0;
        private IEnumerable<Thing> foundThings;
        //private int cCouter;

        /// <summary>
        /// Do something after the object is spawned
        /// </summary>
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            powerComp = GetComp<CompPowerTrader>();
            //glowerComp = GetComp<CompGlower>();
           // flickercomp = GetComp<CompFlickable>();
            
            SoundHiss = SoundDef.Named("PowerOn");

            if (countdown == 0)
            {
                countdown = countdown_max;
            }
            countdown_glowerOff = countdown_glowerOff_max;
            //cCouter = 7000;
            //flickercomp.DoFlick();
           // glowerComp.ReceiveCompSignal("FlickedOff");
            //flickercomp.DoFlick();
            
        }


        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref countdown, "countdown");
        }


        /// <summary>
        /// This is used, when the Ticker is set to Normal
        /// This Tick is done often (60 times per second)
        /// </summary>
        public override void Tick()
        {
            base.Tick();
            //if(glowerComp==null)
            //{
            //    Log.Message("GlowerComp null");
            //    return;
            //}

            //if (cCouter % 500 == 1)
            //{
            //    // Log.Message("LOL1");
            //}
            //cCouter = -1;

            // ============ Do nothing, when the power is off ===============
            // Reset everything, when the power is off
            if (powerComp != null && !powerComp.PowerOn)
            {
                countdown_glowerOff = countdown_glowerOff_max;
                countdown = countdown_max;
                flagWorkDone = true;
                activeWorkItem = 0;
                return;
            }


            // ============ Make a Countdown ===============
            // Timer: Activate, when timer is at zero
            if (countdown > 0)
                countdown -= 1;

            if (countdown <= 0 && flagWorkDone) // Note: always use 'x <= 0' instead of 'x == 0' if possible. Because maybe it overruns because of an error and then? => -1 != 0
            {
                flagWorkDone = false; // activate 
                
            }



            // Now make a countdown for when the glower is lit, so that it can be unlit after x seconds
            if (countdown<=0)
            {
                countdown_glowerOff -= 1;
                if (countdown_glowerOff <= 0)
                {                   
                    countdown_glowerOff = countdown_glowerOff_max;
                    flagWorkDone = true;
                }

                // reset countdown for next run
                countdown = countdown_max;
            }

            // Do the next part only, when the countdown fires:
            if (flagWorkDone)
            {
                counter_runticks = 0;
                activeWorkItem = 0;
                return;
            }

            // ============ Do work, when the countdown fires ================

            // activate the glower
           
            // increase runticks counter
            counter_runticks += 1;
            // do cleaning only every 5 ticks
            if (Math.IEEERemainder((double)counter_runticks, (double)20.0f) != 0)
                return;

            // This is a possible value to get the reach from the xml: use the glower range
            distance = 6;

            // Get the filth in reach and save it in an item collection (IEnumerable<Thing>)
            if (activeWorkItem <= 0)
            {
                foundThings = FindItemsInRoomAndWithinDistance(this, distance, ThingCategory.Filth);
                if (foundThings == null)
                    activeWorkItem = 0;
                else
                    activeWorkItem = foundThings.Count();
            }

            //// This is to create a debug output of the found items
            //StringBuilder strb = new StringBuilder();
            //foreach (Thing thing in foundThings)
            //{
            //    strb.Append(thing.Label);
            //    strb.Append(" - Category: ");
            //    strb.Append(thing.def.category.ToString());
            //    strb.AppendLine();
            //}
            //if (strb.ToString() != string.Empty)
            //    Log.Error(strb.ToString());


            // Check if we are at zero, then there is nothing more to do
            if (activeWorkItem <= 0 || foundThings == null || foundThings.Count() == 0)
            {
                flagWorkDone = true;
                return;
            }

            // Now we need to work with all filth-items that are found
            Thing thing0 = null;

            if (activeWorkItem <= foundThings.Count())
                thing0 = foundThings.ElementAt(activeWorkItem - 1);
            activeWorkItem -= 1;

            if (thing0 != null)
                thing0.Destroy();

        }





        /// <summary>
        /// Find defined things in the room and within distance and return an IEnumerable.
        /// </summary>
        /// <param name="position">The source position.</param>
        /// <param name="distance">The max. distance from the position.</param>
        /// <param name="category">The EntityCategory to search for.</param>
        /// <returns></returns>
        /// 

        private IEnumerable<Thing> FindItemsInRoomAndWithinDistance(Thing thing, float distance, ThingCategory category)
        {
            // Find the room at the position
            Room room = thing.Position.GetRoom(this.Map);
            if (room == null)
            {
                IEnumerable<IntVec3> posList = GenAdj.CellsAdjacent8Way(thing);
                foreach (IntVec3 pos in posList)
                {
                    if (pos.GetRoom(this.Map) == null)
                        continue;

                    room = pos.GetRoom(this.Map);
                    break;
                }
            }


            // LINQ => Compare: room // distance // category
            return this.Map.listerThings.AllThings.Where(t => t.def.category == category &&
                                                          room == t.Position.GetRoom(this.Map) &&
                                                          t.Position.InHorDistOf(thing.Position, distance));
        }


        /// <summary>
        /// This string will be shown when the object is selected (focus)
        /// </summary>
        /// <returns></returns>
        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.GetInspectString());

            //// Here I use it as an ingame counter display
            stringBuilder.AppendLine();
            stringBuilder.Append("ClutterStringCounter".Translate());
            stringBuilder.Append(countdown.ToString());
            
            return stringBuilder.ToString();
        }

    }
}