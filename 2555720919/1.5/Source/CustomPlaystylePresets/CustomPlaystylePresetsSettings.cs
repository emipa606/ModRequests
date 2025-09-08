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
    public class CustomPlaystylePresetsSettings : ModSettings
    {
        public PresetStorage DefaultPreset => presets.FirstOrDefault(x => x.isDefault);
        public void SetDefaultPreset(PresetStorage preset)
        {
            if (!presets.Contains(preset))
            {
                presets.Add(preset);
            }
            foreach (var preset2 in presets)
            {
                if (preset != preset2)
                {
                    preset2.isDefault = false;
                    var def = DefDatabase<DifficultyDef>.GetNamedSilentFail(preset2.DefName);
                    if (def != null)
                    {
                        DefDatabase<DifficultyDef>.Remove(def);
                    }
                }
            }
            preset.isDefault = true;
            this.Write();
        }

        public HashSet<PresetStorage> presets;
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref presets, "presets", LookMode.Deep);
        }
        public void AddNewDifficulty(PresetStorage presetStorage)
        {
            if (!presets.Contains(presetStorage))
            {
                presets.Add(presetStorage);
                this.Write();
            }
        }
        public void DoSettingsWindowContents(Rect inRect)
        {
            Widgets.BeginScrollView(inRect, ref scrollPosition, inRect, true);
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.Label("CPP.RemovePresets".Translate());
            listingStandard.GapLine();
            foreach (var preset in presets)
            {
                if (listingStandard.ButtonTextLabeled(preset.name, "CPP.RemovePreset".Translate()))
                {
                    Find.WindowStack.Add(new Dialog_MessageBox("CPP.DeleteConfirmation".Translate(preset.name), "Ok".Translate(), () =>
                    {
                        presets.Remove(preset);
                        if (presets.Any())
                        {
                            SetDefaultPreset(presets.First());
                        }
                    }, "CPP.Cancel".Translate()));
                }
            }
            listingStandard.End();
            Widgets.EndScrollView();
            base.Write();
        }
        private static Vector2 scrollPosition = Vector2.zero;

    }
}

