using HarmonyLib;
using Verse;

namespace TerrainOverhaul
{
    [HarmonyPatch(typeof(MouseoverReadout), "MouseoverReadoutOnGUI")]
    public class FertReadoutFixPatch
    {
        static void Postfix(ref string ___cachedTerrainString)
        {
            try
            {
                IntVec3 c = UI.MouseCell();
                TerrainDef terrain = c.GetTerrain(Find.CurrentMap);

                if (terrain == null || !(terrain is AddFertileTerrainDef def))
                    return;

                string t = terrain.fertility > 0.0001
                    ? " " + "FertShort".TranslateSimple() + " " + GetCorrectFertility(c, def)
                    : "";
                ___cachedTerrainString = terrain.LabelCap + (terrain.passability != Traversability.Impassable
                    ? " (" + "WalkSpeed".Translate(SpeedPercentString(terrain.pathCost)) + t + ")"
                    : null);
            }
            catch
            {
                // Not really important, just don't want the exception going and
                // breaking something important.
            }
            
        }

        static string SpeedPercentString(float extraPathTicks)
        {
            return (13f / (extraPathTicks + 13f)).ToStringPercent();
        }

        static string GetCorrectFertility(IntVec3 cell, AddFertileTerrainDef def)
        {
            float realFert = Find.CurrentMap.fertilityGrid.FertilityAt(cell);
            return realFert.ToStringPercent();
        }
    }
}
