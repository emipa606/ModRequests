using HarmonyLib;
using Quarry;
using Verse;
using Verse.AI;

namespace StopQuarryingWhenFull
{
    [HarmonyPatch(typeof(WorkGiver_MineQuarry), nameof(WorkGiver_MineQuarry.JobOnThing))]
    static class WorkGiver_MineQuarry_JobOnThing_Patch
    {
        public static bool Prefix(ref Job __result, Pawn pawn, Thing t, bool forced)
        {
            var quarry = t as Building_Quarry;
            if (quarry != null && StopQuarryingWhenFull.QuarryIsFull(quarry))
            {
                __result = null;

                return false;
            }

            return true;
        }
    }
}
