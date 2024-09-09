using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Verse;
using RimWorld;

namespace Adjacent_Light
{
    [StaticConstructorOnStartup]
    static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            var harmony = new Harmony("AdjacentLight");
            harmony.PatchAll();
        }
    }


    [HarmonyPatch(typeof(PawnRecentMemory))]
    static class PawnRecentMemoryPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("RecentMemoryInterval")]
        static void RecentMemoryIntervalPatch(Pawn ___pawn, ref int ___lastLightTick)
        {
            if (___pawn.Spawned)
            {
                if (___lastLightTick < Find.TickManager.TicksGame)
                {
                    Room room = ___pawn.GetRoom();

                    foreach (IntVec3 cell in GenAdj.CellsAdjacent8Way(___pawn))
                    {
                        if (room.ContainsCell(cell) && ___pawn.Map.glowGrid.PsychGlowAt(cell) != 0)
                        {
                            //Log.Message(string.Concat(___pawn.Name, ": Yup in room ", i));
                            ___lastLightTick = Find.TickManager.TicksGame;
                            return;
                        }
                    }

                    foreach (IntVec3 cell in GenAdj.CellsAdjacentCardinal(___pawn))
                    {
                        if (___pawn.Map.glowGrid.PsychGlowAt(cell.ClampInsideMap(___pawn.Map)) != 0)
                        {
                            //Log.Message(string.Concat(___pawn.Name, ": Yup outside room", i));
                            ___lastLightTick = Find.TickManager.TicksGame;
                            return;
                        }
                    }

                    //Log.Message(string.Concat(___pawn.Name, ": Nope: ", i));
                }
            }
        }
    }

}
