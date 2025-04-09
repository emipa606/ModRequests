using HarmonyLib;
using PipeSystem;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PneumaticTubes
{
    /// <summary>
    /// Harmony patch class, allowing modification of the base game or other mods
    /// </summary>
    [StaticConstructorOnStartup]
    public class HarmonyPatches
    {

        /// <summary>
        /// Static constructor to load up injections on startup
        /// </summary>
        static HarmonyPatches()
        {
            Harmony harmony = new Harmony("rimworld.kayesh.standardizedextrusions");

            try
            {
                harmony.PatchAll();
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
            }
        }
    }

    /// <summary>
    /// Patches for the base DesignationCategoryDef.ResolvedAllowedDesignators method
    /// </summary>
    [HarmonyPatch(typeof(DesignationCategoryDef))]
    [HarmonyPatch(nameof(DesignationCategoryDef.ResolvedAllowedDesignators), MethodType.Getter)]
    public static class ResolvedAllowedDesignators_Patch
    {
        /// <summary>
        /// Add another postfix onto DesignationCategoryDef.ResolvedAllowedDesignators after the one tfrom PipeSystem.
        /// Moves the pipe deconstruct designators from this mod into groups.
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="___resolvedDesignators"></param>
        public static void Postfix(ref DesignationCategoryDef __instance, ref List<Designator> ___resolvedDesignators)
        {
            Dictionary<DesignatorDropdownGroupDef, Designator_Dropdown> dictionary = new Dictionary<DesignatorDropdownGroupDef, Designator_Dropdown>();
            
            // Removes the pipe deconstruct pipe designator from the designation category and adds it back under a designation group (if not already present)
            foreach (PipeNetDef pipeNetDef in DefDatabase<PipeNetDef>.AllDefsListForReading.Where(p => p.defName.StartsWith(PipeStuff.ModIdentifier)))
            {

                Designator_DeconstructPipe deconstructPipe = (Designator_DeconstructPipe)___resolvedDesignators.Find(d => d is Designator_DeconstructPipe dPipe && dPipe.pipeNetDef.defName == pipeNetDef.defName);

                if (deconstructPipe != null)
                {
                    ___resolvedDesignators.Remove(deconstructPipe);

                    ThingDef thingDef = pipeNetDef.linkToRefuelables[0].thing;
                    ThingCategoryDef thingCategoryDef = ModSettings.GetAllowedThingCategories().Intersect(thingDef.thingCategories).First();
                    
                    string designationDropDownGroupDefNamePrefix = $"{PipeStuff.ModIdentifier}_{thingCategoryDef}_group";
                    string designationDropDownGroupNameDeconstruct = $"{designationDropDownGroupDefNamePrefix}_deconstruct";
                    DesignatorDropdownGroupDef designationDropDownGroupDeconstructDef = DefDatabase<DesignatorDropdownGroupDef>.GetNamed(designationDropDownGroupNameDeconstruct);

                    if (!___resolvedDesignators.Any(d => d is Designator_Dropdown dropDown && dropDown.Elements.Any(dd => dd is Designator_DeconstructPipe deconPipe && deconPipe.pipeNetDef.defName == pipeNetDef.defName)))
                    {

                        if (!dictionary.ContainsKey(designationDropDownGroupDeconstructDef))
                        {
                            dictionary[designationDropDownGroupDeconstructDef] = new Designator_Dropdown();
                            dictionary[designationDropDownGroupDeconstructDef].Order = deconstructPipe.Order;
                            ___resolvedDesignators.Add(dictionary[designationDropDownGroupDeconstructDef]);
                        }

                        deconstructPipe.defaultIconColor = pipeNetDef.overlayOptions.overlayColor;
                        dictionary[designationDropDownGroupDeconstructDef].Add(deconstructPipe);
                    }
                }
            }
        }
    }
}
