﻿using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse.AI.Group;
using WhatTheHack.ThinkTree;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(CaravanFormingUtility), "AllSendablePawns")]
internal static class CaravanFormingUtility_AllSendablePawns
{
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var instructionsList = new List<CodeInstruction>(instructions);
        for (var i = 0; i < instructionsList.Count; i++)
        {
            var instruction = instructionsList[i];
            if (instruction.opcode == OpCodes.Isinst && instruction.operand.Equals(typeof(LordJob_VoluntarilyJoinable)))
            {
                yield return new CodeInstruction(OpCodes.Call,
                    typeof(CaravanFormingUtility_AllSendablePawns).GetMethod("HasRightLordJob"));
            }
            else
            {
                yield return instruction;
            }
        }
    }

    public static bool HasRightLordJob(LordJob lordJob)
    {
        return lordJob is LordJob_VoluntarilyJoinable || lordJob is LordJob_SearchAndDestroy ||
               lordJob is LordJob_ControlMechanoid;
    }
}