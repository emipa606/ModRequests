using RimWorld;
using HarmonyLib;
using Verse;
using System.Collections.Generic;

namespace HDream
{
    public class FoodWishDef : IngestibleWishDef
    {
        public FloatRange? nutrimentRange;
        public FloatRange? preferabilityRange;
        public bool shouldMatchBothPreferabilityAndJoy = false;
        public bool cannibalisme = false;

        protected override bool CheckMatch(ThingDef ingestible)
        {
            return ingestible.ingestible.JoyKind == JoyKindDefOf.Gluttonous
                && (nutrimentRange?.Includes(ingestible.ingestible.CachedNutrition) ?? true)
                && ((shouldMatchBothPreferabilityAndJoy || preferabilityRange == null || joyRange == null)
                    ? preferabilityRange?.Includes((float)ingestible.ingestible.preferability) ?? true && base.CheckMatch(ingestible)
                    : preferabilityRange?.Includes((float)ingestible.ingestible.preferability) ?? true || base.CheckMatch(ingestible));
        }
    }
}
