using RimWorld;
using System.Collections.Generic;
using Verse;

namespace RealisticMeatScaling
{
    [StaticConstructorOnStartup]
    public static class MeatStatAdjuster
    {
        private const float MeatUnitNutrition = 0.15f;
        static MeatStatAdjuster()
        {
            foreach (ThingDef def in DefDatabase<ThingDef>.AllDefs)
            {
                if (def.ingestible?.foodType.HasFlag(FoodTypeFlags.Meat) == true)
                {
                    if (def.statBases == null) def.statBases = new List<StatModifier>();
                    SetStat(def, StatDefOf.Mass, 0.18f);
                    SetStat(def, StatDefOf.Nutrition, MeatUnitNutrition);
                }
            }
        }
        private static void SetStat(ThingDef def, StatDef stat, float value)
        {
            StatModifier statMod = def.statBases.Find(s => s.stat == stat);
            if (statMod != null) statMod.value = value;
            else def.statBases.Add(new StatModifier { stat = stat, value = value });
        }
    }
}