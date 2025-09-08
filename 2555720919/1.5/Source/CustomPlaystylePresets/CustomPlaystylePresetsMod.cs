using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace CustomPlaystylePresets
{
    class CustomPlaystylePresetsMod : Mod
    {
        public static CustomPlaystylePresetsSettings settings;
        public CustomPlaystylePresetsMod(ModContentPack pack) : base(pack)
        {
            LongEventHandler.ExecuteWhenFinished(delegate
            {
                settings = GetSettings<CustomPlaystylePresetsSettings>();
                if (settings.presets is null)
                {
                    settings.presets = new HashSet<PresetStorage>();
                }
                new Harmony("CustomPlaystylePresets.Mod").PatchAll();
            });
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            settings.DoSettingsWindowContents(inRect);
        }
        public static void AddNewDifficultyDef(PresetStorage presetStorage)
        {
            var customDifficultyDef = CreateNewDifficultyDef(presetStorage);
            DefDatabase<DifficultyDef>.Add(customDifficultyDef);
        }

        public static DifficultyDef CreateNewDifficultyDef(PresetStorage src)
        {
            var newDef = new DifficultyDef { defName = "CPP_" + src.name.Replace(" ", string.Empty), label = src.name, description = src.name };
            src.difficulty.CopyFieldsTo(newDef);
            newDef.isCustom = true;
            return newDef;
        }

        public override string SettingsCategory()
        {
            return "Custom Playstyle Presets";
        }

        public override void WriteSettings()
        {
            base.WriteSettings();
        }
    }

    [StaticConstructorOnStartup]
    public static class Startup
    {
        static Startup()
        {
            if (CustomPlaystylePresetsMod.settings.DefaultPreset != null)
            {
                CustomPlaystylePresetsMod.AddNewDifficultyDef(CustomPlaystylePresetsMod.settings.DefaultPreset);
            }
        }
    }
}