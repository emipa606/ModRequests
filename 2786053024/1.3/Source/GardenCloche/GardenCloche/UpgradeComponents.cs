using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace GardenCloche
{
    public class CompProperties_UpgradeCloche : CompProperties
    {
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
                        ((Cloche) parent).SpawnDelay = ClipSpawnDelay;
                        ((Cloche) parent).HarvestCount = ClipYield;
                        ((Cloche) parent).SelectedCrop = ClipSelectedCrop;
                        ((Cloche) parent).Power = ClipPower;
                        ((Cloche) parent).SetPowerConsumption(ClipPower);
                        ((Cloche) parent).TicksToSpawn = ClipSpawnDelay;
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
            Widgets.BeginGroup(inRect);
            Rect rect1 = new Rect(0.0f, 0.0f, inRect.width * 0.75f, 70f);
            Rect rect2 = new Rect(0.0f, rect1.yMax, rect1.width, 40f);
            Rect rect3 = new Rect(0.0f, rect2.yMax, rect1.width, 40f);
            Rect rect4 = new Rect(0.0f, rect3.yMax, rect1.width, 30f);
            Rect rect5 = new Rect(0.0f, rect4.yMax, rect1.width / 2, 50f);
            
            Rect rect11 = new Rect(inRect.width * 0.80f, 45.0f, inRect.width * 0.2f, inRect.width * 0.2f);
            
            Text.Font = GameFont.Medium;
            Widgets.Label(rect1, "Cloche Tuning");
            GrowthMultiplier = Mathf.RoundToInt(Widgets.HorizontalSlider(rect2,GrowthMultiplier,20f,500f,false,null,"Tune Growth Time",SpawnDelay.ToStringTicksToPeriod()+" ("+GrowthMultiplier+"%)",10f));
            SpawnDelay = (int)(BaseSpawnDelay/(GrowthMultiplier/100f));
            
            YieldMultiplier = Mathf.RoundToInt(Widgets.HorizontalSlider(rect3,YieldMultiplier,20f,500f,false,null,"Tune Yield",Yield+" ("+YieldMultiplier+"%)",10f));
            Yield = (int)(BaseYield*(YieldMultiplier/100f));

            parent.p.Power = Mathf.FloorToInt(Mathf.Pow(GrowthMultiplier / 100f, 1.8f) * Mathf.Pow(YieldMultiplier / 100f, 2) * 100);
            
            Text.Font = GameFont.Small;
            Widgets.Label(rect4,"Power Consumption: "+parent.p.Power +" W");
            
            if (Widgets.ButtonText(rect5, parent.p.Power  <= 2500 ? "Apply" : "Above Power Limit",true,true,parent.p.Power <= 2500))
            {
                Cloche c = this.parent.p;
                c.HarvestCount = Yield;
                c.SpawnDelay = SpawnDelay;
                c.TicksToSpawn = SpawnDelay;
                c.SetPowerConsumption(parent.p.Power);
                Close();
            }
            // Side logo
            
            Widgets.DrawTextureFitted(rect11,cropTexture,1f);
            
            Widgets.EndGroup();
        }
    }
}