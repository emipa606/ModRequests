using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using RimWorld;
using Verse;
using HarmonyLib;
using System.Reflection.Emit;
using static UnityEngine.Random;
//using static Stranger_Danger.BioSkillsPatch;

namespace Stranger_Danger
{
    [StaticConstructorOnStartup]
    static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            Log.Message("JP_SD: v.1.4.2 Loading..");
            var harmony = new Harmony("StrangerDangerPatch");
            harmony.PatchAll();

            //Hides backstory
            //These two lines will likely need adjusting every few updates
            var assemblyClass = AccessTools.TypeByName("CharacterCardUtility").GetNestedType("<>c__DisplayClass43_0", BindingFlags.NonPublic);
            var assemblyMethod = assemblyClass.GetMethod("<DoLeftSection>b__4", BindingFlags.NonPublic | BindingFlags.Instance);
            harmony.Patch(assemblyMethod, transpiler: new HarmonyMethod(typeof(BioBackstoryPatch).GetMethod("DrawCharacterCardMethod_Patch")));

            if (AccessTools.TypeByName("VFEMedieval.BlackKnightUtility") != null)
            {
                Log.Message("JP_SD: VFEMedieval found");
                IDontKnowWhatImDoing.VFEMedievalFound = true;
            }

            if (AccessTools.TypeByName("PawnQuickInfo.Patches") != null)
            {
                Log.Message("JP_SD: PawnQuickInfo found");

                try
                {
                    Log.Message("DrawPawnCommonInfo");
                    var mMethod = AccessTools.TypeByName("PawnQuickInfo.Patches").GetMethod("DrawPawnCommonInfo", new []{typeof(Pawn), typeof(Rect)}); 
                    harmony.Patch(mMethod, transpiler: new HarmonyMethod(typeof(PawnQuickInfoPatch).GetMethod("AllPatch")));
                }
                catch (Exception e)
                {
                    Log.Message("SkillModelPatch failed :( rimhud updated?\n" + e);
                }
            }
            
            if (AccessTools.TypeByName("RimHUD.Mod") != null)
            {
                Log.Message("JP_SD: RimHUD found");
                //*
                
                try
                {
                    Log.Message("SkillModelPatch");
                    var mConstructor = AccessTools.TypeByName("RimHUD.Interface.Hud.Models.Values.SkillValue").GetConstructor(new Type[] { typeof(SkillDef) }); //AccessTools.TypeByName("RimHUD.Interface.Hud.Models.Values.SkillValue.SkillValue"),
                    harmony.Patch(mConstructor, transpiler: new HarmonyMethod(typeof(RimHUDPatch).GetMethod("AllPatch")));
                }
                catch (Exception e)
                {
                    Log.Message("SkillModelPatch failed :( rimhud updated?\n" + e);
                }

                try
                {
                    Log.Message("GetSkillColorPatch");
                    harmony.Patch(AccessTools.Method("RimHUD.Interface.Hud.Models.Values.SkillValue:GetColor"), transpiler: new HarmonyMethod(typeof(RimHUDPatch).GetMethod("AllPatch")));
                }
                catch (Exception e)
                {
                    Log.Message("GetSkillColorPatch failed :( rimhud updated?\n" + e);
                }
                //Log.Message("GetStatStringPatch");
                //When skill is disabled general work does not display, when hidden it does.
                //harmony.Patch(AccessTools.Method("RimHUD.Interface.Hud.Models.Values.SkillValue:PrepareBuilder"), postfix: new HarmonyMethod(typeof(RimHUDPatch).GetMethod("AllPatch")));
                //*/
            }


        }


    }

    // PawnQuickInfo compatability
    static class PawnQuickInfoPatch
    {
        static MethodInfo SkillRecord_TotallyDisabled = AccessTools.PropertyGetter(typeof(SkillRecord), "TotallyDisabled");
        static FieldInfo SkillRecord_passion = AccessTools.Field(typeof(SkillRecord), "passion");
        private static MethodInfo SkillRecord_GetLevel = AccessTools.Method(typeof(SkillRecord), "GetLevel");
        
        public static IEnumerable<CodeInstruction> AllPatch(IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction i in instructions)
            {
                if (i.opcode == OpCodes.Callvirt && (MethodInfo)i.operand == SkillRecord_GetLevel)
                {
                    i.opcode = OpCodes.Call;
                    i.operand = typeof(BioSkillsPatch).GetMethod("GetLevelProxy");
                }
                
                if (i.opcode == OpCodes.Callvirt && (MethodInfo)i.operand == SkillRecord_TotallyDisabled)
                {
                    //Log.Warning(i.ToString());
                    i.opcode = OpCodes.Call;
                    i.operand = typeof(BioSkillsPatch).GetMethod("TotallyDisabled_sd");
                }

                if (i.opcode == OpCodes.Ldfld && (FieldInfo)i.operand == SkillRecord_passion)
                {
                    i.opcode = OpCodes.Call;
                    i.operand = typeof(BioSkillsPatch).GetMethod("passion_sd");
                }
                

                yield return i;
            }
        }
        
    }
    
    //RimHUD Compatibility
    static class RimHUDPatch
    {
        static MethodInfo SkillRecord_TotallyDisabled = AccessTools.PropertyGetter(typeof(SkillRecord), "TotallyDisabled");
        static FieldInfo SkillRecord_passion = AccessTools.Field(typeof(SkillRecord), "passion");

        //replaces vanilla methods with my modified versions when called by rimhud in specific places
        public static IEnumerable<CodeInstruction> AllPatch(IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction i in instructions)
            {
                if (i.opcode == OpCodes.Callvirt && (MethodInfo)i.operand == SkillRecord_TotallyDisabled)
                {
                    //Log.Warning(i.ToString());
                    i.opcode = OpCodes.Call;
                    i.operand = typeof(BioSkillsPatch).GetMethod("TotallyDisabled_sd");
                }

                if (i.opcode == OpCodes.Ldfld && (FieldInfo)i.operand == SkillRecord_passion)
                {
                    i.opcode = OpCodes.Call;
                    i.operand = typeof(BioSkillsPatch).GetMethod("passion_sd");
                }
                

                yield return i;
            }
        }
    }
}
