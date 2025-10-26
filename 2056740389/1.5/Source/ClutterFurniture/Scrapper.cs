using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace ClutterFurniture
{
    class Scrapper : Building
    {
        public static int scrapingTime =1000;
        private int CheckTimer = 100;
        public CompPowerTrader compPowah;
        private Thing thingToSpawn=null;
        private string thingtospawnID;
        private int thingtospawnHealth = 0;
        private string thingtospawnStuff;
        private IntVec3 IntCell = IntVec3.South;
        private int textcounter = 0;
        private string idletext;
        public List<string> EmptyTxt = new List<string>
		{
            "Initialize rust",
            "Rerouting all power to blinking lights",
            "Generating dust layers",
            "Getting old"
            			
		};  

        public void IdleTextChooser()
        {
            idletext = EmptyTxt.RandomElement();
        }
      
                             
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref CheckTimer, "CheckTimer");
            Scribe_Values.Look<string>(ref thingtospawnID, "thingtospawnID");
            Scribe_Values.Look<string>(ref thingtospawnStuff, "thingtospawnStuff");
            Scribe_Values.Look<int>(ref thingtospawnHealth, "thingtospawnHealth");
           
        }
        public Thing FindHopper()
        {
            List<IntVec3> locList = new List<IntVec3>();
            locList = GenAdjFast.AdjacentCells8Way(this.Position, this.Rotation, this.RotatedSize);
            Thing thing2 = null;
            if (this.Map.thingGrid.ThingsAt(this.IntCell).Where<Thing>(s => s.def == ThingDef.Named("OldStorageRack")).FirstOrDefault() != null)
            {
                thing2 = this.Map.thingGrid.ThingsAt(this.IntCell).Where<Thing>(s => s.def == ThingDef.Named("OldStorageRack")).FirstOrDefault();
               // Log.Message("StorageFound");
                return thing2;

            }
            //if (locList.Count > 0)
            //{
            //    Log.Message("count=" + locList.Count());
            //    foreach (IntVec3 x in locList)
            //    {

            //        Log.Message("lol1");
            //        foreach (Thing z in x.GetThingList())
            //        {
            //            Log.Message("defnames" + z.def.defName);
            //        }
            //        if (Find.ThingGrid.ThingsAt(x).Where<Thing>(s => s.def == ThingDef.Named("OldStorageRack")).FirstOrDefault() != null)
            //        {
            //            thing2 = Find.ThingGrid.ThingsAt(x).Where<Thing>(s => s.def == ThingDef.Named("OldStorageRack")).FirstOrDefault();
            //            Log.Message("StorageFound");
            //            return thing2;

            //        }
            //    }
            //}
            return thing2;
        }

        public Thing FindThing()
        {


            Thing thing2 = FindHopper();
                        
                Thing thing = null; 
                if(thing2!=null)
                {
                    foreach(IntVec3 t in thing2.OccupiedRect().Cells)
                    {
                        if(t.GetThingList(this.Map).Where<Thing>(s => s.def.IsApparel).FirstOrDefault()!=null)
                        {
                            thing = t.GetThingList(this.Map).Where<Thing>(s => s.def.IsApparel).FirstOrDefault();
                            break;
                        }
                    }
                }
               
                if(thing != null && thing2 != null)
                {                   
                    return thing;
                }
               
              return null;
        }


        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            IdleTextChooser();
           IntCell= this.PositionHeld + this.def.interactionCellOffset.RotatedBy(this.Rotation);
            compPowah = this.TryGetComp<CompPowerTrader>();
            if(!thingtospawnID.NullOrEmpty())
            {
                if (!thingtospawnStuff.NullOrEmpty())
                {
                    //Log.Message("thingtospawnStuff=" + thingtospawnStuff);
                    thingToSpawn = ThingMaker.MakeThing(ThingDef.Named(thingtospawnID), ThingDef.Named(thingtospawnStuff));
                }
                else
                {
                    thingToSpawn = ThingMaker.MakeThing(ThingDef.Named(thingtospawnID));
                }
                thingToSpawn.HitPoints = thingtospawnHealth;
                //Log.Message("thingToSpawn=" + thingToSpawn.def.defName);
            }
            if (FindHopper() == null)
            {               
                Thing hopper = ThingMaker.MakeThing(ThingDef.Named("OldStorageRack"));
                hopper.SetFactionDirect(this.Faction);
                GenSpawn.Spawn(hopper, this.IntCell,this.Map, this.Rotation);
            }           
                
            
        }

        //private List<ThingCount> Calc2(Thing thing)
        //{
        //    List<ThingCount> thingSet = new List<ThingCount>();

        //    if (!thing.def.costList.NullOrEmpty())
        //    {
        //        foreach (ThingCount t in thing.def.costList)
        //        {
        //            ThingCount x = new ThingCount();
        //            x.ThingDef = t.ThingDef;
        //            float count = ((float)t.Count / 100f) * (((float)thing.HitPoints / (float)thing.MaxHitPoints) * 100);
        //            x.Count = Mathf.FloorToInt(count);
        //            if (x != null)
        //            {
        //                thingSet.Add(x);
        //            }
        //            else if (x == null)
        //            {
        //                Log.Message("Calc2 error cost list x =null");
        //            }
        //        }
        //    }
        //    if (thing.def.MadeFromStuff)
        //    {
        //        ThingCount z = new ThingCount();
        //        z.ThingDef = thing.Stuff;
        //        float count = ((float)thing.def.costStuffCount / 100f) * (((float)thing.HitPoints / (float)thing.MaxHitPoints) * 100);
        //        z.Count = Mathf.FloorToInt(count);
        //        if (z != null)
        //        {
        //            thingSet.Add(z);
        //        }
        //        else if (z == null)
        //        {
        //            Log.Message("Calc2 error stuff list x =null");
        //        }
        //    }

        //    return thingSet;
        //}

        //private void Scrapping(Thing thing)
        //{
        //    List<ThingCount> MatsToSpawn = Calc2(thing);

        //    int xx = Rand.Range(1, 10);

        //    if (MatsToSpawn.Count > 0)
        //    {

        //        foreach (ThingCount mat in MatsToSpawn)
        //        {
        //            Thing t = ThingMaker.MakeThing(mat.thingDef);
        //            //  Log.Message(mat.thingDef.defName + "count" + mat.count);
        //            if (mat.count <= 0)
        //            {
        //                mat.count = 1;
        //            }
        //            t.stackCount = mat.count;

        //            GenDrop.TryDropSpawn(t, this.IntCell, ThingPlaceMode.Near, out t);
        //        }
        //    }
        //    thing.Destroy();
        //    thingToSpawn = null;
        //}
          

        public void PowerUsage()
        {
            if(thingToSpawn!=null)
            {
                compPowah.PowerOutput = -compPowah.Props.basePowerConsumption;
            }
            else
            {
                compPowah.PowerOutput = -10;
            }
        }

        public override void Tick()
        {
            base.Tick();
            if (compPowah != null && compPowah.PowerOn)
            {
                PowerUsage();

                if (CheckTimer >= scrapingTime && thingToSpawn != null)
                {                   
                     //Scrapping(thingToSpawn);
                }
                
                if (CheckTimer >= 200 && thingToSpawn == null)
                {                   
                    thingToSpawn = FindThing();
                 //   Log.Message("looking for hopper");
                  
                    if (thingToSpawn != null)
                    {
                        thingtospawnID = thingToSpawn.def.defName;
                        thingtospawnHealth = thingToSpawn.HitPoints;
                        if(thingToSpawn.def.MadeFromStuff)
                        {
                            thingtospawnStuff = thingToSpawn.Stuff.defName;
                        }
                        thingToSpawn.DeSpawn();
                      //  Log.Message("despawn");
                    }
                    CheckTimer = 0;
                }
                CheckTimer++;
                if(textcounter>2000)
                {
                    IdleTextChooser();
                    textcounter = 0;
                }
                textcounter++;
            }
        }

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.GetInspectString());
            stringBuilder.AppendLine();
            if (compPowah != null && compPowah.PowerOn)
            {
                if (thingToSpawn == null)
                {
                    stringBuilder.Append("Status:" + idletext);
                }
                else if (thingToSpawn != null)
                {
                    stringBuilder.Append("Status: Shreeding!");
                    stringBuilder.AppendLine();

                    stringBuilder.Append("Way of " + CheckTimer.ToString() + " cut`s");
                }
            }
            else
            {
                stringBuilder.Append("Status: No Power");
            }
            stringBuilder.AppendLine();
    
            return stringBuilder.ToString();
        }
                         
     }
}
