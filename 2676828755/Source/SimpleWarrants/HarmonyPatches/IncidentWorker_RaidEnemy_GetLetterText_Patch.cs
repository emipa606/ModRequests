using HarmonyLib;
using RimWorld;
using Verse;

namespace SimpleWarrants
{

    [HarmonyPatch(typeof(IncidentWorker_RaidEnemy), "GetLetterText")]
    public static class IncidentWorker_RaidEnemy_GetLetterText_Patch
    {
        public static void Postfix(ref string __result)
        {
            if (RaidStrategyWorker_MakeLords_Patch.warrantToHunt != null)
            {
                var warrant = RaidStrategyWorker_MakeLords_Patch.warrantToHunt;
                if (warrant.rewardForDead > 0 && warrant.rewardForLiving > 0)
                {
                    __result += "\n\n" + "SW.RaidReasonKillCapture".Translate(warrant.pawn.Named("PAWN"), warrant.reason.ToLower());
                }
                else if (warrant.rewardForDead > 0)
                {
                    __result += "\n\n" + "SW.RaidReasonKill".Translate(warrant.pawn.Named("PAWN"), warrant.reason.ToLower());
                }
                else
                {
                    __result += "\n\n" + "SW.RaidReasonCapture".Translate(warrant.pawn.Named("PAWN"), warrant.reason.ToLower());
                }
            }
        }
    }
}
