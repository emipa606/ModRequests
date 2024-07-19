using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;

namespace FinishingTouchesByTheBestBuilder
{

    [HarmonyPatch(typeof(PlaySettings))]
    internal class PlaySettings_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(PlaySettings.DoPlaySettingsGlobalControls), new Type[] { typeof(WidgetRow), typeof(bool) })]
        static void DoPlaySettingsGlobalControls_Postfix(WidgetRow row, bool worldView)
        {
            if (row == null || WorldRendererUtility.WorldRenderedNow)
            {
                return;
            }
            DataContext dc = DataContext.current;
            int mapId = Find.CurrentMap.uniqueID;
            if (!dc.bestPawnStatesMap.TryGetValue(mapId, out HashSet<PawnState> bestPawns))
            {
                return;
            }
            bool preToggle = dc.enabled;
            row.ToggleableIcon(ref preToggle, Images.TEX_ICON_TINY, "FT_IconTooltip".Translate());
            if (preToggle != dc.enabled)
            {
                preToggle = !preToggle;
                if (!dc.settingDialog.opened)
                {
                    dc.bestPawnStatesMap[mapId] = bestPawns = bestPawns.OrderByDescending(x => x.skill).ThenBy(x => CommonHelper.GetSettingPriority(x.pawn)).ToHashSet();
                    Find.WindowStack.Add(dc.settingDialog);
                }
            }
        }
    }

    [StaticConstructorOnStartup]
    [HarmonyPatch(typeof(Pawn))]
    class Pawn_Patch
    {
        [HarmonyTranspiler]
        [HarmonyPatch(nameof(Pawn.SpawnSetup))]
        [HarmonyPatch(new Type[] { typeof(Map), typeof(bool) })]
        public static IEnumerable<CodeInstruction> SpawnSetup_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            int insertIndex = instructions.Count() - 1;
            CodeInstruction ci = null, ci2 = null;
            for (int i = insertIndex; i > 0; i--)
            {
                ci = instructions.ElementAt(i);
                ci2 = instructions.ElementAt(i - 1);
                if (ci.opcode == OpCodes.Ret && ci2.opcode != OpCodes.Ret)
                {
                    break;
                }
                insertIndex--;
            }

            for (int i = 0; i < instructions.Count(); i++)
            {
                ci = instructions.ElementAt(i);
                if (i == insertIndex)
                {
                    CodeInstruction newLdarg0 = new CodeInstruction(OpCodes.Ldarg_0);
                    foreach (Label label in ci.labels)
                    {
                        newLdarg0.labels.Add(label);
                    }

                    // Pawn個人の情報更新
                    yield return newLdarg0;
                    yield return CodeInstruction.Call(typeof(DataContext), nameof(DataContext.UpdatePawnState), new Type[] { typeof(Pawn) });
                    yield return new CodeInstruction(OpCodes.Ret);
                    continue;
                }
                yield return ci;
            }
        }
    }

    [StaticConstructorOnStartup]
    [HarmonyPatch(typeof(Pawn_WorkSettings))]
    class Pawn_WorkSettings_Patch
    {
        //[HarmonyTranspiler]
        //[HarmonyPatch(nameof(Pawn_WorkSettings.SetPriority))]
        //[HarmonyPatch(new Type[] { typeof(WorkTypeDef), typeof(int) })]
        //public static IEnumerable<CodeInstruction> SetPriority_Transpiler(IEnumerable<CodeInstruction> instructions)
        //{
        //    foreach (CodeInstruction ci in instructions)
        //    {
        //        if (ci.opcode == OpCodes.Ret)
        //        {
        //            // Pawn個人の情報更新
        //            CodeInstruction newLdarg0 = new CodeInstruction(OpCodes.Ldarg_0);
        //            foreach (Label label in ci.labels)
        //            {
        //                newLdarg0.labels.Add(label);
        //            }
        //            yield return newLdarg0;
        //            yield return CodeInstruction.LoadField(typeof(Pawn_WorkSettings), "pawn");
        //            yield return new CodeInstruction(OpCodes.Ldarg_1);
        //            yield return CodeInstruction.Call(typeof(DataContext), nameof(DataContext.UpdatePawnState), new Type[] { typeof(Pawn), typeof(WorkTypeDef) });
        //            yield return new CodeInstruction(OpCodes.Ret);
        //            continue;
        //        }
        //        yield return ci;
        //    }
        //}

        [HarmonyPostfix]
        [HarmonyPatch(nameof(Pawn_WorkSettings.SetPriority))]
        [HarmonyPatch(new Type[] { typeof(WorkTypeDef), typeof(int) })]
        public static void SetPriority_Postfix(WorkTypeDef w, Pawn ___pawn)
        {
            DataContext.UpdatePawnState(___pawn, w);
        }
    }

    //[StaticConstructorOnStartup]
    //[HarmonyPatch(typeof(Map))]
    //class Map_Patch
    //{
    //    [HarmonyTranspiler]
    //    [HarmonyPatch(nameof(Map.FinalizeInit))]
    //    public static IEnumerable<CodeInstruction> FinalizeInit_Transpiler(IEnumerable<CodeInstruction> instructions)
    //    {
    //        foreach (CodeInstruction ci in instructions)
    //        {
    //            if (ci.opcode == OpCodes.Ret)
    //            {
    //                // TODO Mapに紐付くPawnの情報更新
    //            }
    //            yield return ci;
    //        }
    //    }
    //}

    [StaticConstructorOnStartup]
    [HarmonyPatch(typeof(WorkGiver_ConstructFinishFrames))]
    class WorkGiver_ConstructFinishFrames_Patch
    {
        [HarmonyTranspiler]
        [HarmonyPatch(nameof(WorkGiver_ConstructFinishFrames.JobOnThing))]
        [HarmonyPatch(new Type[] { typeof(Pawn), typeof(Thing), typeof(bool) })]
        public static IEnumerable<CodeInstruction> JobOnThing_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase original)
        {
            bool matched = false;
            MethodInfo materialNeeded = AccessTools.Method(typeof(Frame), nameof(Frame.MaterialsNeeded));
            for (int i = 0, count = instructions.Count(), maxIndex = count - 1; i < count; i++)
            {
                CodeInstruction current = current = instructions.ElementAt(i);
                if (!matched && i > 0 && i <= maxIndex - 1)
                {
                    CodeInstruction pre = instructions.ElementAt(i - 1), next = instructions.ElementAt(i + 1);
                    if (pre.opcode == OpCodes.Ret && current.opcode == OpCodes.Ldloc_0 && next.opcode == OpCodes.Callvirt && (MethodInfo)next.operand == materialNeeded)
                    {
                        //Log.Message($"@@@JobOnThing_Transpiler　HIT!!! index={i}");

                        Label allowConstruct = generator.DefineLabel();

                        yield return current; //Ldloc_0 : frame
                        yield return new CodeInstruction(OpCodes.Ldarg_1); // pawn
                        yield return new CodeInstruction(OpCodes.Ldarg_3); // forced
                        yield return new CodeInstruction(OpCodes.Ldc_I4_0); // false: from WorkGiver
                        yield return CodeInstruction.Call(typeof(CommonHelper), nameof(CommonHelper.CanFinisheFrameAndBestPawn), new Type[] { typeof(Frame), typeof(Pawn), typeof(bool), typeof(bool) });

                        yield return new CodeInstruction(OpCodes.Brtrue_S, allowConstruct);

                        yield return new CodeInstruction(OpCodes.Ldnull);
                        yield return new CodeInstruction(OpCodes.Ret);

                        CodeInstruction newLdloc0 = new CodeInstruction(OpCodes.Ldloc_0);
                        newLdloc0.labels.Add(allowConstruct);
                        yield return newLdloc0;

                        matched = true;
                        continue;
                    }
                }
                yield return current;
            }
        }
    }

    [HarmonyPatch(typeof(InspirationHandler))]
    internal class InspirationHandler_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(InspirationHandler.TryStartInspiration), new Type[] { typeof(InspirationDef), typeof(string), typeof(bool) })]
        static void TryStartInspiration_Postfix(InspirationHandler __instance, InspirationDef def)
        {
            if (def == InspirationDefOf.Inspired_Creativity && __instance.CurStateDef == InspirationDefOf.Inspired_Creativity)
            {
                //熱狂的な創作意欲がついたので、最適Pawn再抽選処理
                DataContext.current.ValidateAndRefreshPawnStates();
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(InspirationHandler.EndInspiration), new Type[] { typeof(Inspiration) })]
        static void EndInspiration_Postfix(InspirationHandler __instance, Inspiration inspiration)
        {
            if (inspiration.def == InspirationDefOf.Inspired_Creativity && __instance.CurStateDef == null)
            {
                //熱狂的な創作意欲が終了したので、最適Pawn再抽選処理
                DataContext.current.ValidateAndRefreshPawnStates();
            }
        }
    }
}
