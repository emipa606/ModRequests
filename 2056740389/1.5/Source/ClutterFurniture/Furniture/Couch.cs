using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Noise;
using Verse.Sound;
using RimWorld;
using System;

namespace Clutter_Furniture
{
    class Couch : Building
    {
        public int LeftSeat = 0;
        public int RightSeat = 0;
        private Thing LeftPart = null;
        private Thing RightPart = null;



        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref LeftSeat, "LeftSeat");
            Scribe_Values.Look<int>(ref RightSeat, "RightSeat");
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (LeftSeat != 0)
            {
                PapersPlease(LeftSeat);
            }
            if (RightSeat != 0)
            {
                PapersPlease(RightSeat);
            }

        }

        public Thing PapersPlease(int papers)
        {
            return this.Map.listerThings.AllThings.Where<Thing>(s => s.thingIDNumber == papers && s.def.defName == "ClutterVoidSeat").FirstOrDefault();
        }

        public bool GroundCheck(List<IntVec3> LocList)
        {
            bool check = true;
            foreach (IntVec3 loc in LocList)
            {
                Thing thing = this.Map.thingGrid.ThingAt(loc, ThingDef.Named("ClutterVoidSeat"));
                if (thing != null)
                {
                    //AddReaplly
                    if (loc == LocList.FirstOrDefault())
                    {
                        LeftPart = thing;
                    }
                    else
                    {
                        RightPart = thing;
                    }
                    check = false;

                }
            }

            return check;
        }

        public void Spawner()
        {
            List<IntVec3> LocList = this.OccupiedRect().Cells.ToList();
            if (LocList.Count > 0)
            {
                if (GroundCheck(LocList))
                {
                    foreach (IntVec3 loc in LocList)
                    {
                        Thing thingy = ThingMaker.MakeThing(ThingDef.Named("ClutterVoidSeat"));
                        GenSpawn.Spawn(thingy, loc,this.Map);
                        thingy.Rotation = Rotation;
                        //  Log.Message("Spawned: " + thingy.def.defName + thingy.thingIDNumber);
                        if (LeftPart == null)
                        {

                            LeftPart = thingy;
                            //  Log.Message("Left added " + thingy.def.defName + thingy.thingIDNumber);

                        }
                        else if (RightPart == null)
                        {
                            RightPart = thingy;
                            //  Log.Message("Right added " + thingy.def.defName + thingy.thingIDNumber);
                        }
                    }
                }
            }
        }

        public override void Tick()
        {
            base.Tick();
            if (this.Spawned)
            {
                if (LeftPart == null && RightPart == null)
                {
                    Spawner();
                }
            }
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            base.DeSpawn();
            if (RightPart != null && this.RightPart.Spawned && !RightPart.Destroyed)
            {
                RightPart.Destroy();
                RightPart = null;
                //  Log.Message("Destroying: " + RightPart.def.defName + RightPart.thingIDNumber);
            }
            if (LeftPart != null && this.LeftPart.Spawned && !LeftPart.Destroyed)
            {
                LeftPart.Destroy();
                LeftPart = null;
                // Log.Message("Destroying: " + LeftPart.def.defName + LeftPart.thingIDNumber);
            }
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            base.Destroy(mode);
            if (RightPart != null && RightPart.Spawned && !RightPart.Destroyed)
            {
                RightPart.Destroy();
                RightPart = null;
            }
            if (LeftPart != null && LeftPart.Spawned && !LeftPart.Destroyed)
            {
                LeftPart.Destroy();
                LeftPart = null;
            }
        }


    }
}