using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Verse;
using Verse.Noise;

namespace MakeAnythingCraftable
{
    [HotSwappable]
    public class RecipeDefExposable : RecipeDef, IExposable
    {
        public DefCount<ThingDef> productString;
        public string workSpeedStatString;
        public string efficiencyStatString;
        public string effectWorkingString;
        public string soundWorkingString;
        public string workSkillString;
        public string unfinishedThingDefString;
        public string workerType;
        public string ingredientValueGetterType;
        public List<string> recipeUsersString = new List<string>();
        public List<string> researchPrerequisitesString = new List<string>();
        public List<SkillRequirementString> skillRequirementsString = new List<SkillRequirementString>();
        public List<IngredientCountExposable> ingredientsCount = new List<IngredientCountExposable>();
        public List<string> disallowedIngredients = new List<string>();
        public bool copied;
        public RecipeDefExposable()
        {

        }
        public RecipeDefExposable(RecipeDef source)
        {
            source.ResolveReferences();
            this.defName = source.defName;
            this.label = source.label;
            this.description = source.description;
            this.jobString = source.jobString;
            this.workAmount = source.WorkAmountTotal(null);
            this.allowMixingIngredients = source.allowMixingIngredients;
            
            recipeUsersString = source.AllRecipeUsers.Select(x => x.defName).ToList();
            if (!source.researchPrerequisites.NullOrEmpty())
            {
                researchPrerequisitesString = source.researchPrerequisites.Select(x => x.defName).ToList();
            }
            if (source.researchPrerequisite != null)
            {
                researchPrerequisitesString.Add(source.researchPrerequisite.defName);
            }
            if (!source.skillRequirements.NullOrEmpty())
            {
                foreach (var sk in source.skillRequirements)
                {
                    skillRequirementsString.Add(new SkillRequirementString
                    {
                        skill = sk.skill.defName,
                        minLevel = sk.minLevel,
                    });
                }
            }
            workerType = GenTypes.GetTypeNameWithoutIgnoredNamespaces(source.workerClass);
            ingredientValueGetterType = GenTypes.GetTypeNameWithoutIgnoredNamespaces(source.ingredientValueGetterClass);

            workSpeedStatString = source.workSpeedStat?.defName;
            efficiencyStatString = source.efficiencyStat?.defName;
            effectWorkingString = source.effectWorking?.defName;
            soundWorkingString = source.soundWorking?.defName;
            workSkillString = source.workSkill?.defName;
            unfinishedThingDefString = source.unfinishedThingDef?.defName;
            var product = source.products[0].thingDef;
            productString = new DefCount<ThingDef>(product.defName, source.products[0].count);
            if (!source.ingredients.NullOrEmpty())
            {
                foreach (var ingredient in source.ingredients)
                {
                    var ingredientCategory = new IngredientCountExposable { count = ingredient.count };
                    ingredientsCount.Add(ingredientCategory);
                    if (ingredient.filter.thingDefs != null)
                    {
                        foreach (var def in ingredient.filter.thingDefs)
                        {
                            ingredientCategory.thingDefs.Add(def.defName);
                        }
                    }
                    
                    if (ingredient.filter.allowedDefs != null) 
                    {
                        foreach (var def in ingredient.filter.allowedDefs)
                        {
                            if (!ingredientCategory.thingDefs.Contains(def.defName) && product.CostList != null
                                && product.CostList.Any(x => x.thingDef == def))
                            {
                                ingredientCategory.thingDefs.Add(def.defName);
                            }
                        }
                    }

                    if (ingredient.filter.categories != null)
                    {
                        foreach (var def in ingredient.filter.categories)
                        {
                            ingredientCategory.categories.Add(def);
                        }
                    }
                }
            }

            copied = true;

            if (source is RecipeDefExposable exposable)
            {
                this.productString = exposable.productString;
                this.workSpeedStatString = exposable.workSpeedStatString;
                this.efficiencyStatString = exposable.efficiencyStatString;
                this.effectWorkingString = exposable.effectWorkingString;
                this.soundWorkingString = exposable.soundWorkingString;
                this.workSkillString = exposable.workSkillString;
                this.unfinishedThingDefString = exposable.unfinishedThingDefString;
                this.workerType = exposable.workerType;
                this.ingredientValueGetterType = exposable.ingredientValueGetterType;
                this.recipeUsersString = exposable.recipeUsersString;
                this.researchPrerequisitesString = exposable.researchPrerequisitesString;
                this.skillRequirementsString = exposable.skillRequirementsString;
                this.ingredientsCount = exposable.ingredientsCount;
                this.disallowedIngredients = exposable.disallowedIngredients;
            }
        }

