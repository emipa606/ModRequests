using RimWorld;
using System.Collections.Generic;
using Verse;

namespace TerrainOverhaul
{
    class AddFertileTerrainDef : TerrainDef
    {
        public float addFertility = 0f;

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
        {
            foreach (StatDrawEntry statDrawEntry in base.SpecialDisplayStats(req))
            {
                yield return statDrawEntry;
            }

            yield return new StatDrawEntry(StatCategoryDefOf.Basics,
                "LTO.AddFertilityLabel".Translate(),
                $"{(addFertility >= 0f ? "+" : "")}{addFertility * 100f:F1}%",
                "LTO.AddFertilityDescription".Translate(),
                1000);
        }
    }
}
