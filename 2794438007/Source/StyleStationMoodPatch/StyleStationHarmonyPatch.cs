using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Verse;

namespace StyleStationMoodPatch
{
    public class StyleStationHarmonyPatch
    {
        [StaticConstructorOnStartup]
        public static class StyleStationHarmonyPatches
        {
            static StyleStationHarmonyPatches()
            {

                Harmony harmony = new Harmony("crazedmonkey231.stylestation.mood.patch");
                harmony.PatchAll(Assembly.GetExecutingAssembly());

            }
        }

        [HarmonyPatch(typeof(Pawn_StyleTracker), "SetupNextLookChangeData")]
        [HarmonyPatch(new Type[] { typeof(HairDef), typeof(BeardDef), typeof(TattooDef), typeof(TattooDef), typeof(Color?) })]
        static class FloatMenuMakerMap_ChoicesAtFor
        {
            static void Postfix(Pawn_StyleTracker __instance, HairDef hair, BeardDef beard, TattooDef faceTatoo, TattooDef bodyTattoo, Color? hairColor)
            {

                if (!StyleStationPatchMod.settings.changeHair)
                {
                    __instance.nextHairDef = null;
                }

                if (!StyleStationPatchMod.settings.changeBeard)
                {
                    __instance.nextBeardDef = __instance.beardDef;
                }

                if (!StyleStationPatchMod.settings.changeFaceTattoo)
                {
                    __instance.nextFaceTattooDef = __instance.FaceTattoo;
                }

                if (!StyleStationPatchMod.settings.changeBodyTattoo)
                {
                    __instance.nextBodyTatooDef = __instance.BodyTattoo;
                }

                if (!StyleStationPatchMod.settings.changeHairColor)
                {
                    __instance.nextHairColor = null;
                }
            }
        }
    }
}
