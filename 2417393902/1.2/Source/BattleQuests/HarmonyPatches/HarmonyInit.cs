using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Grammar;

namespace BattleQuests
{
    [StaticConstructorOnStartup]
    internal static class HarmonyInit
    {
        static HarmonyInit()
        {
            new Harmony("BattleQuests.HarmonyInit").PatchAll();
        }
    }

    [HarmonyPatch(typeof(SettlementDefeatUtility), "IsDefeated")]
    internal static class IsDefeated_Patch
    {
        private static void Postfix(bool __result, Map map, Faction faction)
        {
            if (__result && map.Parent is Settlement settlement)
            {
                QuestUtility.SendQuestTargetSignals(settlement.questTags, "AllEnemiesDefeated");
            }
        }
    }
}
