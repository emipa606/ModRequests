using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace BattleQuests;

[HarmonyPatch(typeof(SettlementDefeatUtility), "IsDefeated")]
internal static class IsDefeated_Patch
{
    private static void Postfix(bool __result, Map map)
    {
        if (__result && map.Parent is Settlement settlement)
        {
            QuestUtility.SendQuestTargetSignals(settlement.questTags, "AllEnemiesDefeated");
        }
    }
}