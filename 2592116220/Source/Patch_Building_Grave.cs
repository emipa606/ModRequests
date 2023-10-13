using HarmonyLib;
using RimWorld;
using Verse;

namespace BuriedAlive
{
    [HarmonyPatch(typeof(Building_Grave))]
    [HarmonyPatch("Accepts")]
    public static class Patch_Building_Grave_Accepts
	{
        public static void Postfix(Building_Grave __instance, ref Thing thing, ref bool __result) 
        {
            if (!__result)
            {
                if (thing is Pawn)
                {
                    __result = true;
                }
            }
		}
    }
}
