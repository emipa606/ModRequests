using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using HarmonyLib;

namespace ArtifactsExpanded
{
    [StaticConstructorOnStartup]
    public class HarmonyPatch_ThingSetMaker_RandomOption
    {
        static HarmonyPatch_ThingSetMaker_RandomOption()
        {
            //creates a new Harmony instance and assigns it a unique ID
            var harmonyInstance = new Harmony("RimWorld.Carnagion.ArtifactsExpanded.HarmonyPatch_ThingSetMaker_RandomOption");

            //assigns original method and postfix method to variables
            var originalMethod = AccessTools.Method(typeof(ThingSetMaker_RandomOption), "GetSelectionWeight");
            var postfixMethod = AccessTools.Method(typeof(HarmonyPatch_ThingSetMaker_RandomOption), "Postfix_GetSelectionWeight");

            //calls the patch method
            harmonyInstance.Patch(originalMethod, postfix: new HarmonyMethod(postfixMethod));
        }

        static float Postfix_GetSelectionWeight(float __result, ThingSetMaker_RandomOption.Option option, ThingSetMakerParams parms)
        {
            if (option.weightIfPlayerHasNoItem != null && !PlayerItemAccessibilityUtility.PlayerOrQuestRewardHas(option.weightIfPlayerHasNoItemItem, 1))
            {
                return __result;
            }
            return __result * ArtifactsExpandedMod.modSettings.rewardStandardHighFreqWeightFactor;
        }
    }
}
