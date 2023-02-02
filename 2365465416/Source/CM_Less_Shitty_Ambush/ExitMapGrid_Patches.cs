using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace CM_Less_Shitty_Ambush
{
    [StaticConstructorOnStartup]
    public static class ExitMapGrid_Patches
    {
        [HarmonyPatch(typeof(ExitMapGrid))]
        [HarmonyPatch("MapUsesExitGrid", MethodType.Getter)]
        public static class PocketDimensionExitCells_Patch
        {
            [HarmonyPostfix]
            private static void Postfix(ExitMapGrid __instance, Map ___map, ref bool __result)
            {
                if (__result == false && LessShittyAmbushMod.settings.allowExitMapBeforeWin)
                {
                    CaravansBattlefield caravansBattlefield = ___map.Parent as CaravansBattlefield;
                    if (caravansBattlefield != null && caravansBattlefield.def.blockExitGridUntilBattleIsWon && !caravansBattlefield.WonBattle)
                    {
                        AmbushTracker ambushTracker = Current.Game.World.GetComponent<AmbushTracker>();

                        int currentTick = Find.TickManager.TicksGame;

                        if (ambushTracker != null && currentTick > ambushTracker.CheckMapCreateTime(___map) + (LessShittyAmbushMod.settings.secondsUntilExitMapPossible * 60))
                            __result = true;
                    }
                }
            }
        }
    }
}
