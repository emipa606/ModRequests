using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using UnityEngine;
using RimWorld.Planet;
using LostTechnology;

namespace LostTechnology
{
    [StaticConstructorOnStartup]
	public static class lostTechnologyWorld
	{
        static lostTechnologyWorld()
        {
            if (ModLister.RoyaltyInstalled)
            {
                patchDef2();
            }
        }

		public static List<ResearchProjectDef> patchDef()
		{
            string TECHPRINT_NEOLITHIC = "techprint_neolithic";
            string TECHPRINT_MEDIEVAL = "techprint_medieval";
            string TECHPRINT_INDUSTRIAL = "techprint_industrial";
            string TECHPRINT_SPACER = "techprint_spacer";
            string TECHPRINT_ULTRA = "techprint_ultra";


            // about mod factions
            List<string> ar_tag_neolithic = new List<string>() { TECHPRINT_NEOLITHIC };
            List<string> ar_tag_medieval = new List<string>() { TECHPRINT_MEDIEVAL };
            List<string> ar_tag_industrial = new List<string>() { TECHPRINT_INDUSTRIAL };
            List<string> ar_tag_spacer = new List<string>() { TECHPRINT_SPACER };
            List<string> ar_tag_ultra = new List<string>() { TECHPRINT_ULTRA };


            //foreach (FactionDef f in from faction in DefDatabase<FactionDef>.AllDefs where !faction.modContentPack.PackageId.ToLower().Contains("ludeon") select faction)
            foreach (FactionDef f in from faction in DefDatabase<FactionDef>.AllDefs where 
                                     !faction.hidden 
                                     //&& !faction.isPlayer 
                                     && !faction.permanentEnemy
                                     select faction)
            {
                //Log.Message($"faction : {f.defName} , techlevel : {f.techLevel}");

                if(f.categoryTag == null)
                {
                    switch (f.techLevel)
                    {
                        case TechLevel.Neolithic:
                            f.categoryTag = TECHPRINT_NEOLITHIC;
                            break;
                        case TechLevel.Medieval:
                            f.categoryTag = TECHPRINT_MEDIEVAL;
                            break;
                        case TechLevel.Industrial:
                            f.categoryTag = TECHPRINT_INDUSTRIAL;
                            break;
                        case TechLevel.Spacer:
                            f.categoryTag = TECHPRINT_SPACER;
                            break;
                        case TechLevel.Ultra:
                            f.categoryTag = TECHPRINT_ULTRA;
                            break;
                    }
                }
                else
                {
                    switch (f.techLevel)
                    {
                        case TechLevel.Neolithic:
                            ar_tag_neolithic.Add(f.categoryTag);
                            break;
                        case TechLevel.Medieval:
                            ar_tag_medieval.Add(f.categoryTag);
                            break;
                        case TechLevel.Industrial:
                            ar_tag_industrial.Add(f.categoryTag);
                            break;
                        case TechLevel.Spacer:
                            ar_tag_spacer.Add(f.categoryTag);
                            break;
                        case TechLevel.Ultra:
                            ar_tag_ultra.Add(f.categoryTag);
                            break;
                    }
                }
                

            }

            if (!ar_tag_neolithic.Contains("Tribal"))
            {
                ar_tag_neolithic.Add("Tribal");
            }
            if (!ar_tag_medieval.Contains("Tribal"))
            {
                ar_tag_medieval.Add("Tribal");
            }
            if (!ar_tag_industrial.Contains("Outlander"))
            {
                ar_tag_industrial.Add("Outlander");
            }
            if (!ar_tag_spacer.Contains("Outlander"))
            {
                ar_tag_spacer.Add("Outlander");
            }



            // save for those project with numbers tailing name, for example SomeProject_1, SomeProject_2
            List<ResearchProjectDef> ProjectNamedInNumbers = new List<ResearchProjectDef>();

            foreach (ResearchProjectDef r in from research in DefDatabase<ResearchProjectDef>.AllDefs where research.techprintCount > 0 select research)
			{
                if (r.defName.Any(char.IsDigit))
                {
                    ProjectNamedInNumbers.Add(r);
                    continue;
                }

                if (r.baseCost > 4050)
				{
					r.techprintCount = 1;
				}
			}

            Dictionary<TechLevel,bool> UnlockSetting = new Dictionary<TechLevel, bool>();
            UnlockSetting.Add(TechLevel.Neolithic, LostTechnologySettings.UnlockNeolithic);
            UnlockSetting.Add(TechLevel.Medieval, LostTechnologySettings.UnlockMedieval);
            UnlockSetting.Add(TechLevel.Industrial, LostTechnologySettings.UnlockIndustrial);
            UnlockSetting.Add(TechLevel.Spacer, LostTechnologySettings.UnlockSpacer);
            UnlockSetting.Add(TechLevel.Ultra, LostTechnologySettings.UnlockUltra);

            foreach (ResearchProjectDef r in from research in DefDatabase<ResearchProjectDef>.AllDefs where research.techprintCount <= 0 select research)
			{
                UnlockSetting.TryGetValue(r.techLevel, out bool shouldbeunlocked);
                if (shouldbeunlocked)
                {
                    continue;
                }
                if (r.baseCost > 4050)
                {
                    r.techprintCount = 1;
                }
                else
                {
                    switch (r.defName)
                    {
                        default:
                            r.techprintCount = 1;
                            break;
                        case "Electricity":
                            r.techprintCount = 1;
                            break;
                        case "MicroelectronicsBasics":
                            r.techprintCount = 1;
                            break;
                        case "MultiAnalyzer":
                            r.techprintCount = 1;
                            break;
                    }
                }
                // I think it's a good idea to change the commonality for key early tech
                //ComplexFurniture,Stonecutting,ComplexClothing
                switch (r.defName)
                {
                    default:
                        r.techprintCommonality = 15f * LostTechnologySettings.appearanceProbability;
                        break;
                    case "ComplexClothing":
                        r.techprintCommonality = 60f * LostTechnologySettings.appearanceProbability;
                        break;
                    case "ComplexFurniture":
                        r.techprintCommonality = 60f * LostTechnologySettings.appearanceProbability;
                        break;
                    case "Stonecutting":
                        r.techprintCommonality = 60f * LostTechnologySettings.appearanceProbability;
                        break;

                }                

                //below add the support for mod faction
                if (r.heldByFactionCategoryTags == null)
                {
                    r.heldByFactionCategoryTags = new List<string>();
                }
                switch (r.techLevel)
                {
                    default:
                        Log.Message($"{r.defName} tech level is {r.techLevel}");
                        break;
                    case TechLevel.Neolithic:
                        r.heldByFactionCategoryTags.AddRange(ar_tag_neolithic);
                        break;
                    case TechLevel.Medieval:
                        r.heldByFactionCategoryTags.AddRange(ar_tag_medieval);
                        break;
                    case TechLevel.Industrial:
                        r.heldByFactionCategoryTags.AddRange(ar_tag_industrial);
                        break;
                    case TechLevel.Spacer:
                        r.heldByFactionCategoryTags.AddRange(ar_tag_spacer);
                        r.heldByFactionCategoryTags.AddRange(ar_tag_ultra);
                        break;
                    case TechLevel.Ultra:
                        r.heldByFactionCategoryTags.AddRange(ar_tag_ultra);
                        break;
                }

                r.techprintMarketValue = r.baseCost * LostTechnologySettings.priceFactor / (float)r.techprintCount;
                if (r.defName.Any(char.IsDigit))
                {
                    ProjectNamedInNumbers.Add(r);
                    r.techprintCount = 0;
                }

            }

            return ProjectNamedInNumbers;


        }



        // when defs loaded : It is triggered late to avoid affecting the price of the techprint.
       
        public static void patchDef2()
        {
            // patch def . research cost modify by setting value
            foreach (ResearchProjectDef r in from research in DefDatabase<ResearchProjectDef>.AllDefs where true select research)
            {
                if (r.techprintCount > 0) { r.baseCost *= LostTechnologySettings.costMultiply; }
            }
        }
    }
}
