using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace HarderGearLooting
{
    // Patch the ImpliedThingDefs method using HarmonyPostfix
    [HarmonyPatch(typeof(ThingDef))]
    [HarmonyPatch(nameof(ThingDef.ResolveReferences))]
    public static class ThingDefResolveReferencesPatch
    {
        [HarmonyPostfix]
        public static void PatchThingDefs_Postfix(ThingDef __instance)
        {

            if (HarderGearLootingSettings.expandBiocoding && HarderGearLooting.ShouldBeBiocodable(__instance))
            {
                if (__instance.comps == null)
                {
                    __instance.comps = new List<CompProperties>();
                }
                if (!__instance.comps.Any<CompProperties>((Func<CompProperties, bool>)(comp => comp is CompProperties_Biocodable)))
                {
                    CompProperties_Biocodable newComp = new CompProperties_Biocodable();
                    __instance.comps.Add(newComp);
                    newComp.ResolveReferences(__instance);
                }
            }
        }
       
    }
}
