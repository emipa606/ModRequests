using HarmonyLib;
using Verse;

namespace BattleQuests;

[StaticConstructorOnStartup]
internal static class HarmonyInit
{
    static HarmonyInit()
    {
        new Harmony("BattleQuests.HarmonyInit").PatchAll();
    }
}