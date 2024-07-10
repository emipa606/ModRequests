using HarmonyLib;
using RimWorld;
using Verse;

namespace TerrainOverhaul
{
    [HarmonyPatch(typeof(Frame), "CompleteConstruction")]
    public static class FarmlandXpPatch
    {
        private static TO_Settings settings;

        static void Prefix(Frame __instance, Pawn worker)
        {
            if (worker == null)
                return;
            var def = __instance.def;
            if (def?.entityDefToBuild == null)
                return;
            if (!(def.entityDefToBuild is TerrainDef td))
                return;

            if (td.defName.StartsWith("FarmLand"))
            {
                // Assume that it's farmland from TerrainOverhaul...

                float multi = 1f;
                if (settings == null)
                    settings = TerrainOverhaulMod.Instance?.GetSettings<TO_Settings>();
                if (settings != null)
                    multi = settings.FarmlandXPScale;

                float xp = 20 * multi;
                worker.skills?.GetSkill(SkillDefOf.Plants)?.Learn(xp);
            }
        }
    }
}
