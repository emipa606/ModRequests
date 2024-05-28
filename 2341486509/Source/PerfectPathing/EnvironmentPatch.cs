using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using RimWorld;
using Verse;
using Verse.AI;
using System.Reflection.Emit;

namespace PerfectPathing
{
    /* local variables as of Rimworld 1.4.3555
     * 3 = curIndex
     * 12 = topGrid
     * 31 = TicksPerMoveCardinal
     * 32 = TicksPerMoveDiagonal
     * 33? = curCell
     * 45 = newIndex
     * 48 = cost, @ int num19 = ((i > 3) ? num10 : num9);
     */

    public static class EnvironmentPatch
    {
        public static IEnumerable<CodeInstruction> Patch_FindPath(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo initStatuses = AccessTools.Method(typeof(PathFinder), "InitStatusesAndPushStartNode");
            MethodInfo getCard = AccessTools.Property(typeof(Pawn), nameof(Pawn.TicksPerMoveCardinal)).GetGetMethod();
            MethodInfo getDiag = AccessTools.Property(typeof(Pawn), nameof(Pawn.TicksPerMoveDiagonal)).GetGetMethod();
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
                        new CodeInstruction(OpCodes.Callvirt, typeof(EnvironmentPatch).GetMethod("CacheEnvironmental"))
                     });
                    break;
                }
            }

            //store pawn's cardinal & diagonal movespeeds before main loop
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Callvirt && CodeInstructionExtensions.OperandIs(codes[i], getCard))
                {
                    //Log.Warning(codes[i].ToString());
                    codes[i].operand = typeof(EnvironmentPatch).GetMethod("TicksPerMove_pawn");
                    codes.Insert(i, new CodeInstruction(OpCodes.Ldc_I4_0));
                    break;
                }
            }

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Callvirt && CodeInstructionExtensions.OperandIs(codes[i], getDiag))
                {
                    //Log.Warning(codes[i].ToString());
                    codes[i].operand = typeof(EnvironmentPatch).GetMethod("TicksPerMove_pawn");
                    codes.Insert(i, new CodeInstruction(OpCodes.Ldc_I4_1));
                    break;
                }
            }

            //load stored cardinal or diagonal movespeed each index iteration
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldloc_S && codes[i].operand.ToString() == "System.Int32 (48)") //I can't figure out how to do this better
                {
                    //Log.Warning(codes[i].ToString());
                    codes.RemoveAt(i);
                    codes.InsertRange(i, new List<CodeInstruction>
                    {
                        new CodeInstruction(OpCodes.Ldarg_0), //this
                        new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PathFinder), "map")),
                        new CodeInstruction(OpCodes.Ldloc_0), //pawn
                        //new CodeInstruction(OpCodes.Ldloc, 33?), //curCell
                        new CodeInstruction(OpCodes.Ldloc, 45), //newIndex
                        new CodeInstruction(OpCodes.Ldloc, 48), //cost
                        new CodeInstruction(OpCodes.Callvirt, typeof(EnvironmentPatch).GetMethod("TicksPerMove_tile"))
                    });
                    break;
                }
            }

            return codes;
        }

        //modified TicksPerMove to NOT consider light or weather at the initial pawn location
        public static int TicksPerMove_pawn(Pawn pawn, bool diagonal)
        {
            //RimWorld.StatWorker.GetValue
            var stat = StatDefOf.MoveSpeed;
            float moveSpeed = stat.Worker.GetValueUnfinalized(StatRequest.For(pawn));

            //RimWorld.StatWorker.FinalizeValue
            //skip stat.parts glow factor
            if (Find.Scenario != null)
                moveSpeed *= Find.Scenario.GetStatFactor(stat);
            moveSpeed = Mathf.Clamp(moveSpeed, stat.minValue, stat.maxValue);
            if (Mathf.Abs(moveSpeed) > stat.roundToFiveOver)
                moveSpeed = Mathf.Round(moveSpeed / 5f) * 5f;

            //Pawn.TicksPerMove
            if (RestraintsUtility.InRestraints(pawn))
                moveSpeed *= 0.35f;
            if (pawn.carryTracker != null && pawn.carryTracker.CarriedThing != null && pawn.carryTracker.CarriedThing.def.category == ThingCategory.Pawn)
                moveSpeed *= 0.6f;
            moveSpeed /= 60f;

            float ticks;
            if (moveSpeed == 0f)
            {
                ticks = 450f;
            }
            else
            {
                ticks = 1f / moveSpeed;
                //skip weather factor
                if (diagonal)
                    ticks *= 1.41421f;
            }

            return Mathf.Clamp(Mathf.RoundToInt(ticks), 1, 450);
        }

        //adjusts loaded movement value (cost) according to the status of the cell currently being polled

        readonly static SimpleCurve LightEvalPoints = new SimpleCurve() { new CurvePoint(0f, 0.8f), new CurvePoint(0.3f, 1.0f) };
        public static int TicksPerMove_tile(Map map, Pawn pawn, int newIndex, int cost)
        {
            //Friendly humans only, raiders and animals dont care about the dark
            if (notfriendlyHuman)
                return cost;

            float mod = 1f;

            if (Settings.lightPatch && glowActiveForPawn) //pawns that are blind, worship darkness, or use certain genes don't recieve movement debuffs in the dark
            {
                float glow = map.glowGrid.GameGlowAt(map.cellIndices.IndexToCell(newIndex));
                mod *= LightEvalPoints.Evaluate(glow);
            }

            if (Settings.weatherPatch && pawn.Spawned && !map.roofGrid.Roofed(newIndex))
            {
                mod *= weatherSpeedMult;
            }

            float ticks = (mod != 0f) ? (cost / mod) : cost;
            return Mathf.Clamp(Mathf.RoundToInt(ticks), 1, 450);
        }

        static float GameGlow()
        {
            return 1;
        }

        static bool notfriendlyHuman;
        static bool glowActiveForPawn;
        static float weatherSpeedMult;
        public static void CacheEnvironmental(Map map, Pawn pawn)
        {
            notfriendlyHuman = pawn == null || !pawn.RaceProps.Humanlike || pawn.HostileTo(Faction.OfPlayer);
            glowActiveForPawn = MoveSpeed_Glow_ActiveFor(pawn);
            weatherSpeedMult = map.weatherManager.CurMoveSpeedMultiplier;
        }

        static bool MoveSpeed_Glow_ActiveFor(Pawn pawn)
        {
            //RimWorld.StatPart_Glow:ActiveFor
            if (notfriendlyHuman)
            {
                return false;
            }
            if (PawnUtility.IsBiologicallyOrArtificiallyBlind(pawn))
            {
                return false;
            }
            if (pawn.Ideo != null && pawn.Ideo.IdeoPrefersDarkness())
            {
                return false;
            }
            if (pawn.genes != null && !pawn.genes.AffectedByDarkness)
            {
                return false;
            }
            return pawn.Spawned;
        }
    }
}
