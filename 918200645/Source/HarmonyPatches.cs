using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace SWSaber
{
    [StaticConstructorOnStartup]
    internal static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            var harmony = new Harmony("rimworld.jecrell.starwars.lightsaber");
            harmony.Patch(AccessTools.Method(typeof(Pawn_EquipmentTracker), nameof(Pawn_EquipmentTracker.AddEquipment)), null,
                new HarmonyMethod(typeof(HarmonyPatches).GetMethod(nameof(AddEquipment_PostFix))));
            harmony.Patch(AccessTools.Method(typeof(PawnInventoryGenerator), nameof(PawnInventoryGenerator.GenerateInventoryFor)), null,
                new HarmonyMethod(typeof(HarmonyPatches).GetMethod(nameof(GenerateInventoryFor_PostFix))));
        }



        // RimWorld.PawnInventoryGenerator
        public static void GenerateInventoryFor_PostFix(Pawn p, PawnGenerationRequest request)
        {
            //Log.Message("1");
            if (!Utility.AreFactionsLoaded())
            {
                return;
            }
            //Log.Message("2");

            if (p.kindDef == null)
            {
                return;
            }
            //Log.Message("3");

            if (p.kindDef.defName == "PJ_ImpCommander" ||
                p.kindDef.defName == "PJ_RebCouncilman" ||
                p.kindDef.defName == "PJ_ScumBoss")
            {
                //Log.Message("4");

                Utility.GenerateCrystalFor(p);
            }
        }

        //public static void Remove_PostFix(Pawn_EquipmentTracker __instance, ThingWithComps eq)
        //{
        //    CompLightsaberActivatableEffect lightsaberEffect = eq.TryGetComp<CompLightsaberActivatableEffect>();
        //    if (lightsaberEffect != null)
        //    {

        //    }
        //}


        public static void AddEquipment_PostFix(Pawn_EquipmentTracker __instance, ThingWithComps newEq)
        {
            var pawn = (Pawn) AccessTools.Field(typeof(Pawn_EquipmentTracker), "pawn").GetValue(__instance);

            var lightsaberEffect = newEq.TryGetComp<CompLightsaberActivatableEffect>();
            if (lightsaberEffect == null)
            {
                return;
            }

            if (pawn == null)
            {
                return;
            }

            if (pawn.Faction == null)
            {
                return;
            }

            if (pawn.Faction == Faction.OfPlayerSilentFail)
            {
                return;
            }

            var crystalSlot = newEq.GetComp<CompCrystalSlotLoadable>();
            if (crystalSlot == null)
            {
                return;
            }

            Utility.CrystalSlotter(crystalSlot, lightsaberEffect);
        }
    }
}