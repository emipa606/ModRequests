﻿using HarmonyLib;
using RimWorld;
using System;
using System.Reflection.Emit;
using System.Collections.Generic;
using Verse;
using Settings = Tacticowl.ModSettings_Tacticowl;

namespace Tacticowl.DualWield
{
    //Tick the stance tracker of the offfhand weapon
    [HarmonyPatch(typeof(Verb), nameof(Verb.TryStartCastOn), new Type[] { typeof(LocalTargetInfo), typeof(LocalTargetInfo), typeof(bool), typeof(bool), typeof(bool), typeof(bool) } )]
    class Patch_TryStartCastOn
    {
        static bool Prepare()
        {
            return Settings.runAndGunEnabled || Settings.dualWieldEnabled;
        }
        
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> inst, ILGenerator generator)
        {
            var instructions = new List<CodeInstruction>(inst);
            var label = generator.DefineLabel();
            bool labelNext = false;
            foreach (CodeInstruction instruction in instructions)
            {
                if (labelNext)
                {
                    instruction.labels.Add(label);
                    break;
                }

                if (!labelNext && instruction.opcode == OpCodes.Callvirt && instruction.OperandIs(AccessTools.Method(typeof(Pawn_StanceTracker), nameof(Pawn_StanceTracker.SetStance))))
                    labelNext = true;
                
            }
            
            bool found = false;
            foreach (CodeInstruction instruction in instructions)
            {
                yield return instruction;
                if (!found && instruction.opcode == OpCodes.Stloc_2)
                {
                    found = true;
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldloc_2); //ticks
                    yield return new CodeInstruction(OpCodes.Ldarg_1); //castTarg
                    yield return new CodeInstruction(OpCodes.Call, typeof(Patch_TryStartCastOn).GetMethod(nameof(CheckTactics)));
                    yield return new CodeInstruction(OpCodes.Brtrue, label); //castTarg
                }
            }
            if (!found) Log.Error("[Tacticowl] Patch_TryStartCastOn transpiler failed to find its target. Did RimWorld update?");
        }

        static void Postfix(ref bool __result, Verb __instance, LocalTargetInfo castTarg)
        {
            //Check if it's an enemy that's attacked, and not a fire or an arguing husband
            //TODO: this could probably be transpiled in somehow
            if (Settings.dualWieldEnabled && !__instance.EquipmentSource.IsOffHandedWeapon() && __instance.caster is Pawn casterPawn && !casterPawn.InMentalState && castTarg.Thing != null && castTarg.Thing is not Fire && castTarg.Thing.Faction != casterPawn.Faction)
                __result = DualWieldUtility.TryStartOffHandAttack(casterPawn, castTarg, __result);
            
        }
        

        public static bool CheckTactics(Verb verb, int ticks, LocalTargetInfo castTarg)
        {
            Pawn pawn = verb == null ? null : verb.CasterPawn;
            if (pawn == null || pawn.RaceProps == null || pawn.RaceProps.Animal) return false;

            if (Settings.runAndGunEnabled && pawn.CurJobDef == JobDefOf.Goto && pawn.RunsAndGuns())
            {
                var curStance = pawn.stances.curStance;
                if (curStance is Stance_RunAndGun_Cooldown) return true;
                if (curStance is not Stance_RunAndGun)
                {
                    pawn.stances.SetStance(new Stance_RunAndGun(ticks, castTarg, verb));
                    return true;
                }
            }

            if (Settings.dualWieldEnabled && verb.EquipmentSource.IsOffHandedWeapon())
            {
                pawn.GetOffHandStanceTracker().SetStance(new Stance_Warmup_DW(ticks, castTarg, verb));
                return true;
            }
            return false;
        }
    }
}
