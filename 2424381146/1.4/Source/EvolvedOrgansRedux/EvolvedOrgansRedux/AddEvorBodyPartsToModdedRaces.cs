using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace EvolvedOrgansRedux {
    [StaticConstructorOnStartup]
    public static class AddEvorBodyPartsToModdedRaces {
        static AddEvorBodyPartsToModdedRaces() {
            bool meatRecipeWasNotRemovedForCompatibilityReasons = DefDatabase<RecipeDef>.GetNamedSilentFail("EVOR_Production_Protein_Humie") != null;
            //Go through all the ThingDefs to find out which humanoid races exist
            foreach (ThingDef def in DefDatabase<ThingDef>.AllDefs) {
                //If the race is Humanoid but not the base race
                if (def.race?.Humanlike == true && def.defName != "Human" &&
                    (def.modContentPack == null || !Singleton.Instance.ForbiddenMods.Contains(def.modContentPack.Name))) {
                    try {
                        AddBodyParts(def.race);
                        if (def.race.body.modExtensions == null)
                            def.race.body.modExtensions = new();
                        def.race.body.modExtensions.Add(new EVOR_DefModExtension_BodyDefToModify());
                        if (meatRecipeWasNotRemovedForCompatibilityReasons) {
                            AddMeatToRecipesFromModdedRace(def.race);
                            if (Prefs.LogVerbose)
                                Log.Message("EvolvedOrgansRedux: Meat of " + def.defName + " was added to the protein recipe.");
                        }
                    } catch (System.Exception e) {
                        Log.Error(
                            "EvolvedOrgansRedux : AddDefsForEachModdedRace" +
                            "\nProblematic race: " + def.defName +
                            "\nError: " + e);
                    }
                }
            }
        }
        private static void AddBodyParts(RaceProperties raceProperties) {
            Singleton.Instance.FillListsOfBodyPartTagsToRecalculate();
            foreach (BodyPartRecord bodyPartRecord in DefDatabase<BodyDef>.GetNamed("Human").AllParts
                .Where(e => e.def.modContentPack == Singleton.Instance.Settings.Mod.Content)) {
                bool alreadyHasSimilarPart = raceProperties.body.AllParts.Exists(e => e.def.defName.ToLower().Contains(bodyPartRecord.def.defName.ToLower()));
                if (alreadyHasSimilarPart) {
                    foreach (BodyPartRecord bpr in raceProperties.body.AllParts.Where(e => e.def.defName.ToLower().Contains(bodyPartRecord.def.defName.ToLower())).ToList()) {
                        if (bpr.def.tags == null) bpr.def.tags = new();
                        bpr.def.tags.Remove(BodyPartTagDefOf.MovingLimbSegment);
                        bpr.def.tags.Remove(BodyPartTagDefOf.ManipulationLimbSegment);
                        foreach (BodyPartTagDef tag in bodyPartRecord.def.tags)
                            bpr.def.tags.AddDistinct(tag);
                    }
                } else if (bodyPartRecord.parent.def == BodyPartDefOf.Torso || bodyPartRecord.parent.def == BodyPartDefOf.Head) {
                    BodyPartRecord parent = raceProperties.body.AllParts.Find(e => e.Label == bodyPartRecord.parent.Label);
                    AddParts(bodyPartRecord, raceProperties, parent);
                }
            }
            DefDatabase<BodyDef>.GetNamed(raceProperties.body.defName).AllParts.Clear();
            DefDatabase<BodyDef>.GetNamed(raceProperties.body.defName).ResolveReferences();
        }
        private static void AddParts(BodyPartRecord bodyPartRecord, RaceProperties raceProperties, BodyPartRecord parent) {
            BodyPartRecord newBodyPart = CopyBodyPartRecord(bodyPartRecord, raceProperties, parent);
            parent.parts.Add(newBodyPart);
            if (bodyPartRecord.GetDirectChildParts().Count() > 0)
                foreach (BodyPartRecord bpr in bodyPartRecord.GetDirectChildParts())
                    AddParts(bpr, raceProperties, newBodyPart);
        }
        private static BodyPartRecord CopyBodyPartRecord(BodyPartRecord bpr, RaceProperties raceProperties, BodyPartRecord parent) {
            return new BodyPartRecord {
                body = raceProperties.body,
                coverage = bpr.coverage,
                coverageAbs = bpr.coverageAbs,
                coverageAbsWithChildren = bpr.coverageAbsWithChildren,
                customLabel = bpr.customLabel,
                def = bpr.def,
                depth = bpr.depth,
                groups = bpr.groups,
                height = bpr.height,
                parent = parent,
                untranslatedCustomLabel = bpr.untranslatedCustomLabel
            };
        }
        private static void AddMeatToRecipesFromModdedRace(RaceProperties raceProperties) {
            if (!ModLister.HasActiveModWithName("Questionable Ethics Enhanced")) {
                DefDatabase<RecipeDef>.GetNamed("EVOR_Production_Protein_Humie").fixedIngredientFilter.SetAllow(raceProperties.meatDef, true);
                DefDatabase<RecipeDef>.GetNamed("EVOR_Production_Protein_Humie").ClearCachedData();
                DefDatabase<RecipeDef>.GetNamed("EVOR_Production_Protein_Humie").ResolveReferences();
                DefDatabase<RecipeDef>.GetNamed("EVOR_Production_Protein_Humie_Bulk").fixedIngredientFilter.SetAllow(raceProperties.meatDef, true);
                DefDatabase<RecipeDef>.GetNamed("EVOR_Production_Protein_Humie_Bulk").ClearCachedData();
                DefDatabase<RecipeDef>.GetNamed("EVOR_Production_Protein_Humie_Bulk").ResolveReferences();
            }
        }
    }
}
