﻿using HarmonyLib;
using RimWorld;
using Verse;
using WhatTheHack.Buildings;

namespace WhatTheHack.Harmony;

//This makes sure that mechanoids without work modules for certain skills don't use their skill value for those skills, but use the noSkillOffset or noSkillFactor instead.
//[HarmonyPatch(typeof(StatWorker), "GetExplanationUnfinalized")]
//public static class StatWorker_GetExplanationUnfinalized
//{
//    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
//    {
//        var instructionsList = new List<CodeInstruction>(instructions);
//        int i = 0;
//        foreach (CodeInstruction instruction in instructionsList)
//        {
//            if (instruction.operand as FieldInfo == typeof(Pawn).GetField("skills") && instructionsList[i + 1].opcode == OpCodes.Brfalse)
//            {
//                yield return new CodeInstruction(OpCodes.Ldarg_0);
//                yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(StatWorker), "stat"));
//                yield return new CodeInstruction(OpCodes.Call, typeof(Utilities).GetMethod("ShouldGetStatValue"));
//            }
//            else
//            {
//                yield return instruction;
//            }
//            i++;
//        }
//    }
//}
//This makes sure that mechanoids without work modules for certain skills don't use their skill value for those skills, but use the noSkillOffset or noSkillFactor instead.
//[HarmonyPatch(typeof(StatWorker), "GetValueUnfinalized")]
//public static class StatWorker_GetValueUnfinalized
//{
//    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
//    {
//        var instructionsList = new List<CodeInstruction>(instructions);
//        foreach (CodeInstruction instruction in instructionsList)
//        {
//            if (instruction.operand as FieldInfo == AccessTools.Field(typeof(Pawn), "skills"))
//            {
//                yield return new CodeInstruction(OpCodes.Ldarg_0);
//                yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(StatWorker), "stat"));
//                yield return new CodeInstruction(OpCodes.Call, typeof(Utilities).GetMethod("ShouldGetStatValue"));
//            }
//            else
//            {
//                yield return instruction;
//            }
//        }
//    }
//    /*
//    public static bool ShouldGetStatValue(Pawn pawn)
//    {
//        return false;
//    }

//    */

//}
[HarmonyPatch(typeof(StatWorker), "ShouldShowFor")]
internal static class StatWorker_ShouldShowFor
{
    private static bool Prefix(StatRequest req, ref bool __result, ref StatDef ___stat)
    {
        if (___stat.category == WTH_DefOf.WTH_StatCategory_HackedMechanoid && req.Thing is Pawn pawn)
        {
            __result = pawn.IsHacked();
            return false;
        }

        if (___stat.category == WTH_DefOf.WTH_StatCategory_Colonist && req.Thing is Pawn pawn2)
        {
            __result = pawn2.IsColonistPlayerControlled;
            return false;
        }

        if (___stat.category == WTH_DefOf.WTH_StatCategory_Platform && req.Thing is Building_BaseMechanoidPlatform)
        {
            __result = true;
            return false;
        }

        if (___stat.category != WTH_DefOf.WTH_StatCategory_HackedMechanoid &&
            ___stat.category != WTH_DefOf.WTH_StatCategory_Colonist &&
            ___stat.category != WTH_DefOf.WTH_StatCategory_Platform)
        {
            return true;
        }

        __result = false;
        return false;
    }
}