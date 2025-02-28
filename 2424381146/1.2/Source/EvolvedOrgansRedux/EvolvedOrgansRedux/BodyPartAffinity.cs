namespace EvolvedOrgansRedux {
    public class BodyPartAffinity {
        public BodyPartAffinity(string nameOfThisMod) {
            System.Collections.Generic.List<Verse.RecipeDef> RecipesToAdd = new System.Collections.Generic.List<Verse.RecipeDef>();
                string newBodyPart = "";
            Verse.RecipeDef recipe = null;
            string modOfRecipe = "";
            //Put the new RecipeDefs first into this list and add them later
            foreach (Verse.RecipeDef rd in Verse.DefDatabase<Verse.RecipeDef>.AllDefs) {
                try {
                    modOfRecipe = rd.modContentPack.Name;
                } catch {
                    modOfRecipe = "The mod can't be determined.";
                }
                newBodyPart = "Something went wrong -> Step 1";
                try {
                    newBodyPart = "Something went wrong -> Step 2";
                    //Natural body parts can't be added. It's just how the game is coded.
                    if (!rd.defName.StartsWith("EVOR_Surgery_Secondary_") && !rd.defName.StartsWith("EVOR_Surgery_Primary_") && rd.recipeUsers != null && rd.IsSurgery && rd.defaultIngredientFilter.AnyAllowedDef != null && rd.appliedOnFixedBodyParts.Count == 1 && !rd.defaultIngredientFilter.AnyAllowedDef.thingCategories.Contains(Verse.DefDatabase<Verse.ThingCategoryDef>.GetNamed("BodyPartsNatural"))) {
                        recipe = rd;
                        newBodyPart = "Something went wrong -> Step 3";
                        //There are three BodyParts that are added by Evor that I would think could use the affinity.
                        //Lower Shoulders for arms.
                        if (rd.appliedOnFixedBodyParts.Contains(Verse.DefDatabase<Verse.BodyPartDef>.GetNamed("Shoulder"))) {
                            newBodyPart = "LowerShoulder";
                            RecipesToAdd.Add(CopyRecipeDef(rd, "LowerShoulder"));
                        }
                        //Chest Cavity Left for lungs
                        else if (rd.appliedOnFixedBodyParts.Contains(Verse.DefDatabase<Verse.BodyPartDef>.GetNamed("Lung"))) {
                            newBodyPart = "BodyCavity1";
                            //Add all other lungs to the RecipeDef, except the natural one.
                            RecipesToAdd.Add(CopyRecipeDef(rd, "BodyCavity1"));
                            //Gives them the Breathing stat so they actually do something else than just filling a hole.
                            if (rd.addsHediff.stages == null) {
                                rd.addsHediff.stages = new System.Collections.Generic.List<Verse.HediffStage> { addCapMods(rd, "Breathing") };
                            } else {
                                rd.addsHediff.stages.Add(addCapMods(rd, "Breathing"));
                            }
                            //Reduces the effience by the amount of offset to get it back the base stats.
                            rd.addsHediff.addedPartProps.partEfficiency *= 0.9f;
                        }
                        //Chest Cavity Right for Hearts
                        else if (rd.appliedOnFixedBodyParts.Contains(Verse.DefDatabase<Verse.BodyPartDef>.GetNamed("Heart"))) {
                            newBodyPart = "BodyCavity2";
                            //Add all other hearts to the RecipeDef, except the natural one.
                            RecipesToAdd.Add(CopyRecipeDef(rd, "BodyCavity2"));
                            //Gives them the BloodPunping stat so they actually do something else than just filling a hole.
                            if (rd.addsHediff.stages == null) {
                                rd.addsHediff.stages = new System.Collections.Generic.List<Verse.HediffStage> { addCapMods(rd, "BloodPumping") };
                            } else {
                                rd.addsHediff.stages.Add(addCapMods(rd, "BloodPumping"));
                            }
                            //Reduces the effience by the amount of offset to get it back the base stats.
                            rd.addsHediff.addedPartProps.partEfficiency *= 0.9f;
                        }
                    }
                } catch (System.Exception e) {
                    string errorMessage = "";
                    errorMessage += "BodyPartAffinity Exception";
                    errorMessage += "\nFailed to copy recipe: " + recipe.defName;
                    errorMessage += "\nRecipe is part of mod:: " + modOfRecipe;
                    errorMessage += "\nAffected body part: " + newBodyPart;
                    errorMessage += "\nCurrently applied to these BodyParts: ";
                    foreach (Verse.BodyPartDef bpd in recipe.appliedOnFixedBodyParts) {
                        errorMessage += "\n" + bpd.defName;
                    }
                    errorMessage += "\nException: " + e;
                    Verse.Log.Error(errorMessage);
                }
            }
            //Add the RecipeDefs into the actual list the game looks up.
            foreach (Verse.RecipeDef rd in RecipesToAdd) {
                    Verse.DefDatabase<Verse.RecipeDef>.Add(rd);
                    Verse.DefDatabase<Verse.RecipeDef>.GetNamed(rd.defName).ResolveReferences();
                }
        }

        private Verse.HediffStage addCapMods(Verse.RecipeDef rd, string name) {
            Verse.HediffStage hs = new Verse.HediffStage();
                Verse.PawnCapacityModifier pwm = new Verse.PawnCapacityModifier();
                pwm.capacity = Verse.DefDatabase<Verse.PawnCapacityDef>.GetNamed(name);
                pwm.offset = rd.addsHediff.addedPartProps.partEfficiency * 0.1f;
                hs.capMods = new System.Collections.Generic.List<Verse.PawnCapacityModifier> { pwm };
            return hs;
        }
        //Copy the Surgery Recipe the basegame or mod uses to install the bodypart into their usual appliedOnFixedBodyParts.
        //Change the appliedOnFixedBodyParts it gets installed into and add the researchPrerequisite
        private Verse.RecipeDef CopyRecipeDef(Verse.RecipeDef rd, string bodyPart) {
            Verse.RecipeDef rdn = new Verse.RecipeDef();
            try {
                rdn.defName = "Evor_" + rd.defName;
                rdn.label = rd.label;
                rdn.description = rd.description;
                rdn.workerClass = rd.workerClass;
                rdn.jobString = rd.jobString;
                rdn.workAmount = rd.workAmount;
                rdn.skillRequirements = rd.skillRequirements;
                rdn.recipeUsers = rd.recipeUsers;
                rdn.ingredients = rd.ingredients;
                rdn.fixedIngredientFilter = rd.fixedIngredientFilter;
                rdn.appliedOnFixedBodyParts = new System.Collections.Generic.List<Verse.BodyPartDef> { Verse.DefDatabase<Verse.BodyPartDef>.GetNamed(bodyPart) };
                rdn.addsHediff = rd.addsHediff;
                rdn.researchPrerequisite = Verse.DefDatabase<Verse.ResearchProjectDef>.GetNamed("EVOR_Research_Limbs3");
                rdn.descriptionHyperlinks = rd.descriptionHyperlinks;
            } catch (System.Exception e) {
                Verse.Log.Error("EvolvedOrgansRedux error: CopyRecipeDef" + 
                    "\nProblematic recipe: " + rd.defName +
                    e);
            }
            return rdn;
        }
    }
}
