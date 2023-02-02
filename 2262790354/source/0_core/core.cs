using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;
using UnityEngine;
using HugsLib;
using HugsLib.Settings;
using System.Linq;
using RimWorld.Planet;

namespace forcedOrder
{
    /*
    public class core : ModBase
    {
        public override string ModIdentifier => "yayoEnding";

        static public List<string> ar_gemDef = new List<string>();



        private SettingHandle<int> goalBiomeSetting;
        public static int goalBiome = 3;

        private SettingHandle<float> extractSpeedSetting;
        public static float extractSpeed = 1f;



        public override void DefsLoaded()
        {
            // 셋팅
            goalBiomeSetting = Settings.GetHandle<int>("goalBiome", "goalBiome_title".Translate(), "goalBiome_desc".Translate(), 3);
            goalBiome = goalBiomeSetting.Value;

            extractSpeedSetting = Settings.GetHandle<float>("extractSpeed", "extractSpeed_title".Translate(), "extractSpeed_desc".Translate(), 1f);
            extractSpeed = extractSpeedSetting.Value;


            // 바이옴 에너지 조각 텍스쳐 수정
            int a = 0;
            foreach (ThingDef t in from thing in DefDatabase<ThingDef>.AllDefs
                                   where
                                        thing.defName.Contains("yy_gem_")
                                   select thing)
            {
                GraphicData gd = new GraphicData();
                gd.graphicClass = typeof(Graphic_Single);
                gd.texPath = $"yy_bep{a % 15}";
                t.graphicData = gd;
                a++;
            }
        }

        public override void SettingsChanged()
        {
            goalBiome = goalBiomeSetting.Value;
            extractSpeed = Mathf.Clamp(extractSpeedSetting.Value, 0.01f, 50f);

        }


        static public void patchDef()
        {
            Log.Message($"# Yayo's Ending Init 1");

            Log.Message($"# generate biome energy item");
            foreach (BiomeDef b in from biome in DefDatabase<BiomeDef>.AllDefs
                                   where
                                        biome.impassable == false
                                   select biome)
            {

                ThingDef t = new ThingDef();

                // base
                t.thingClass = typeof(ThingWithComps);
                t.category = ThingCategory.Item;
                t.resourceReadoutPriority = ResourceCountPriority.Middle;
                t.selectable = true;
                t.altitudeLayer = AltitudeLayer.Item;
                t.comps = new List<CompProperties>() { new CompProperties_Forbiddable() };
                t.alwaysHaulable = true;
                t.drawGUIOverlay = true;
                t.rotatable = false;
                t.pathCost = 14;

                // detail
                t.defName = $"yy_gem_{b.defName}";
                //t.label = $"{b.label} energy piece";
                t.label = string.Format("yayoEnding_energyPiece".Translate(), b.label);
                t.description = string.Format("yayoEnding_energyPiece".Translate(), b.label);
                t.graphicData = new GraphicData();
                t.graphicData.texPath = "Things/Item/Resource/Gold";
                //t.graphicData.texPath = $"yy_bep{ar_gemDef.Count % 15}";
                t.graphicData.graphicClass = typeof(Graphic_StackCount);
                t.soundInteract = SoundDef.Named("Silver_Drop");
                t.soundDrop = SoundDef.Named("Silver_Drop");
                t.useHitPoints = false;
                t.healthAffectsPrice = false;
                t.statBases = new List<StatModifier>();
                t.statBases = RimWorld.ThingDefOf.Silver.statBases;
                t.thingCategories = new List<ThingCategoryDef>();
                
                t.stackLimit = 100;
                //t.smallVolume = true;
                t.deepCommonality = 0.5f;
                t.deepCountPerPortion = 8;
                t.deepLumpSizeRange = new IntRange(1, 4);
                t.burnableByRecipe = false;
                t.smeltable = true;
                t.terrainAffordanceNeeded = TerrainAffordanceDefOf.Medium;

                t.thingCategories.Add(ThingCategoryDef.Named("yy_gem_piece_category"));
                t.tradeability = Tradeability.None;
                t.tradeTags = new List<string>();
                t.tradeTags.Add("yy_gem");

                ar_gemDef.Add(t.defName);
                DefGenerator.AddImpliedDef<ThingDef>(t);

                //Log.Message($"{b.defName}, {b.label}, {t.label}");

                
            }

            patchDef2();

        }

        
        static public void patchDef2()
        {
            Log.Message($"# Yayo's Ending Init 2");

            Log.Message($"# generate planet energy core recipe");
            for (int i = 0; i < 4; i++)
            {
                RecipeDef r = new RecipeDef();
                r.defName = $"Make_yy_planetCore_{i+1}";
                r.label = string.Format("yayoEnding_energyCore_recipe_label".Translate(), ThingDef.Named("yy_planetCore").label, (i+1).ToString());
                r.description = ThingDef.Named("yy_planetCore").description;
                r.jobString = string.Format("yayoEnding_energyCore_recipe_jobstring".Translate(), ThingDef.Named("yy_planetCore").label);
                r.workSpeedStat = StatDefOf.GeneralLaborSpeed;
                r.effectWorking = EffecterDefOf.Drill;
                r.soundWorking = SoundDef.Named("Recipe_Machining");

                r.workAmount = 1500; // 작업량

                r.recipeUsers = new List<ThingDef>(); // 제작 장소
                r.recipeUsers.Add(ThingDef.Named("CraftingSpot"));
                r.recipeUsers.Add(ThingDef.Named("FueledSmithy"));
                r.recipeUsers.Add(ThingDef.Named("ElectricSmithy"));
                r.recipeUsers.Add(ThingDef.Named("TableMachining"));
                r.recipeUsers.Add(ThingDef.Named("FabricationBench"));

                r.unfinishedThingDef = ThingDef.Named("UnfinishedComponent");

                List<IngredientCount> ingredient = new List<IngredientCount>();
                while (ingredient.Count < goalBiome && ingredient.Count < ar_gemDef.Count)
                {
                    ThingDef td = ThingDef.Named(ar_gemDef[Rand.Range(0, ar_gemDef.Count)]);
                    bool already = false;
                    for(int k = 0; k < ingredient.Count; k++)
                    {
                        if (ingredient[k].filter.AllowedThingDefs.Contains(td))
                        {
                            already = true;
                        }
                    }

                    if (!already)
                    {
                        IngredientCount ing = new IngredientCount();
                        ThingFilter tf = new ThingFilter();

                        ing.filter.SetAllow(td, true);

                        ing.SetBaseCount(100); // 각 바이옴 조각 필요 개수
                        ingredient.Add(ing);
                    }
                    
                }
                r.ingredients = ingredient;
                //Log.Message($"{r.ingredients[0].filter.AnyAllowedDef.defName}");


                r.products = new List<ThingDefCountClass>();
                ThingDefCountClass tdc = new ThingDefCountClass();
                tdc.thingDef = ThingDef.Named("yy_planetCore");
                tdc.count = 1;
                r.products.Add(tdc);
                r.skillRequirements = new List<SkillRequirement>();
                SkillRequirement sr = new SkillRequirement();
                sr.skill = SkillDefOf.Crafting;
                sr.minLevel = 8; // 제작 스킬레벨
                r.skillRequirements.Add(sr);
                r.workSkill = SkillDefOf.Crafting;

                //DefDatabase<RecipeDef>.Add(r);
                DefGenerator.AddImpliedDef<RecipeDef>(r);

            }
        }


        public override void WorldLoaded()
        {
            base.WorldLoaded();


            int seed = Find.World.info.Seed;

            // 실존하는 바이옴 리스트 생성
            List<string> tmp_ar_gemDef = new List<string>();
            for(int i = 0; i < Find.WorldGrid.tiles.Count; i++)
            {
                Tile t = Find.WorldGrid.tiles[i];
                string gemDefName = $"yy_gem_{t.biome.defName}";
                if (t.biome.canBuildBase && !tmp_ar_gemDef.Contains(gemDefName))
                {
                    tmp_ar_gemDef.Add(gemDefName);
                }
            }

            // 행성 에너지 핵 레시피 수정
            foreach (RecipeDef r in from recipe in DefDatabase<RecipeDef>.AllDefs
                                    where
                                         recipe.defName.Contains("Make_yy_planetCore_")
                                    select recipe)
            {
                List<IngredientCount> ingredient = new List<IngredientCount>();
                while (ingredient.Count < goalBiome && ingredient.Count < tmp_ar_gemDef.Count)
                {
                    ThingDef td = ThingDef.Named(tmp_ar_gemDef[Rand.RangeSeeded(0, tmp_ar_gemDef.Count, seed)]);
                    seed++;

                    bool already = false;
                    for (int k = 0; k < ingredient.Count; k++)
                    {
                        if (ingredient[k].filter.AllowedThingDefs.Contains(td))
                        {
                            already = true;
                        }
                    }

                    if (!already)
                    {
                        IngredientCount ing = new IngredientCount();
                        ThingFilter tf = new ThingFilter();

                        ing.filter.SetAllow(td, true);

                        ing.SetBaseCount(100); // 각 바이옴 조각 필요 개수
                        ingredient.Add(ing);
                    }

                }
                r.ingredients = ingredient;

            }

            



        }


        

    }







    */



}