        public void ModifyRecipe(RecipeDef recipeDef)
        {
            recipeDef.label = this.label;
            recipeDef.description = this.description;
            recipeDef.recipeUsers = this.recipeUsers;
            recipeDef.researchPrerequisite = null;
            recipeDef.researchPrerequisites = this.researchPrerequisites;
            recipeDef.effectWorking = this.effectWorking;
            recipeDef.soundWorking = this.soundWorking;
            recipeDef.workSpeedStat = this.workSpeedStat;
            recipeDef.efficiencyStat = this.efficiencyStat;
            recipeDef.workSkill = this.workSkill;
            recipeDef.unfinishedThingDef = this.unfinishedThingDef;
            recipeDef.skillRequirements = this.skillRequirements;
            recipeDef.products = this.products;
            recipeDef.adjustedCount = this.adjustedCount;
            recipeDef.fixedIngredientFilter = this.fixedIngredientFilter;
            recipeDef.defaultIngredientFilter = this.defaultIngredientFilter;
            recipeDef.ingredients = this.ingredients;
            recipeDef.workAmount = this.workAmount;
            recipeDef.allowMixingIngredients = this.allowMixingIngredients;
        }

        public override void ResolveReferences()
        {
            if (!workerType.NullOrEmpty())
            {
                workerClass = GenTypes.GetTypeInAnyAssembly(workerType);
            }
            if (!ingredientValueGetterType.NullOrEmpty())
            {
                ingredientValueGetterClass = GenTypes.GetTypeInAnyAssembly(ingredientValueGetterType);
            }
            recipeUsers = new List<ThingDef>();
            foreach (var defName in recipeUsersString)
            {
                var def = DefDatabase<ThingDef>.GetNamedSilentFail(defName);
                if (def != null)
                {
                    if (!recipeUsers.Contains(def))
                    {
                        recipeUsers.Add(def);
                    }
                }
            }

            if (!researchPrerequisitesString.NullOrEmpty())
            {
                researchPrerequisites = new List<ResearchProjectDef>();
                foreach (var defName in researchPrerequisitesString)
                {
                    var def = DefDatabase<ResearchProjectDef>.GetNamedSilentFail(defName);
                    if (def != null)
                    {
                        if (!researchPrerequisites.Contains(def))
                        {
                            researchPrerequisites.Add(def);
                        }
                    }
                }
            }
            
            if (!effectWorkingString.NullOrEmpty())
            {
                effectWorking = DefDatabase<EffecterDef>.GetNamedSilentFail(effectWorkingString);
            }
            
            if (!soundWorkingString.NullOrEmpty())
            {
                soundWorking = DefDatabase<SoundDef>.GetNamedSilentFail(soundWorkingString);
            }
            
            if (!workSpeedStatString.NullOrEmpty())
            {
                workSpeedStat = DefDatabase<StatDef>.GetNamedSilentFail(workSpeedStatString);
            }
            
            if (!efficiencyStatString.NullOrEmpty())
            {
                efficiencyStat = DefDatabase<StatDef>.GetNamedSilentFail(efficiencyStatString);
            }
            
            if (!workSkillString.NullOrEmpty())
            {
                workSkill = DefDatabase<SkillDef>.GetNamedSilentFail(workSkillString);
            }
            
            if (!unfinishedThingDefString.NullOrEmpty())
            {
                unfinishedThingDef = DefDatabase<ThingDef>.GetNamedSilentFail(unfinishedThingDefString);
            }
            if (!skillRequirementsString.NullOrEmpty())
            {
                skillRequirements = new List<SkillRequirement>();
                foreach (var skillRequirement in skillRequirementsString)
                {
                    if (!skillRequirements.Any(x => x.skill.defName == skillRequirement.skill))
                    {
                        skillRequirements.Add(new SkillRequirement
                        {
                            minLevel = skillRequirement.minLevel,
                            skill = DefDatabase<SkillDef>.GetNamed(skillRequirement.skill)
                        });
                    }
                }
            }

            products = new List<ThingDefCountClass>();
            var product = new ThingDefCountClass(DefDatabase<ThingDef>.GetNamed(productString.defName), productString.count);
            products.Add(product);
            this.adjustedCount = product.count;
            this.fixedIngredientFilter = new ThingFilter();
            if (ingredientsCount != null)
            {
                this.ingredients = new List<IngredientCount>();
                foreach (var ingredientCategory in ingredientsCount)
                {
                    IngredientCount ingredientCount = new IngredientCount();
                    ingredientCount.SetBaseCount(ingredientCategory.count);
                    if (ingredientCategory.categories != null)
                    {
                        foreach (var item in ingredientCategory.categories)
                        {
                            var categoryDef = DefDatabase<ThingCategoryDef>.GetNamedSilentFail(item);
                            if (categoryDef != null)
                            {
                                fixedIngredientFilter.SetAllow(categoryDef, true);
                                ingredientCount.filter.SetAllow(categoryDef, allow: true);
                                foreach (var def in categoryDef.DescendantThingDefs)
                                {
                                    if (disallowedIngredients?.Contains(def.defName) ?? false)
                                    {
                                        ingredientCount.filter.SetAllow(def, false);
                                        fixedIngredientFilter.SetAllow(def, false);
                                    }
                                }
                            }
                        }
                    }
                    if (ingredientCategory.thingDefs != null)
                    {
                        foreach (var defName in ingredientCategory.thingDefs)
                        {
                            var def = DefDatabase<ThingDef>.GetNamedSilentFail(defName);
                            if (def != null)
                            {
                                ingredientCount.SetBaseCount(def.smallVolume ? ingredientCategory.count / 10f : ingredientCategory.count);
                                ingredientCount.filter.SetAllow(def, allow: true);
                                this.fixedIngredientFilter.SetAllow(def, allow: true);
                            }
                        }
                    }
                    this.ingredients.Add(ingredientCount);
                }
            }
            this.fixedIngredientFilter.ResolveReferences();
            base.ResolveReferences();
        }
        public void ExposeData()
        {
            Scribe_Values.Look(ref defName, "defName");
            Scribe_Values.Look(ref label, "label");
            Scribe_Values.Look(ref workerType, "workerType");
            Scribe_Values.Look(ref ingredientValueGetterType, "ingredientValueGetterType");
            Scribe_Values.Look(ref description, "description");
            Scribe_Values.Look(ref jobString, "jobString");
            Scribe_Values.Look(ref workAmount, "workAmount");
            Scribe_Values.Look(ref effectWorkingString, "effectWorkingString");
            Scribe_Values.Look(ref soundWorkingString, "soundWorkingString");
            Scribe_Values.Look(ref workSpeedStatString, "workSpeedStatString");
            Scribe_Values.Look(ref efficiencyStatString, "efficiencyStatString");
            Scribe_Values.Look(ref workSkillString, "workSkillString");
            Scribe_Values.Look(ref unfinishedThingDefString, "unfinishedThingDefString");
            Scribe_Collections.Look(ref recipeUsersString, "recipeUsersString", LookMode.Value);
            Scribe_Collections.Look(ref researchPrerequisitesString, "researchPrerequisitesString", LookMode.Value);
            Scribe_Collections.Look(ref skillRequirementsString, "skillRequirementsString", LookMode.Deep);
            Scribe_Collections.Look(ref ingredientsCount, "ingredientsCount", LookMode.Deep);
            Scribe_Collections.Look(ref disallowedIngredients, "disallowedIngredients", LookMode.Value);
            Scribe_Deep.Look(ref productString, "productString");
            Scribe_Values.Look(ref allowMixingIngredients, "allowMixingIngredients");
            Scribe_Values.Look(ref copied, "copied");
        }
        public override string ToString()
        {
            var text = base.ToString();
            if (this.ingredients != null)
            {
                text += "\nIngredients: ";
                foreach (var ingredient in this.ingredients)
                {
                    text += "\n" + ingredient.ToString();
                }
            }
            if (this.ingredientsCount != null)
            {
                text += "\nIngredientsCount: ";
                foreach (var ingredient in this.ingredientsCount)
                {
                    text += "\n" + ingredient.count + " - " + ingredient.categories?.Count;
                }
            }
            return text;
        }
    }
}
