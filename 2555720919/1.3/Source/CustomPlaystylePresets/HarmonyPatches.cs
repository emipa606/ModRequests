using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace CustomPlaystylePresets
{
    [HarmonyPatch(typeof(StorytellerUI), "DrawStorytellerSelectionInterface")]
    public static class DrawStorytellerSelectionInterface_Patch
    {
        public static PresetStorage customStorage;
        public static DifficultyDef customDifficultyDef;
        public static DifficultyDef curDifficultyDef;
        public static Dictionary<Difficulty, DifficultyDef> mappedDifficulties = new Dictionary<Difficulty, DifficultyDef>();

        public static void Postfix(Rect rect, ref StorytellerDef chosenStoryteller, ref DifficultyDef difficulty, ref Difficulty difficultyValues, Listing_Standard infoListing)
        {
            Log.ResetMessageCount();
            if (!mappedDifficulties.ContainsKey(difficultyValues))
            {
                mappedDifficulties[difficultyValues] = difficulty;
            }
            if (customStorage != null)
            {
                difficultyValues = customStorage.CopyOf(customStorage.difficulty);
                difficulty = customDifficultyDef;
                customStorage = null;
                customDifficultyDef = null;
            }
            else if (difficulty != null)
            {
                foreach (var preset in CustomPlaystylePresetsMod.settings.presets)
                {
                    if (preset.DefName == difficulty.defName)
                    {
                        if (difficulty != curDifficultyDef)
                        {
                            CustomPlaystylePresetsMod.settings.SetDefaultPreset(preset);
                            difficultyValues = preset.CopyOf(preset.difficulty);
                        }
                        else
                        {
                            if (mappedDifficulties.TryGetValue(difficultyValues, out var difficultyDef))
                            {
                                if (difficultyDef != null && difficultyDef.defName == preset.DefName)
                                {
                                    preset.difficulty = preset.CopyOf(difficultyValues);
                                    CustomPlaystylePresetsMod.settings.Write();
                                }
                            }
                        }
                        break;
                    }
                }
                curDifficultyDef = difficulty;
            }
        }
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr, ILGenerator ilGen)
        {
            var list = instr.ToList();
            var methodInfo = AccessTools.PropertyGetter(typeof(Current), "ProgramState");
            bool found = false;
            for (var i = 0; i < list.Count; i++)
            {
                var inst = list[i];
                yield return inst;
                if (!found && inst.opcode == OpCodes.Call && inst.operand is MethodInfo info && info == methodInfo)
                {
                    found = true;
                    yield return new CodeInstruction(OpCodes.Ldarg_S, 4);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DrawStorytellerSelectionInterface_Patch), "AddNewPresetButton", null, null));
                }
            }
        }
        public static void AddNewPresetButton(Listing_Standard listing)
        {
            if (listing.ButtonText("CPP.SelectOtherPresets".Translate()) && CustomPlaystylePresetsMod.settings.presets.Any(x => x != CustomPlaystylePresetsMod.settings.DefaultPreset))
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                foreach (var preset in CustomPlaystylePresetsMod.settings.presets)
                {
                    if (preset != CustomPlaystylePresetsMod.settings.DefaultPreset)
                    {
                        FloatMenuOption item = new FloatMenuOption(preset.name, delegate
                        {
                            CustomPlaystylePresetsMod.settings.SetDefaultPreset(preset);
                            CustomPlaystylePresetsMod.AddNewDifficultyDef(preset);
                        });
                        list.Add(item);
                    }
                }
                Find.WindowStack.Add(new FloatMenu(list));
            }
        }
    }

    [HarmonyPatch(typeof(StorytellerUI), "DrawCustomRight")]
    public static class DrawCustomRight_Patch
    {
        public static void Prefix(Listing_Standard listing, Difficulty difficulty)
        {
            listing.Gap(-32f);
            if (listing.ButtonText("CPP.SaveCurrentPreset".Translate()))
            {
                var newStorage = new PresetStorage(difficulty);
                var window = new Dialog_SetPresetName(newStorage);
                Find.WindowStack.Add(window);
            }
        }
    }
}