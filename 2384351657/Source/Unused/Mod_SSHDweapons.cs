using System.Reflection;
using UnityEngine;
using RimWorld;
using Verse;
using HarmonyLib;

namespace SSTHDweapons
{
    public class Mod_SSTHDweapons : Mod
    {
        Listing_Standard listingStandard = new Listing_Standard();

        public Mod_SSTHDweapons(ModContentPack content) : base(content)
        {
            GetSettings<ModSettings_SSTHDweapons>();
            Harmony harmony = new Harmony("rimworld.varietymattersfashion");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
        public override string SettingsCategory()
        {
            return "SSTH Weapons always visible";
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Rect rect = new Rect(10f, 50f, inRect.width * .6f, inRect.height);
            listingStandard.Begin(rect);
            listingStandard.CheckboxLabeled("Water is cold: ", ref ModSettings_SSTHDweapons.weaponsalwaysvisible);
            listingStandard.GapLine();
        }
        private void LabeledIntEntry(Rect rect, string label, ref int value, ref string editBuffer, int multiplier, int largeMultiplier, int min, int max)
        {
            float num = rect.width / 15f;
            Widgets.Label(rect, label);
            if (multiplier != largeMultiplier)
            {
                if (Widgets.ButtonText(new Rect(rect.xMax - num * 5f, rect.yMin, (float)num, rect.height), (-1 * largeMultiplier).ToString(), true, true, true))
                {
                    value -= largeMultiplier * GenUI.CurrentAdjustmentMultiplier();
                    editBuffer = value.ToString();
                    SoundDefOf.Checkbox_TurnedOff.PlayOneShotOnCamera(null);
                }
                if (Widgets.ButtonText(new Rect(rect.xMax - num, rect.yMin, num, rect.height), "+" + largeMultiplier.ToString(), true, true, true))
                {
                    value += largeMultiplier * GenUI.CurrentAdjustmentMultiplier();
                    editBuffer = value.ToString();
                    SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera(null);
                }
            }
            if (Widgets.ButtonText(new Rect(rect.xMax - num * 4f, rect.yMin, num, rect.height), (-1 * multiplier).ToString(), true, true, true))
            {
                value -= multiplier * GenUI.CurrentAdjustmentMultiplier();
                editBuffer = value.ToString();
                SoundDefOf.Checkbox_TurnedOff.PlayOneShotOnCamera(null);
            }
            if (Widgets.ButtonText(new Rect(rect.xMax - (num * 2f), rect.yMin, num, rect.height), "+" + multiplier.ToString(), true, true, true))
            {
                value += multiplier * GenUI.CurrentAdjustmentMultiplier();
                editBuffer = value.ToString();
                SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera(null);
            }
            Widgets.TextFieldNumeric<int>(new Rect(rect.xMax - (num * 3f), rect.yMin, num, rect.height), ref value, ref editBuffer, min, max);
        }
    }
}