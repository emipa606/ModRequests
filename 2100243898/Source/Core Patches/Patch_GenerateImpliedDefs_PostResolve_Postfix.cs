using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using HarmonyLib;
using Verse;
using Verse.AI;

/*
namespace DTechprinting
{
    [HarmonyPatch(typeof(DefGenerator))]
    [HarmonyPatch("GenerateImpliedDefs_PostResolve")]
    class Patch_GenerateImpliedDefs_PostResolve_Postfix
    {
        private static void Postfix()
        {
            
            if (TechprintingSettings.addTechprintRequirements)
                ResearchProjectHelper.SetTechprintRequirements();
            if (TechprintingSettings.splitProjects)
                ResearchProjectHelper.SplitAllProjects();

            foreach (ThingDef def in ThingDefGenerator_Techshards.ImpliedTechshardDefs())
            {
                DefGenerator.AddImpliedDef<ThingDef>(def);
				def.ResolveReferences();
			}

            DefDatabase<ThingCategoryDef>.ResolveAllReferences(true, false);
            DefDatabase<RecipeDef>.ResolveAllReferences(true, true);
            ResourceCounter.ResetDefs();
		    
        }
        
       
    }
}
*/