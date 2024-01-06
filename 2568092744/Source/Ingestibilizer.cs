using Verse;
using RimWorld;

namespace EatAnything
{
    [StaticConstructorOnStartup]
    public static class Ingestibilizer
    {
        static Ingestibilizer()
        {
            foreach (ThingDef def in DefDatabase<ThingDef>.AllDefs)
            {
                Debug.Log(def.ToString());
                if (def.ingestible == null)
                {
                    def.ingestible = new IngestibleProperties();
                    def.ingestible.ingestSound = SoundDefOf.RawMeat_Eat;
                    def.ingestible.preferability = FoodPreferability.NeverForNutrition;
                    def.ingestible.foodType = FoodTypeFlags.Processed;
                    def.ingestible.parent = def;
                }
            }
        }
    }
}
