using RimWorld;
using Verse;

namespace MoreArchotechGarbage
{
    [StaticConstructorOnStartup]
    public static class Startup
    {
        static Startup()
        {
            ApplySettings();
        }
        public static void ApplySettings()
        {
            MAG_DefOf.MAG_ArchoGeneExtractor.GetCompProperties<CompProperties_Power>().basePowerConsumption
                = MoreArchotechGarbageSettings.archotechGeneExtractorPower;
            if (MAG_DefOf.MAG_ArchotechGeneRipper != null)
            {
                MAG_DefOf.MAG_ArchotechGeneRipper.GetCompProperties<CompProperties_Power>().basePowerConsumption
    = MoreArchotechGarbageSettings.archotechGeneRipperPower;
            }
            MAG_DefOf.MAG_ArchiteGenepackAssembler.GetCompProperties<CompProperties_Spawner>().spawnIntervalRange
    = new IntRange(MoreArchotechGarbageSettings.architeGenepackCountDownDuration, MoreArchotechGarbageSettings.architeGenepackCountDownDuration);
            
            foreach (var ingredient in MAG_DefOf.MAG_CraftArchiteCapsules.ingredients)
            {
                if (ingredient.FixedIngredient == MAG_DefOf.archotechmatteraddingsomecraptoavoidproblems)
                {
                    ingredient.count = MoreArchotechGarbageSettings.architeCapsuleMatterCraftCost;
                }
                else if (ingredient.FixedIngredient == MAG_DefOf.ArchotechScrap)
                {
                    ingredient.count = MoreArchotechGarbageSettings.architeCapsuleFragmentCraftCost;
                }
            }
        }
    }
}
