using HarmonyLib;
using RimWorld;
using Verse;

namespace TerrainOverhaul
{
    [HarmonyPatch(typeof(FertilityGrid), "CalculateFertilityAt")]
    class FertilityPatch
    {
        private static TO_Settings settings;

        private static void Postfix(IntVec3 loc, Map ___map, ref float __result)
        {
            var topTerrain = ___map.terrainGrid.TerrainAt(loc);
            if (topTerrain == null)
                return;

            if (!(topTerrain is AddFertileTerrainDef def))
                return;

            Thing edifice = loc.GetEdifice(___map);
            if (edifice != null && edifice.def.AffectsFertility)
            {
                __result = edifice.def.fertility;
                return;
            }

            var below = ___map.terrainGrid.UnderTerrainAt(loc);
            float baseFert;
            if (below != null)
                baseFert = below.fertility;
            else
                baseFert = def.fertility;

            float scale = 1f;
            if (settings == null)
                settings = TerrainOverhaulMod.Instance?.GetSettings<TO_Settings>();
            if (settings != null)
                scale = settings.FarmlandFertilityScale;

            __result = baseFert + def.addFertility * scale;
        }
    }
}
