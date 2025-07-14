using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using VanillaSocialInteractionsExpanded;
using Verse;

namespace VSIERationalTraitDevelopment
{
    [HarmonyPatch(typeof(PregnancyUtility), "ApplyBirthOutcome")]
    public static class PregnancyUtility_ApplyBirthOutcome_Patch
    {
        public static void Postfix(Thing __result, OutcomeChance outcome, float quality, Precept_Ritual ritual, List<GeneDef> genes,
            Pawn geneticMother, Thing birtherThing, Pawn father = null, Pawn doctor = null, LordJob_Ritual lordJobRitual = null, RitualRoleAssignments assignments = null)
        {
            if (__result is Pawn)
            {
                if (geneticMother != null)
                {
                    VSIE_Utils.TryDevelopNewTrait(geneticMother, VSIE_Extra_DefOf.VSIE_BecameParentTraitChange.key, 0.1f);
                }
                if (father != null)
                {
                    VSIE_Utils.TryDevelopNewTrait(father, VSIE_Extra_DefOf.VSIE_BecameParentTraitChange.key, 0.1f);
                }
            }
            else if (__result is Corpse stillborn)
            {
                if (geneticMother != null)
                {
                    VSIE_Utils.TryDevelopNewTrait(geneticMother, VSIE_Extra_DefOf.VSIE_BecameParentStillbornTraitChange.key, 0.1f);
                }
                if (father != null)
                {
                    VSIE_Utils.TryDevelopNewTrait(father, VSIE_Extra_DefOf.VSIE_BecameParentStillbornTraitChange.key, 0.1f);
                }
            }
        }
    }

}
