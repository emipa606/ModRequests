using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace TogglableStorageItemVisibility
{
    [StaticConstructorOnStartup]
    public static class Startup
    {
        static Startup()
        {
            new Harmony("TogglableStorageItemVisibilityMod").PatchAll();
            foreach (var thingDef in DefDatabase<ThingDef>.AllDefs)
            {
                if (typeof(Building_Storage).IsAssignableFrom(thingDef.thingClass))
                {
                    thingDef.comps ??= new List<CompProperties>();
                    thingDef.comps.Add(new CompProperties
                    {
                        compClass = typeof(CompToggleStorageItemVisibility)
                    });
                }
            }
        }

        public static bool HasToggleStorage(this Thing thing, out CompToggleStorageItemVisibility comp)
        {
            comp = thing.TryGetComp<CompToggleStorageItemVisibility>();
            return comp != null;
        }
        public static bool CameraPlusActive = ModsConfig.IsActive("brrainz.cameraplus");

        public static float GetCellSizePixels()
        {
            if (CameraPlusActive)
            {
                return CameraLerpCellSizePixels();
            }
            return Find.CameraDriver.CellSizePixels;
        }

        private static float CameraLerpCellSizePixels()
        {
            var rootSize = CameraPlus.Tools.LerpRootSize(Find.CameraDriver.rootSize);
            return (float)UI.screenHeight / (rootSize * 2f);
        }
    }
}
