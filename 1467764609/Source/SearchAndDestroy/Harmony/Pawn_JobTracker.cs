using Harmony;
using SearchAndDestroy.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace SearchAndDestroy.Harmony
{
    [HarmonyPatch(typeof(Pawn_JobTracker), "DetermineNextJob")]
    static class Pawn_JobTracker_DetermineNextJob
    {
        static void Postfix(Pawn_JobTracker __instance, ref ThinkResult __result)
        {
            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            
            if (pawn.Drafted)
            {
                ExtendedPawnData pawnData = Base.Instance.GetExtendedDataStorage().GetExtendedDataFor(pawn);
                if(pawnData.SD_enabled && __instance.jobQueue.Count > 0)
                {
                    QueuedJob qjob = __instance.jobQueue.Last();  
                    __instance.ClearQueuedJobs();
                    __result = new ThinkResult(qjob.job, __result.SourceNode, null, false);
                }
            }

        }
    }
}
