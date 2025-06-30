using System.Collections.Generic;
using Verse;
using RimWorld;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using System.Diagnostics;
using System;

namespace MakeAnythingCraftable
{
    [StaticConstructorOnStartup]
    public static class Utils
    {
        public static List<ThingDef> workbenches = new List<ThingDef>();
        public static List<ThingDef> craftableItems = new List<ThingDef>();
        public static List<ThingDef> unfinishedThings = new List<ThingDef>();
        public static HashSet<StatDef> workSpeedStats = new HashSet<StatDef>();
        public static HashSet<StatDef> efficiencyStats = new HashSet<StatDef>();
        public static HashSet<EffecterDef> effecterDefs = new HashSet<EffecterDef>();
        public static HashSet<SoundDef> soundDefs = new HashSet<SoundDef>();
        public static HashSet<RecipeDef> medicalRecipes = new HashSet<RecipeDef>();
        public static List<RecipeDef> craftingRecipes = new List<RecipeDef>();
        static Utils()
        {
            new Harmony("MakeAnythingCraftable.Mod").PatchAll();
            MakeAnythingCraftableMod.settings.ApplySettings();
        }
        public static void Reset()
        {
            workbenches = new List<ThingDef>();
            craftableItems = new List<ThingDef>();
            unfinishedThings = new List<ThingDef>();
            workSpeedStats = new HashSet<StatDef>();
            efficiencyStats = new HashSet<StatDef>();
            effecterDefs = new HashSet<EffecterDef>();
            soundDefs = new HashSet<SoundDef>();
            medicalRecipes = new HashSet<RecipeDef>();
            craftingRecipes = new List<RecipeDef>();            
        }
        
        public static void CreateRecipeLists()
        {
            foreach (var item in DefDatabase<ThingDef>.AllDefsListForReading)
            {
                if (typeof(Pawn).IsAssignableFrom(item.thingClass))
                {
                    foreach (var recipe in item.AllRecipes)
                    {
                        medicalRecipes.Add(recipe);
                    }
                }

                if (Spawnable(item))
                {
                    craftableItems.Add(item);
                }
                if (item.IsWorkTable)
                {
                    workbenches.Add(item);
                }
                if (item.isUnfinishedThing)
                {
                    unfinishedThings.Add(item);
                }
            }

            foreach (var recipe in DefDatabase<RecipeDef>.AllDefs)
            {
                if (recipe.workSpeedStat != null)
                {
                    workSpeedStats.Add(recipe.workSpeedStat);
                }
                if (recipe.efficiencyStat != null)
                {
                    efficiencyStats.Add(recipe.efficiencyStat);
                }
                if (recipe.soundWorking != null)
                {
                    soundDefs.Add(recipe.soundWorking);
                }
                if (recipe.effectWorking != null)
                {
                    effecterDefs.Add(recipe.effectWorking);
                }
                if (!medicalRecipes.Contains(recipe) && recipe.products != null
                    && recipe.products.Count == 1)
                {
                    craftingRecipes.Add(recipe);
                }
            }
        }

        public static bool Spawnable(this ThingDef item)
        {
            try
            {
                return (DebugThingPlaceHelper.IsDebugSpawnable(item) || item.Minifiable)
                                && !typeof(Filth).IsAssignableFrom(item.thingClass)
                                && !typeof(Mote).IsAssignableFrom(item.thingClass)
                                && item.category != ThingCategory.Ethereal && item.plant is null
                                && (item.building is null || item.Minifiable);
            }
            catch (Exception ex)
            {
                Log.Error("Caught error processing " + item + ": " + ex.ToString());
                return false;
            }
        }

        public static void ClearRemovedRecipesFromRecipeUsers(this RecipeDef recipeDef)
        {
            if (recipeDef.recipeUsers != null)
            {
                foreach (var recipeUser in recipeDef.recipeUsers)
                {
                    if (recipeUser.allRecipesCached != null)
                    {
                        for (int i = recipeUser.allRecipesCached.Count - 1; i >= 0; i--)
                        {
                            if (MakeAnythingCraftableMod.settings.removedRecipeDefs?.Any(x => x.defName == recipeUser.allRecipesCached[i].defName) ?? false)
                            {
                                recipeUser.allRecipesCached.RemoveAt(i);
                            }
                        }
                    }
                }
            }
        }

        public static void AddCreatedRecipesFromRecipeUsers(this RecipeDef recipeDef)
        {
            if (recipeDef.recipeUsers != null)
            {
                foreach (var recipeUser in recipeDef.recipeUsers)
                {
                    if (recipeUser.allRecipesCached != null)
                    {
                        foreach (var createdRecipe in MakeAnythingCraftableMod.settings.createdRecipeDefs)
                        {
                            if (!recipeUser.allRecipesCached.Contains(createdRecipe))
                            {
                                recipeUser.allRecipesCached.Add(createdRecipe);
                            }
                        }
                    }
                }
            }
        }
    }
    
    [HarmonyPatch(typeof(Game), "FinalizeInit")]
    public static class ThingDef_AllRecipes_Patch
    {
        public static void Prefix()
        {
            foreach (var def in DefDatabase<ThingDef>.AllDefs)
            {
                _ = def.AllRecipes;
                def.allRecipesCached = def.allRecipesCached?.Distinct()?.ToList();
            }
        }
    }
}
