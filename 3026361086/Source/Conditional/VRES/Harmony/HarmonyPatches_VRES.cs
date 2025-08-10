using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VanillaRacesExpandedSanguophage;
using Verse;

namespace Komishne.SanguophageTweaks.Conditional.VRES
{
    [HarmonyPatch(typeof(CompAbilityEffect_AnimalfeederBite), "BiteAmount")]
    static class CompAbilityEffect_AnimalfeederBite_BiteAmount_Patch
    {
        public static float _baseBiteAmount = 0.6f;

        static bool Prepare(/*MethodBase original*/)
        {
            // There is only one method being patched in this class, and only one feature being patched, so we do not
            // need to check the MethodBase parameter.
            return SanguophageTweaksSettings.UseThisModsFormulaForAnimalBloodfeedingBloodLoss;
        }

        // The base "amount" that BiteAmount is modifying is always the same: 0.6. This can be determined by computing
        // the value of BiteAmount for a pawn with a body size of 1. Instead of using BiteAmount's computed value, we
        // will ignore it and compute our own.
        //
        // Note that the BiteAmount function triggers constantly while mousing over a target with the ability active.
        static float Postfix(float __result, /*CompAbilityEffect_AnimalfeederBite __instance,*/ Pawn target)
        {
            return _baseBiteAmount * Utility.BloodlossSeverityMultiplierFromBodySize(target);
        }
    }
}
