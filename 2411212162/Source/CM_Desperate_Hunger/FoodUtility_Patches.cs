using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using UnityEngine;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace CM_Desperate_Hunger
{
    [StaticConstructorOnStartup]
    public static class FoodUtility_Patches
    {
        [HarmonyPatch(typeof(FoodUtility))]
        [HarmonyPatch("WillEat", new Type[] { typeof(Pawn), typeof(ThingDef), typeof(Pawn), typeof(bool)})]
        public class FoodUtility_WillEat_ThingDef
        {
            [HarmonyPostfix]
            public static void Postfix(Pawn p, ThingDef food, ref bool __result)
            {
                if (__result && food != null && DesperateHungerMod.settings.featureEnabled && DesperateHungerMod.settings.ignoreFertilizedEggs && food.HasComp(typeof(CompHatcher)))
                {
                    __result = false;
                    return;
                }

                if (!__result && DesperateHungerMod.settings.featureEnabled && (p.AnimalOrWildMan() || DesperateHungerMod.settings.desperateHumans) && food.ingestible != null && (food.ingestible.foodType & FoodTypeFlags.Corpse) == FoodTypeFlags.Corpse)
                {
                    Hediff malnutrition = p.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Malnutrition);
                    bool overrideEat = (malnutrition != null && malnutrition.Severity > DesperateHungerMod.settings.minimumMalnutritionToHuntWoundedTarget);

                    __result = overrideEat;
                }
            }
        }

        [HarmonyPatch(typeof(FoodUtility))]
        [HarmonyPatch("WillEat", new Type[] { typeof(Pawn), typeof(Thing), typeof(Pawn), typeof(bool) })]
        public class FoodUtility_WillEat_Thing
        {
            [HarmonyPostfix]
            public static void Postfix(Pawn p, Thing food, ref bool __result)
            {
                if (__result && food != null && DesperateHungerMod.settings.featureEnabled && DesperateHungerMod.settings.ignoreFertilizedEggs && food.def.HasComp(typeof(CompHatcher)))
                {
                    __result = false;
                    return;
                }

                if (!__result && DesperateHungerMod.settings.featureEnabled && (p.AnimalOrWildMan() || DesperateHungerMod.settings.desperateHumans) && food.def.ingestible != null && (food.def.ingestible.foodType & FoodTypeFlags.Corpse) == FoodTypeFlags.Corpse)
                {
                    Hediff malnutrition = p.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Malnutrition);
                    bool overrideEat = (malnutrition != null && malnutrition.Severity > DesperateHungerMod.settings.minimumMalnutritionToHuntWoundedTarget);

                    __result = overrideEat;
                }
            }
        }
    }
}
