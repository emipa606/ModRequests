/* For v1.2
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Entomophagy
{

        [HarmonyPatch(typeof(FoodUtility), "ThoughtsFromIngesting")]
        private static void Postfix(ref List<ThoughtDef> __result, Pawn ingester)
        {
            //Entomophagous
            if (ingester.story.traits.HasTrait(TraitDef.Named("Entomophagous")))
            {
                if (__result.Contains(ThoughtDefOf.AteInsectMeatAsIngredient))
                {
                    __result.Remove(ThoughtDefOf.AteInsectMeatAsIngredient);
                    __result.Add(ThoughtDef.Named("AteInsectMeatAsEntomophagous"));
                }
                if (__result.Contains(ThoughtDefOf.AteInsectMeatDirect))
                {
                    __result.Remove(ThoughtDefOf.AteInsectMeatDirect);
                    __result.Add(ThoughtDef.Named("AteInsectMeatAsEntomophagous"));
                }
            }
        }
    }
}
        */
