namespace EvolvedOrgansRedux {
    public class AddDefsForEachModdedRace {
        public AddDefsForEachModdedRace(string nameOfThisMod) {
            //List of mods that shouldn't have any meaty parts.
            bool meatRecipeWasNotRemovedForCompatibilityReasons = Verse.DefDatabase<Verse.RecipeDef>.GetNamedSilentFail("EVOR_Production_Protein_Humie") != null;
            //Go through all the ThingDefs to find out which races exist
            foreach (Verse.ThingDef def in Verse.DefDatabase<Verse.ThingDef>.AllDefs) {
                //Some mods remove this recipe.
                //If the race is Humanoid but not the base race of this game
                if (def.race?.Humanlike == true && !def.defName.Equals("Human") && (def.modContentPack == null || !Singleton.Instance.forbiddenMods.Contains(def.modContentPack.Name))) { //Human stuff will just be solved with the XMLs.
                    try {
                        new AddBodyParts(def.race, nameOfThisMod);
                        if (meatRecipeWasNotRemovedForCompatibilityReasons) {
                            new AddMeatToRecipesFromModdedRace(def.race);
                            Verse.Log.Message("EvolvedOrgansRedux: Meat of " + def.defName + " was added to the protein recipe.");
                        }
                    } catch (System.Exception e) {
                        Verse.Log.Error(
                            "EvolvedOrgansRedux : AddDefsForEachModdedRace" +
                            "\nProblematic race: " + def.defName +
                            "\nError: " + e
                            );
                    }
                }
            }
        }
    }
}
