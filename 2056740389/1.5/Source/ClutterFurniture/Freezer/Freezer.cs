using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse.AI;
using Verse;
using Verse.Sound;
using RimWorld;
using System.Reflection;

namespace Clutter_Furniture
{
    [StaticConstructorOnStartup]
    class Freezer : Building_Storage
    {
        private StorageSettings baseSettings;        
       // private int tickCounter = 0;
        private Thing MealInSlot;
        public static Graphic BottomTexture;
        public float Temp = -3000f;
        public CompPowerTrader powerComp;
        public CompGlower glow;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            powerComp = GetComp<CompPowerTrader>();
            glow = GetComp<CompGlower>();
            LongEventHandler.ExecuteWhenFinished(ReadXML);
        }

        public override void PostMake()
        {
            var ptr = typeof(ThingWithComps).GetMethod("PostMake").MethodHandle.GetFunctionPointer();
            var basePostMake = (Action)Activator.CreateInstance(typeof(Action), this, ptr);
            base.PostMake();
            this.baseSettings = new StorageSettings();
            this.baseSettings.CopyFrom(this.def.building.fixedStorageSettings);

            foreach (ThingDef def in DefDatabase<ThingDef>.AllDefs)
            {
                if (def.HasComp(typeof(CompRottable)))
                {
                    this.baseSettings.filter.SetAllow(def, true);
                }
            }

            this.settings = new StorageSettings(this);
            if (this.def.building.defaultStorageSettings != null)
            {
                this.settings.CopyFrom(this.def.building.defaultStorageSettings);
            }
        }

        public new bool StorageTabVisible
        {
            get
            {
                return base.StorageTabVisible;
            }
        }

        public new StorageSettings GetStoreSettings()
        {
            return this.settings;
        }

        public new StorageSettings GetParentStoreSettings()
        {
            return this.baseSettings;
        }
              

