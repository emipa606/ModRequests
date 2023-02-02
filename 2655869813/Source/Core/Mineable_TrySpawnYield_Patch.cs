using HarmonyLib;
using RimWorld;
using Verse;

namespace BenLubarsVanillaExpandedPatches
{
    [HarmonyPatch(typeof(Mineable), "TrySpawnYield")]
    static class Mineable_TrySpawnYield_Patch
    {
        private static Pawn currentPawn;

        public static void Prefix(Pawn pawn)
        {
            currentPawn = pawn;
        }

        public static void Postfix()
        {
            currentPawn = null;
        }

        [HarmonyPatch(typeof(Pawn), nameof(Pawn.IsColonist), MethodType.Getter)]
        [HarmonyPrefix]
        public static bool IsColonist(ref bool __result, Pawn __instance)
        {
            if (__instance == currentPawn && __instance.Faction == Faction.OfPlayer)
            {
                switch (BenLubarsVanillaExpandedPatches.autoAllowMinedOres.Value)
                {
                    case HomeAreaOrAlways.Always:
                        __result = true;

                        return false;
                    case HomeAreaOrAlways.HomeArea:
                        if (__instance.Position.InBounds(__instance.Map) && __instance.Map.areaManager.Home[__instance.Position])
                        {
                            __result = true;

                            return false;
                        }

                        break;
                    case HomeAreaOrAlways.Never:
                        break;
                }
            }

            return true;
        }
    }
}
