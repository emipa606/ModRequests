using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using RimWorld;
using Verse;
using Verse.AI;
using System.Reflection.Emit;

namespace PerfectPathing
{
    public static class DirtAvoid
    {
        public static IEnumerable<CodeInstruction> Patch_FindPath(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo initStatuses = AccessTools.Method(typeof(PathFinder), "InitStatusesAndPushStartNode");
            FieldInfo drafted = AccessTools.Field(typeof(TerrainDef), "extraDraftedPerceivedPathCost");
            FieldInfo nondrafted = AccessTools.Field(typeof(TerrainDef), "extraNonDraftedPerceivedPathCost");
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Call && CodeInstructionExtensions.OperandIs(codes[i], initStatuses))
                {
                    //Log.Warning(codes[i].ToString());                    

                    codes.InsertRange(i + 1, new List<CodeInstruction>
                    {
                        new CodeInstruction(OpCodes.Ldarg_0), //this
                        new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PathFinder), "map")),
                        new CodeInstruction(OpCodes.Ldloc_0), //pawn
                        new CodeInstruction(OpCodes.Callvirt, typeof(DirtAvoid).GetMethod("CacheDirt"))
                    });
                    break;
                }
            }

            for (int i = 10; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldfld && CodeInstructionExtensions.OperandIs(codes[i], nondrafted))
                {
                    //Log.Warning(codes[i].ToString());
                    if (CodeInstructionExtensions.OperandIs(codes[i], drafted))
                    {
                        Log.Error("JP Note to self: Set the dirtavoid hook back to nondrafted you adhd brain spaz");
                    }

                    //thank you to OwlChemist for this bit
                    codes.InsertRange(i + 3, new List<CodeInstruction>
                    {
                        new CodeInstruction(OpCodes.Ldloc_0), //pawn
                        new CodeInstruction(OpCodes.Ldloc_S, 12), //topGrid (TerrainDef[])
                        new CodeInstruction(OpCodes.Ldloc_S, 45), //newIndex
                        new CodeInstruction(OpCodes.Ldelem_Ref),
                        new CodeInstruction(OpCodes.Ldloc_S, 48), //cost
                        new CodeInstruction(OpCodes.Ldloc_S, 3), //curIndex
                        new CodeInstruction(OpCodes.Ldloc_S, 45), //newIndex
                        new CodeInstruction(OpCodes.Call, typeof(DirtAvoid).GetMethod("AddCost")),
                        new CodeInstruction(OpCodes.Stloc_S, 48) //cost
                    });
                    break;
                }
            }
            return codes;
        }

        static bool friendlyHuman;
        static bool[] dirtGrid;
        public static void CacheDirt(Map map, Pawn pawn)
        {
            friendlyHuman = pawn == null || pawn.RaceProps.Animal || pawn.HostileTo(Faction.OfPlayer);
            dirtGrid = new bool[map.Size.x * map.Size.z];
        }

        public static int AddCost(Pawn pawn, TerrainDef def, int cost, int curIndex, int newIndex)
        {
            //Friendly humans only
            if (friendlyHuman)
                return cost;

            if (dirtGrid[curIndex])
            {
                dirtGrid[newIndex] = true;
                return cost;
            }

            if (def.generatedFilth != null)
            {
                dirtGrid[newIndex] = true;
                return cost + Mathf.RoundToInt(Settings.dirtStrength * 60);
            }

            return cost;
        }
    }
}
