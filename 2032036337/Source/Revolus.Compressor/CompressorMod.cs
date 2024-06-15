using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Revolus.Compressor {
    public class CompressorMod : Mod {
        internal static CompressorSettings Settings;
        internal static bool CurrentlySavingSavegame = false;

        public CompressorMod(ModContentPack content) : base(content) {
            Settings = this.GetSettings<CompressorSettings>();

            var harmony = new Harmony(typeof(CompressorMod).AssemblyQualifiedName);
            PatchRimWorld.DoPatch(harmony);
            PatchBetterModMismatchWindow.DoPatch(harmony);
        }

        public override void DoSettingsWindowContents(Rect rect) {
            bool changed;

            var listing = new Listing_Standard();
            listing.Begin(rect);
            try {
                TextAnchor oldAnchorValue = Text.Anchor;
                try {
                    changed = Settings.ShowAndChangeSettings(listing);
                } finally {
                    Text.Anchor = oldAnchorValue;
                }
            } finally {
                listing.End();
            }

            if (changed) {
                SoundDefOf.DragSlider.PlayOneShotOnCamera();
            }

            base.DoSettingsWindowContents(rect);
        }

        public override string SettingsCategory() {
            return "Savegame Compressor";
        }
    }
}
