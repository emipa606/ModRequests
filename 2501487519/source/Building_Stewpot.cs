using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace AOBAStew
{
    // TODO May need to postfix FoodRestrictionDatabase, or find a way to make new meal type there properly including on save/load/restart.

    // Info: This is building off the nutrient paste dispenser class in order to attempt to have it work through the core codebase without an issue.
    // It means messing around to remove the powercomp stuff, but if it makes all the job/toil stuff work, then it is worth it.
    public class Building_StewPot : Building_NutrientPasteDispenser
    {
        //public static int CollectDuration = 50;
        //public CompPowerTrader powerComp;
        //private List<IntVec3> cachedAdjCellsCardinal;

        public CompRefuelable refuelableComp;
        private FloatRange smokeSize = new FloatRange(0.2f, .8f);

        public new bool CanDispenseNow
        {
            get
            {
                if (this.refuelableComp.HasFuel)
                    return this.HasEnoughFeedstockInHoppers();
                return false;
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            // TODO Do something about the base.spawnsetup using the powercomp
            this.powerComp = null;
            this.refuelableComp = this.GetComp<CompRefuelable>();
        }

        //public List<IntVec3> AdjCellsCardinalInBounds
        //{
        //    get
        //    {
        //        if (this.cachedAdjCellsCardinal == null)
        //            this.cachedAdjCellsCardinal = GenAdj.CellsAdjacentCardinal((Thing)this).Where<IntVec3>((Func<IntVec3, bool>)(c => c.InBounds(this.Map))).ToList<IntVec3>();
        //        return this.cachedAdjCellsCardinal;
        //    }
        //}

        // TODO  May have to add a harmony post fix on this, if it doesn't use the overriden one. Which it won't.
        public override ThingDef DispensableDef
        {
            get
            {
                return AOBADefOf.FT_MealStew;
            }
        }

        // TODO - May need a harmony patch here, if they won't access the slop pails.
        //public override Building AdjacentReachableHopper(Pawn reacher)
        //{
        //    for (int index = 0; index < this.AdjCellsCardinalInBounds.Count; ++index)
        //    {
        //        Building edifice = this.AdjCellsCardinalInBounds[index].GetEdifice(this.Map);
        //        if (edifice != null && edifice.def == AOBADefOf.FT_FoodTable && reacher.CanReach((LocalTargetInfo)((Thing)edifice), PathEndMode.Touch, Danger.Deadly, false, TraverseMode.ByPawn))
        //            return edifice;
        //    }
        //    return (Building)null;
        //}

        public override void Tick()
        {
            base.Tick();
            ResolveSmoke();
        }

        private void ResolveSmoke()
        {
            if (Find.TickManager.TicksGame % 200 == 0 && this.refuelableComp.HasFuel)
            {
                IntVec3 smokePos = this.Position + GenAdj.CardinalDirections[Rot4.North.AsInt] +
                    GenAdj.CardinalDirections[Rot4.North.AsInt];
                //Vector3 smokePosV3 = smokePos.ToVector3();
                float smokePosX = (float)smokePos.x;
                float smokePosZ = (float)smokePos.z;
                if (this.Rotation == Rot4.North || this.Rotation == Rot4.South)
                {
                    smokePosX += 1.0f;
                }
                if (this.Rotation == Rot4.South)
                {
                    smokePosX -= 0.75f;
                    smokePosZ -= 1.0f;
                }
                else if (this.Rotation == Rot4.West)
                {
                    smokePosX += .25f;
                }
                else if (this.Rotation == Rot4.East)
                {
                    smokePosX += 0.75f;
                    smokePosZ -= 1.0f; 
                }
                if (GenView.ShouldSpawnMotesAt(smokePos, this.Map))
                {
                    MoteThrown moteThrown =
                        (MoteThrown)ThingMaker.MakeThing(ThingDef.Named("Mote_Smoke"), null);
                    moteThrown.Scale = Rand.Range(1.5f, 2.5f) * smokeSize.RandomInRange;
                    moteThrown.exactRotation = Rand.Range(-0.5f, 0.5f);
                    moteThrown.exactPosition = new Vector3(smokePosX + Rand.Range(-0.1f, 0.1f), 0,
                        smokePosZ + Rand.Range(-0.1f, 0.1f));
                    moteThrown.airTimeLeft = 5000f;
                    moteThrown.SetVelocity((float)Rand.Range(30, 40), Rand.Range(0.008f, 0.012f));
                    GenSpawn.Spawn(moteThrown, smokePos, this.Map);
                }
            }
        }

        // TODO - may need a harmony patch on this one, if the correct food stuff doesn't come out. 
        public override Thing TryDispenseFood()
        {
            //Log.Error("TryDispenseFood BUILDING");
            if (!this.CanDispenseNow)
                return (Thing)null;
            float num = this.def.building.nutritionCostPerDispense - 0.0001f;
            List<ThingDef> thingDefList = new List<ThingDef>();
            do
            {
                Thing feedInAnyHopper = this.FindFeedInAnyHopper();
                if (feedInAnyHopper == null)
                {
                    //Log.Error("Did not find enough food in slop pails for a StewPot meal.", false);
                    return (Thing)null;
                }
                int count = Mathf.Min(feedInAnyHopper.stackCount, Mathf.CeilToInt(num / feedInAnyHopper.GetStatValue(StatDefOf.Nutrition, true)));
                num -= (float)count * feedInAnyHopper.GetStatValue(StatDefOf.Nutrition, true);
                thingDefList.Add(feedInAnyHopper.def);
                feedInAnyHopper.SplitOff(count);
            }
            while ((double)num > 0.0);
            this.def.building.soundDispense.PlayOneShot((SoundInfo)new TargetInfo(this.Position, this.Map, false));
            Thing thing = ThingMaker.MakeThing(this.DispensableDef, (ThingDef)null);
            CompIngredients comp = thing.TryGetComp<CompIngredients>();
            for (int index = 0; index < thingDefList.Count; ++index)
                comp.RegisterIngredient(thingDefList[index]);
            return thing;
        }

        // TODO - See if this will work. May need a harmony patch.
        public override Thing FindFeedInAnyHopper()
        {
            //Log.Error("FindFeedInAnyHopper BUILDING");
            for (int index1 = 0; index1 < this.AdjCellsCardinalInBounds.Count; ++index1)
            {
                Thing thing1 = (Thing)null;
                Thing thing2 = (Thing)null;
                List<Thing> thingList = this.AdjCellsCardinalInBounds[index1].GetThingList(this.Map);
                for (int index2 = 0; index2 < thingList.Count; ++index2)
                {
                    Thing thing3 = thingList[index2];
                    if (Building_StewPot.IsAcceptableFeedstock(thing3.def))
                        thing1 = thing3;
                    if (thing3.def == AOBADefOf.FT_FoodTable)
                        thing2 = thing3;
                }
                if (thing1 != null && thing2 != null)
                    return thing1;
            }
            return (Thing)null;
        }

        // TODO May have to add a harmony patch to do this, if this one isn't called.
        public override bool HasEnoughFeedstockInHoppers()
        {
            //Log.Error("HasEnoughFeedstockInHoppers BUILDING");
            float num = 0.0f;
            for (int index1 = 0; index1 < this.AdjCellsCardinalInBounds.Count; ++index1)
            {
                IntVec3 cellsCardinalInBound = this.AdjCellsCardinalInBounds[index1];
                Thing thing1 = (Thing)null;
                Thing thing2 = (Thing)null;
                Map map = this.Map;
                List<Thing> thingList = cellsCardinalInBound.GetThingList(map);
                for (int index2 = 0; index2 < thingList.Count; ++index2)
                {
                    Thing thing3 = thingList[index2];
                    if (Building_StewPot.IsAcceptableFeedstock(thing3.def))
                        thing1 = thing3;
                    if (thing3.def == AOBADefOf.FT_FoodTable)
                        thing2 = thing3;
                }
                if (thing1 != null && thing2 != null)
                    num += (float)thing1.stackCount * thing1.GetStatValue(StatDefOf.Nutrition, true);
                if ((double)num >= (double)this.def.building.nutritionCostPerDispense)
                    return true;
            }
            return false;
        }

        // The base class won't work for this call, since it uses the power comp.
        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            QuestUtility.AppendInspectStringsFromQuestParts(stringBuilder, (ISelectable)this);
            string str = this.InspectStringPartsFromCompsLocal();
            if (!str.NullOrEmpty())
            {
                if (stringBuilder.Length > 0)
                    stringBuilder.AppendLine();
                stringBuilder.Append(str);
            }
            // Nothing is socially proper for a beastman.
            //if (!this.IsSociallyProper((Pawn)null, false, false))
            //    stringBuilder.AppendLine((string)"InPrisonCell".Translate());
            return stringBuilder.ToString().Trim();
        }

        protected string InspectStringPartsFromCompsLocal()
        {
            if (this.AllComps == null)
                return (string)null;
            StringBuilder stringBuilder = new StringBuilder();
            for (int index = 0; index < this.AllComps.Count; ++index)
            {
                if (!(this.AllComps[index] is CompPower))
                { 
                    string str = this.AllComps[index].CompInspectStringExtra();
                    if (!str.NullOrEmpty())
                    {
                        if (Prefs.DevMode && char.IsWhiteSpace(str[str.Length - 1]))
                        {
                            //Log.ErrorOnce(this.AllComps[index].GetType().ToString() + " CompInspectStringExtra ended with whitespace: " + str, 25612, false);
                            str = str.TrimEndNewlines();
                        }
                        if (stringBuilder.Length != 0)
                            stringBuilder.AppendLine();
                        stringBuilder.Append(str);
                    }
                }
            }
            return stringBuilder.ToString();
        }
    }
}
