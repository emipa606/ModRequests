using System.Linq;
using Verse;

namespace EvolvedOrgansRedux {
    public static class BodyPartAffinity {
        public static void AddEVORBodyPartsToRecipes(BodyPartDef BodyPartsToCopyFrom, BodyPartDef BodyPartsToCopyTo) {
            foreach (RecipeDef recipeDef in DefDatabase<RecipeDef>.AllDefs.Where(x => x.appliedOnFixedBodyParts.Contains(BodyPartsToCopyFrom))) {
                recipeDef.appliedOnFixedBodyParts.Add(BodyPartsToCopyTo);
                if (Prefs.LogVerbose)
                    Log.Message("EvolvedOrgansRedux -> BodyPartAffinity.AddEVORBodyPartsToRecipes: RecipeDef: " + recipeDef.defName +
                        " BodyPartsToCopyFrom: " + BodyPartsToCopyFrom + " BodyPartsToCopyTo: " + BodyPartsToCopyTo);
            }
            DefDatabase<RecipeDef>.ResolveAllReferences();
        }
        public static void AddCapModsToHediffs(BodyPartDef BodyPartToModify, PawnCapacityDef PawnCapacityDefToAdd, float CapacityDefModifier) {
            foreach (RecipeDef recipeDef in DefDatabase<RecipeDef>.AllDefs.Where(x => x.appliedOnFixedBodyParts.Contains(BodyPartToModify))) {
                if (recipeDef.addsHediff != null && recipeDef.addsHediff.addedPartProps != null) {
                    float originalPartEfficiency = recipeDef.addsHediff.addedPartProps.partEfficiency;
                    if (recipeDef.addsHediff.stages == null) recipeDef.addsHediff.stages = new() { new HediffStage() };
                    recipeDef.addsHediff.stages[0].capMods.Add(new() {
                        capacity = PawnCapacityDefToAdd,
                        offset = originalPartEfficiency * CapacityDefModifier
                    });
                    recipeDef.addsHediff.addedPartProps.partEfficiency -= originalPartEfficiency * CapacityDefModifier;
                }
            }
        }
    }
}
