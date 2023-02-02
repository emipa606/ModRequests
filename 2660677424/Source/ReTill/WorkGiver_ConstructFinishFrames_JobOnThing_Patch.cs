using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace ReTill
{
    [HarmonyPatch(typeof(WorkGiver_ConstructFinishFrames), nameof(WorkGiver_ConstructFinishFrames.JobOnThing))]
    static class WorkGiver_ConstructFinishFrames_JobOnThing_Patch
    {
        public static bool Prefix(ref Job __result, Pawn pawn, Thing t)
        {
            var frame = t as Frame;
            if (frame == null)
            {
                return true;
            }

            if (GenConstruct.FirstBlockingThing(frame, pawn) != null)
            {
                return true;
            }

            if (ReTill.IsTilledSoilDef(frame.def.entityDefToBuild))
            {
                __result = null;

                return false;
            }

            return true;
        }
    }
}
