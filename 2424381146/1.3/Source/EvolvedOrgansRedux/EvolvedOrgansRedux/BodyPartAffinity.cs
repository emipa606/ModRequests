namespace EvolvedOrgansRedux {
    public class BodyPartAffinity {
        enum BodyPart {
            None,
            Shoulder,
            Lung,
            Heart
        }
        System.Collections.Generic.List<Verse.RecipeDef> RemoveImplantRecipesToAdd = new System.Collections.Generic.List<Verse.RecipeDef>();
        public BodyPartAffinity(string nameOfThisMod) {
            string newBodyPart = "";
            Verse.RecipeDef recipe = null;
            string modOfRecipe = "";
            BodyPart bodypart;
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
                    if (rd.appliedOnFixedBodyParts.Contains(Verse.DefDatabase<Verse.BodyPartDef>.GetNamed("Shoulder")))
                        bodypart = BodyPart.Shoulder;
                    else if (rd.appliedOnFixedBodyParts.Contains(DefOf.Lung))
                        bodypart = BodyPart.Lung;
                    else if (rd.appliedOnFixedBodyParts.Contains(DefOf.Heart))
                        bodypart = BodyPart.Heart;
                    else
                        bodypart = BodyPart.None;
                    //Natural body parts can't be added. It's just how the game is coded.
                    if (bodypart != 0 && rd.workerClass.FullName == "Verse.Recipe_InstallArtificialBodyPart" && rd.modContentPack.Name != nameOfThisMod && rd.recipeUsers != null && rd.IsSurgery && rd.defaultIngredientFilter.AnyAllowedDef != null && rd.appliedOnFixedBodyParts.Count == 1 && !rd.defaultIngredientFilter.AnyAllowedDef.thingCategories.Contains(Verse.DefDatabase<Verse.ThingCategoryDef>.GetNamed("BodyPartsNatural"))) {
                        recipe = rd;
                        newBodyPart = "Something went wrong -> Step 3";
                        switch (bodypart) {
                            case BodyPart.Shoulder:
                                newBodyPart = "LowerShoulder";
                                Singleton.Instance.AddLowershouldersToRecipedef.Add(rd);
                                makeNewUninstallIDK(rd);
                                break;
                            case BodyPart.Lung:
                                newBodyPart = "BodyCavity1";
                                //Add all other lungs to the RecipeDef, except the natural one.
                                Singleton.Instance.AddLeftchestcavityToRecipedef.Add(rd);
                                //Gives them the Breathing stat so they actually do something else than just filling a hole.
                                if (rd.addsHediff.stages == null) {
                                    rd.addsHediff.stages = new System.Collections.Generic.List<Verse.HediffStage> { addCapMods(rd, "Breathing") };
                                } else {
                                    rd.addsHediff.stages.Add(addCapMods(rd, "Breathing"));
                                }
                                //Reduces the effience by the amount of offset to get it back the base stats.
                                rd.addsHediff.addedPartProps.partEfficiency *= 0.9f;
                                break;
                            case BodyPart.Heart:
                                newBodyPart = "BodyCavity2";
                                //Add all other hearts to the RecipeDef, except the natural one.
                                Singleton.Instance.AddRightchestcavityToRecipedef.Add(rd);
                                //Gives them the BloodPunping stat so they actually do something else than just filling a hole.
                                if (rd.addsHediff.stages == null) {
                                    rd.addsHediff.stages = new System.Collections.Generic.List<Verse.HediffStage> { addCapMods(rd, "BloodPumping") };
                                } else {
                                    rd.addsHediff.stages.Add(addCapMods(rd, "BloodPumping"));
                                }
                                //Reduces the effience by the amount of offset to get it back the base stats.
                                rd.addsHediff.addedPartProps.partEfficiency *= 0.9f;
                                break;
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
            foreach (Verse.RecipeDef recipeDef in RemoveImplantRecipesToAdd) {
                Verse.DefDatabase<Verse.RecipeDef>.Add(recipeDef);
            }
            Verse.DefDatabase<Verse.RecipeDef>.ResolveAllReferences();
        }

        private Verse.HediffStage addCapMods(Verse.RecipeDef rd, string name) {
            Verse.HediffStage hs = new Verse.HediffStage();
                Verse.PawnCapacityModifier pwm = new Verse.PawnCapacityModifier();
                pwm.capacity = Verse.DefDatabase<Verse.PawnCapacityDef>.GetNamed(name);
                pwm.offset = rd.addsHediff.addedPartProps.partEfficiency * 0.1f;
                hs.capMods = new System.Collections.Generic.List<Verse.PawnCapacityModifier> { pwm };
            return hs;
        }
        private void makeNewUninstallIDK(Verse.RecipeDef rd) {
            Verse.RecipeDef template = Verse.DefDatabase<Verse.RecipeDef>.GetNamed("EVOR_SurgeryRemove_Appendage_ExtradextrousArm");
            Verse.RecipeDef recipeDef = new Verse.RecipeDef();
            recipeDef.defName = "EVOR_SurgeryRemove_" + rd.defName;
            try {
                recipeDef.label = "Remove " + rd.descriptionHyperlinks.Find(e => e.def.GetType() == typeof(Verse.ThingDef)).def.label;
            } catch {
                recipeDef.label = "Remove";
            }
            recipeDef.description = rd.description;
            recipeDef.jobString = rd.jobString;
            recipeDef.effectWorking = template.effectWorking;
            recipeDef.soundWorking = template.soundWorking;
            recipeDef.workSpeedStat = template.workSpeedStat;
            recipeDef.workSkill = template.workSkill;
            recipeDef.workSkillLearnFactor = template.workSkillLearnFactor;
            recipeDef.workAmount = template.workAmount;
            recipeDef.isViolation = template.isViolation;
            recipeDef.skillRequirements = template.skillRequirements;
            recipeDef.recipeUsers = rd.recipeUsers;
            recipeDef.ingredients = template.ingredients;
            recipeDef.fixedIngredientFilter = template.fixedIngredientFilter;
            recipeDef.workerClass = template.workerClass;
            recipeDef.descriptionHyperlinks = rd.descriptionHyperlinks;
            recipeDef.removesHediff = rd.addsHediff;
            RemoveImplantRecipesToAdd.Add(recipeDef);
        }
    }
}
