using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace GardenCloche
{
    public class CompProperties_UpgradeCloche : CompProperties
    {
        public int basePower = 100;
        public int powerLimit = 2500;
        
        public float growthMin = 20;
        public float growthMax = 500;
        
        public float yieldMin = 20;
        public float yieldMax = 500;

        public int cropsInCloche = 4;
        
        public CompProperties_UpgradeCloche() => this.compClass = typeof (UpgradeCloche);
    }
    
    [StaticConstructorOnStartup]
    public class UpgradeCloche : ThingComp
    {

        public Cloche p;

        public static int ClipSpawnDelay = 0;
        public static int ClipYield = 0;
        public static int ClipPower = 0;
        public static ThingDef ClipSelectedCrop = new ThingDef();
        public static ThingDef ClipSelectedCloche = new ThingDef();

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            p = (Cloche)parent;
        }
        
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            yield return new Command_Action
            {
                icon = ContentFinder<Texture2D>.Get("UI/tune_crops", true),
                defaultLabel = "Tune Crop Growth",
                defaultDesc = "Change crop growth properties",
                action = delegate ()
                {
                    //The callback's code executed when the button is clicked
                    Log.Message("Clicked on upgrade gizmo");
                    UpgradeWindow uw = new UpgradeWindow();
                    uw.BaseSpawnDelay = ((Cloche) parent).BaseSpawnDelay;
                    uw.BaseYield = ((Cloche) parent).BaseHarvestCount;
                    uw.cropTexture = ((Cloche) parent).SelectedCrop.uiIcon;
                    // TODO: set growth multipliers in the upgrade window such that you can finetune the already set settings
                    uw.GrowthMultiplier = ((Cloche) parent).GrowthSettingCache;
                    uw.YieldMultiplier = ((Cloche) parent).YieldSettingCache;

                    uw.parent = this;
                    Find.WindowStack.Add(uw);
                }
            };
            yield return new Command_Action
            {
                icon = ContentFinder<Texture2D>.Get("UI/copy", true),
                defaultLabel = "Copy current settings",
                defaultDesc = "Copy cloche settings",
                action = delegate ()
                {
                    //The callback's code executed when the button is clicked
                    Log.Message("Clicked on copy gizmo");
                    ClipSpawnDelay = ((Cloche) parent).SpawnDelay;
                    ClipYield = ((Cloche) parent).HarvestCount;
                    ClipSelectedCrop = ((Cloche) parent).SelectedCrop;
                    ClipPower = ((Cloche) parent).Power;
                    ClipSelectedCloche = parent.def;
                }
            };
            if (ClipSpawnDelay != 0)
            {
                yield return new Command_Action
                {
                    icon = ContentFinder<Texture2D>.Get("UI/paste", true),
                    defaultLabel = "Paste current settings",
                    defaultDesc = "Paste cloche settings",
                    action = delegate ()
                    {
                        //The callback's code executed when the button is clicked
                        Log.Message("Clicked on paste gizmo");
                        if (parent.def == ClipSelectedCloche)
                        {
                            ((Cloche) parent).SpawnDelay = ClipSpawnDelay;
                            ((Cloche) parent).HarvestCount = ClipYield;
                            ((Cloche) parent).SelectedCrop = ClipSelectedCrop;
                            ((Cloche) parent).Power = ClipPower;
                            ((Cloche) parent).SetPowerConsumption(ClipPower);
                            ((Cloche) parent).TicksToSpawn = ClipSpawnDelay;
                        }
                    }
                };
            }
        }
    }

    public class UpgradeWindow : Window
    {
        public int GrowthMultiplier = 100;
        public int YieldMultiplier = 100;
        
        public int BaseSpawnDelay = 0;
        public int SpawnDelay = 0;
        
        public int BaseYield = 0;
        public int Yield = 0;
        
        public Texture2D cropTexture;
        public UpgradeCloche parent;

        public UpgradeWindow()
        {
            doCloseX = true;
        }
        
        public override void PreOpen()
        {
            base.PreOpen();
            windowRect.height = 300f;
            windowRect.width = 400f;
        }
        
        public override void DoWindowContents(Rect inRect)
        {
            var props = ((CompProperties_UpgradeCloche) parent.props);
            var powerLimit = props.powerLimit;
            
            Widgets.BeginGroup(inRect);
            Rect rect1 = new Rect(0.0f, 0.0f, inRect.width * 0.75f, 70f);
            Rect rect2 = new Rect(0.0f, rect1.yMax, rect1.width, 40f);
            Rect rect3 = new Rect(0.0f, rect2.yMax, rect1.width, 40f);

            float powerUsage = (float)parent.p.Power / powerLimit;
            Rect rectPowBarBack = new Rect(0.0f, rect3.yMax, rect1.width, 20f);
            Rect rectPowBarFront = new Rect(0.0f, rect3.yMax, rect1.width*Math.Min(powerUsage,1), 20f);
            
            Rect rect5 = new Rect(0.0f, rectPowBarBack.yMax + 10f, rect1.width / 2, 50f);
            
            Rect rect11 = new Rect(inRect.width * 0.80f, 45.0f, inRect.width * 0.2f, inRect.width * 0.2f);
            
            Text.Font = GameFont.Medium;
            Widgets.Label(rect1, "Cloche Tuning");
            GrowthMultiplier = Mathf.RoundToInt(Widgets.HorizontalSlider(rect2,GrowthMultiplier,props.growthMin,props.growthMax,false,null,"Tune Growth Time",SpawnDelay.ToStringTicksToPeriod()+" ("+GrowthMultiplier+"%)",10f));
            SpawnDelay = (int)(BaseSpawnDelay/(GrowthMultiplier/100f));
            
            YieldMultiplier = Mathf.RoundToInt(Widgets.HorizontalSlider(rect3,YieldMultiplier,props.yieldMin,props.yieldMax,false,null,"Tune Yield",Yield+" ("+YieldMultiplier+"%)",10f));
            Yield = (int)(BaseYield*(YieldMultiplier/100f));
            
            
            parent.p.Power = Mathf.FloorToInt(Mathf.Pow(GrowthMultiplier / 100f, 1.8f) * Mathf.Pow(YieldMultiplier / 100f, 2) * props.basePower);
            
            Text.Font = GameFont.Small;
            if (Widgets.ButtonText(rect5, parent.p.Power  <= powerLimit ? "Apply" : "Above Power Limit",true,true,parent.p.Power <= powerLimit))
            {
                Log.Message("Clicked On close box");
                Cloche c = this.parent.p;
                c.HarvestCount = Yield;
                c.SpawnDelay = SpawnDelay;
                c.TicksToSpawn = SpawnDelay;
                c.GrowthSettingCache = GrowthMultiplier;
                c.YieldSettingCache = YieldMultiplier;
                c.SetPowerConsumption(parent.p.Power);
                Close();
            }
            // Side logo
            
            Widgets.DrawRectFast(rectPowBarBack,new Color(0,0,0));
            if (powerUsage <= 1)
            {
                Widgets.DrawRectFast(rectPowBarFront, new Color(0f, 0.5f, 0.2f));
            }
            else
            {
                Widgets.DrawRectFast(rectPowBarFront,new Color(0.5f,0.2f,0.2f));
            }
            Widgets.Label(rectPowBarBack,parent.p.Power +" W / "+powerLimit+" W");

            Widgets.DrawTextureFitted(rect11,cropTexture,1f);
            
            Widgets.EndGroup();
        }
    }
}