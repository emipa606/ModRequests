using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace GardenCloche
{
    public class CompProperties_SelectCrop : CompProperties
    {
        public CompProperties_SelectCrop() => this.compClass = typeof (SelectCrop);
    }
    [StaticConstructorOnStartup]
    public class SelectCrop : ThingComp
    {
        private bool PowerOn
        {
            get
            {
                CompPowerTrader comp = this.parent.GetComp<CompPowerTrader>();
                return comp != null && comp.PowerOn;
            }
        }

        private Cloche p;
        
        public readonly Vector2 BarSize = new Vector2(1.3f, 0.2f);
        public static readonly Material BarFilledPoweredMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.45f, 0.85f, 0.3f));
        public static readonly Material BarFilledUnpoweredMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.85f, 0.3f, 0.45f));
        public static readonly Material BarUnfilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.3f, 0.3f, 0.3f));

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            p = (Cloche)parent;
            if (p.SelectedCrop == null)
            {
                CropSelectEvent(ThingDef.Named("Plant_Rice"));
            }
            if (p.BaseSpawnDelay == null || p.BaseSpawnDelay == 0)
            {
                p.BaseSpawnDelay = p.SpawnDelay;
            }
            if (p.BaseHarvestCount == null || p.BaseHarvestCount == 0)
            {
                p.BaseHarvestCount = p.HarvestCount;
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            yield return new Command_Action
            {
                //icon = ContentFinder<Texture2D>.Get(selectedCrop.graphic.path, true),
                icon = p.SelectedCrop.uiIcon,
                defaultLabel = "Change Crops",
                defaultDesc = "Change type of crops grown",
                action = delegate ()
                {
                    //The callback's code executed when the button is clicked
                    Log.Message("Clicked on gizmo");
                    GenerateMenu();
                }
            };
            yield break;
        }
        
        public override void CompTick()
        {
            base.CompTick();
            if (p.TicksToSpawn % 2500 == 0) {p.SetPowerConsumption(p.Power);}
            if (PowerOn)
                p.TicksToSpawn--;
            if (p.TicksToSpawn <= 0)
            {
                p.TicksToSpawn = p.SpawnDelay;
                DoSpawn(p.HarvestCount);
            }
        }
        
        public override void PostDraw()
        {
            base.PostDraw();
            GenDraw.FillableBarRequest r = new GenDraw.FillableBarRequest();
            r.center = parent.DrawPos + Vector3.up * 0.1f + Vector3.forward * -0.6f;
            r.size = BarSize;
            r.fillPercent = 1-((float)p.TicksToSpawn / (float)p.SpawnDelay);
            r.filledMat = PowerOn ? BarFilledPoweredMat : BarFilledUnpoweredMat;
            r.unfilledMat = BarUnfilledMat;
            r.margin = 0.1f;
            Rot4 rotation = parent.Rotation;
            r.rotation = rotation;
            GenDraw.DrawFillableBar(r);
        }
        
        public bool DoSpawn(int count) 
        {
            if (count <= p.SelectedCrop.plant.harvestedThingDef.stackLimit)
            {
                try
                {
                    IntVec3 result;
                    if (!CompSpawner.TryFindSpawnCell((Thing) this.parent, p.SelectedCrop.plant.harvestedThingDef,
                            p.HarvestCount, out result))
                        return false;
                    Thing t = ThingMaker.MakeThing(p.SelectedCrop.plant.harvestedThingDef);
                    t.stackCount = count;
                    Thing lastResultingThing;
                    GenPlace.TryPlaceThing(t, result, this.parent.Map, ThingPlaceMode.Direct, out lastResultingThing);
                    return true;
                }
                catch (NullReferenceException e)
                {
                    Log.Error(e.StackTrace);
                    return false;
                }
            }
            DoSpawn(p.SelectedCrop.plant.harvestedThingDef.stackLimit);
            DoSpawn(count - p.SelectedCrop.plant.harvestedThingDef.stackLimit);
            return true;
        }
        public void GenerateMenu()
        {
            List<FloatMenuOption> floatMenuOptionList = new List<FloatMenuOption>();
            
            
            foreach (ThingDef thingDef in DefDatabase<ThingDef>.AllDefs.Where<ThingDef>((Func<ThingDef, bool>) (def => def.category == ThingCategory.Plant)))
            {
                if (thingDef.plant.sowTags.Contains("Hydroponic") && Command_SetPlantToGrow.IsPlantAvailable(thingDef,this.parent.MapHeld))
                {
                    floatMenuOptionList.Add(new FloatMenuOption(thingDef.label, (Action) (() =>
                    {
                        CropSelectEvent(thingDef);
                    }),thingDef, extraPartWidth: 29f, extraPartOnGUI: ((Func<Rect, bool>) (rect => Widgets.InfoCardButton(rect.x + 5f, rect.y + (float) (((double) rect.height - 24.0) / 2.0), (Def) thingDef)))));
                }
            }
            Find.WindowStack.Add((Window) new FloatMenu(floatMenuOptionList));
        }

        public override string CompInspectStringExtra()
        {
            return String.Format("Collecting {0} x{1} in: {2}",p.SelectedCrop.plant.harvestedThingDef.label,p.HarvestCount,p.TicksToSpawn.ToStringTicksToPeriod().Colorize(ColoredText.DateTimeColor));
        }
        public void CropSelectEvent(ThingDef crop)
        {
            var config_props = (CompProperties_UpgradeCloche) (p.GetComp<UpgradeCloche>().props);
            p.SelectedCrop = crop;
            p.BaseSpawnDelay = (int)Math.Floor(crop.plant.growDays * 60000);
            p.BaseHarvestCount = (int)Math.Floor(crop.plant.harvestYield*config_props.cropsInCloche);
            p.SpawnDelay = p.BaseSpawnDelay;
            p.HarvestCount = p.BaseHarvestCount;
            p.TicksToSpawn = p.SpawnDelay;
        }
    }
}