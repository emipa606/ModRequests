using System.Collections.Generic;
using HarmonyLib;
using Verse;
using RimWorld;
using UnityEngine;

namespace ForgettablePumps
{
    public class ForgettablePumpsSettings : ModSettings
    {
        public bool AlertOnly = false;
        public bool DebridgeOnDry = false;
    }

    public class ForgettablePumps : Mod
    {
        private static ForgettablePumpsSettings settings;
        private static bool minifyEverythingLoaded = false;
        public ForgettablePumps(ModContentPack content) : base(content)
        {
            settings = GetSettings<ForgettablePumpsSettings>();
            minifyEverythingLoaded = ModsConfig.IsActive("erdelf.MinifyEverything");
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect); 
            listingStandard.CheckboxLabeled((string) "AlertOnlyExplain".Translate(),
                ref settings.AlertOnly,
                (string) "AlertOnlyTooltip".Translate());
            listingStandard.CheckboxLabeled((string) "DebridgeOnDryExplain".Translate(),
                ref settings.DebridgeOnDry,
                (string) "DebridgeOnDryTooltip".Translate());
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "ModName".Translate();
        }
        
        //CompTerrainPumpDry::AffectCell
        [StaticConstructorOnStartup]
        public static class CompTerrainPumpDryPatch
        {
            static CompTerrainPumpDryPatch()
            {
                var m = typeof(RimWorld.CompTerrainPumpDry).GetMethod("AffectCell");
                var post = typeof(ForgettablePumps.CompTerrainPumpDryPatch).GetMethod("PostAffectCell",
                    System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                new HarmonyLib.Harmony("topodic.forgettablepumps").Patch(m,
                    postfix: new HarmonyLib.HarmonyMethod(post));
            }

            public static void PostAffectCell(CompTerrainPumpDry __instance, Map __0, IntVec3 __1)
            {
                if (__1.GetTerrain(__0).bridge && !__0.designationManager.HasMapDesignationAt(__1) && settings.DebridgeOnDry)
                {
                    __0.designationManager.AddDesignation(new Verse.Designation((LocalTargetInfo)__1,
                        DesignationDefOf.RemoveFloor));
                }
            }
        }

        //CompTerrainPump::CompTickRare
        [StaticConstructorOnStartup]
        public static class CompTerrainPumpPatch
        {
            static CompTerrainPumpPatch()
            {
                var m = typeof(RimWorld.CompTerrainPump).GetMethod("CompTickRare");
                var post = typeof(ForgettablePumps.CompTerrainPumpPatch).GetMethod("PostCompTickRare",
                    System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                new HarmonyLib.Harmony("topodic.forgettablepumps").Patch(m,
                    postfix: new HarmonyLib.HarmonyMethod(post));
            }

            public static void PostCompTickRare(CompTerrainPump __instance)
            {
                var traverse = new Traverse(__instance);
                var props = traverse.Property("Props").GetValue<CompProperties_TerrainPumpDry>();


                var maxRadius = props.radius;
                var currentRadius = traverse.Property("CurrentRadius").GetValue<float>();
                var progress = currentRadius / maxRadius;
                var thing = __instance.parent;

                if (progress >= 9.99E-1f)
                {
                    var mapComponent = thing.Map.GetComponent<ForgettablePumpsMapComponent>();
                    var list = mapComponent.finishedPumps;
                    if (list.Contains(thing)) return;
                    list.Add(thing);

                    var messageToShow = settings.AlertOnly
                        ? "MoisturePumpFinishedAlertOnly"
                        : (minifyEverythingLoaded 
                            ? "MoisturePumpFinishedMinifyEverything" 
                            : "MoisturePumpFinished");
                    Messages.Message((string) messageToShow.Translate(),
                        (LookTargets)thing, MessageTypeDefOf.TaskCompletion);
                    if (!settings.AlertOnly)
                    {
                        var designation = minifyEverythingLoaded
                            ? DesignationDefOf.Uninstall
                            : DesignationDefOf.Deconstruct;
                        thing.Map.designationManager.AddDesignation(new Verse.Designation((LocalTargetInfo)thing,
                            designation));
                    }
                }
                else if (minifyEverythingLoaded)
                {
                    var mapComponent = thing.Map.GetComponent<ForgettablePumpsMapComponent>();
                    var list = mapComponent.finishedPumps;
                    if (!list.Contains(thing)) return;
                    list.Remove(thing);
                }
            }
        }
    }

    public class ForgettablePumpsMapComponent : MapComponent
    {
        public List<Thing> finishedPumps = new List<Thing>();

        public ForgettablePumpsMapComponent(Map map) : base(map)
        {
        }
        
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref finishedPumps, "finishedPumps", LookMode.Reference);
        }
    }
}