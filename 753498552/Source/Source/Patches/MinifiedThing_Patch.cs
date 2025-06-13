using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Hospitality.Patches
{
    public class MinifiedThing_Patch
    {
        [HarmonyPatch(typeof(MinifiedThing), nameof(MinifiedThing.DrawAt))]
        public class DrawAt
        {
            [HarmonyPrefix]
            public static bool Prefix(MinifiedThing __instance)
            {
                if (__instance.InnerThing is not Building_Bed) return true;
                if (__instance.InnerThing?.def?.Minifiable != true)
                {
                    // Destroy next tick - This may be unsafe now!
                    DestroySafely(__instance);
                    return false;
                }
                return true;
            }

            private static Action DestroySafely(MinifiedThing __instance)
            {
                return () => {
                    try
                    {
                        if (__instance?.InnerThing?.def != null)
                        {
                            Log.Message("Removing invalid minified thing of type "+__instance.InnerThing.def.LabelCap+".");
                        }
                        __instance.Destroy();
                    }
                    catch (NullReferenceException)
                    {
                        // We're expecting one from the InstallBlueprintUtility - can't be avoided
                    }
                };
            }
        }
    }
}