        public override void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                this.baseSettings = new StorageSettings();
            }

            base.ExposeData();

            Scribe_Values.Look<float>(ref this.Temp, "temp", -3000f, false);
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                this.baseSettings.CopyFrom(this.def.building.fixedStorageSettings);

                foreach (ThingDef def in DefDatabase<ThingDef>.AllDefs)
                {
                    if (def.HasComp(typeof(CompRottable)))
                    {
                        this.baseSettings.filter.SetAllow(def, true);
                    }
                }
            }
        }

        //public override Graphic Graphic
        //{
        //    get
        //    {
        //        if (BottomTexture != null && this.GetInnerIfMinified() == null)
        //        {
        //            return BottomTexture;
        //        }
        //        return base.Graphic;
        //    }
        //}

        private void ReadXML()
        {
            ClutterThingDefsFurniture clutterThingDefs = (ClutterThingDefsFurniture)def;
            if (!string.IsNullOrEmpty(clutterThingDefs.FreezerBase))
            {
                BottomTexture = GraphicDatabase.Get<Graphic_Single>(clutterThingDefs.FreezerBase, def.graphicData.Graphic.Shader);
            }
        }


        //private void DoTickWork(int ticks)
        //{
        //    tickCounter -= ticks;
        //    if (tickCounter <= 0)
        //    {
        //        tickCounter = Rand.RangeInclusive(45, 90);

        //        if (MealInSlot == null || MealInSlot != FindItemAtPosition(Position))
        //        {
        //            MealInSlot = FindItemAtPosition(Position);

        //        }
        //    }
        //}

        public override void TickRare()
        {
            if (Temp < -2000f)
            {
                Temp = Position.GetTemperature(this.Map);
            }

            foreach (var cell in AllSlotCells())
            {
                foreach (var thing in cell.GetThingList(this.Map))
                {
                    if (thing != this)
                    {
                        var rottable = thing.TryGetComp<CompRottable>();
                        if (rottable != null && !(rottable is CompBetterRottable))
                        {
                            var li = thing as ThingWithComps;
                            var newRot = new CompBetterRottable();
                            li.AllComps.Remove(rottable);
                            li.AllComps.Add(newRot);
                            newRot.props = rottable.props;
                            newRot.parent = li;
                            newRot.RotProgress = rottable.RotProgress;


                        }
                      
                        if (thing.def.ingestible!=null && thing.def.ingestible.foodType == FoodTypeFlags.Meal)
                        {
                            MealInSlot = thing;
                        }
                    }
                    
                }
            }

            if (!slotEmpty())
            {
            var roomTemp = Position.GetTemperature(this.Map);
            var changeTemp = (roomTemp - Temp) * 0.01f;
            var changeEnergy = -changeTemp;
            var powerMult = 0f;
            if ((Temp + changeTemp) > -10)
            {
                var change = Mathf.Max((-10 - (Temp + changeTemp)), -1f);
                if (powerComp != null && powerComp.PowerOn)
                {
                    changeTemp += change;
                    changeEnergy -= change * 1.25f;
                }
                powerMult = change * -1f;
            }
            this.Temp += changeTemp;

            Room room = this.GetRoom();
                                                     
                this.powerComp.PowerOutput = -((CompProperties_Power)powerComp.props).basePowerConsumption * (powerMult * 0.9f + 0.1f);
            }
            else if(slotEmpty())
            {
                this.powerComp.PowerOutput = 5f;
                this.Temp = 0;
            }
        }
        
        public bool slotEmpty()
        {
            if(MealInSlot!=null)
            {
                if(MealInSlot.Position==this.Position)
                {
                    return false;
                }
            }
            return true;
        }


        //private Thing FindItemAtPosition(IntVec3 pos)
        //{
        //    var mealCategory = ThingCategoryDef.Named("FoodMeals");
        //    var enumerable = this.Map.listerThings.AllThings.Where(x =>
        //    (x.Position == pos) &&
        //    (!x.def.thingCategories.NullOrEmpty()) &&
        //    (x.def.thingCategories.Contains(mealCategory))
        //    );
        //    Thing result = null;
        //    foreach (Thing current in enumerable)
        //    {
        //        if (current.def.category == ThingCategory.Item && current != this)
        //        {
        //            result = current;
        //            break;
        //        }
        //    }
        //    return result;
        //}
        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            string b = base.GetInspectString();
            if (b.Length > 0)
            {
                stringBuilder.Append(b);
                stringBuilder.Append(Environment.NewLine);
            }
            stringBuilder.Append("Target Temperature" + ": " + (-10f).ToStringTemperature("F0"));
            stringBuilder.Append(Environment.NewLine);
            stringBuilder.Append("Temperature" + ": " + Temp.ToStringTemperature("F0"));
            stringBuilder.Append(Environment.NewLine);
            if (slotEmpty())
            {
                stringBuilder.Append("Power: " + (powerComp != null && powerComp.PowerOn ? "Power Saving Mode" : "Off"));
            }
            else
            {
                stringBuilder.Append("Power: " + (powerComp != null && powerComp.PowerOn ? "On" : "Off"));
            }
            return stringBuilder.ToString().TrimEndNewlines();
        }


        public override void Draw()
        {
            base.Draw();
           // DoTickWork(1);
            if (BottomTexture != null)
            {
                //Log.Message("BottomTexture");
                Matrix4x4 matrix2 = default(Matrix4x4);
                Vector3 x = new Vector3(1f, 0f, 1f);
                matrix2.SetTRS(this.DrawPos + Altitudes.AltIncVect+new Vector3(0,-0.3f,0), Rotation.AsQuat, x);
                Graphics.DrawMesh(MeshPool.plane10, matrix2, BottomTexture.MatAt(this.Rotation), 0);

                if (MealInSlot != null)
                {
                    //Log.Message("MealInSlot");
                    Thing food = slotGroup.HeldThings.FirstOrDefault();
                    Matrix4x4 matrix = default(Matrix4x4);
                    Vector3 s = new Vector3(0.8f, 0f, 0.8f);
                    matrix.SetTRS(this.DrawPos + Altitudes.AltIncVect + new Vector3(0, -0.2f, 0), Rotation.AsQuat, s);
                    Graphics.DrawMesh(MeshPool.plane10, matrix, MealInSlot.Graphic.MatAt(Rotation), 0);

                    //if(powerComp != null && powerComp.PowerOn)
                    //{

                    //}
                }
            }
        }
    }
}
