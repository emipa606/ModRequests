using System;
using UnityEngine;
using Verse;

namespace Revolus.Compressor {
    internal class CompressorSettings : ModSettings {
        internal static readonly bool prettyDefault = false;
        internal bool pretty = prettyDefault;

        internal static readonly int levelDefault = 0;
        internal int level = levelDefault;

        public override void ExposeData() {
            Scribe_Values.Look(ref this.pretty, "pretty", prettyDefault);
            Scribe_Values.Look(ref this.level, "level", levelDefault);
            base.ExposeData();
        }

        internal bool ShowAndChangeSettings(Listing_Standard listing) {
            int levelNew, levelOld = Math.Max(-1, Math.Min(this.level, +1));
            bool prettyNew, prettyOld = this.pretty;

            {
                Rect wholeRect = listing.GetRect(Text.LineHeight);
                Rect labelRect = wholeRect.LeftHalf().Rounded();
                Rect dataRect = wholeRect.RightHalf().Rounded();
                Rect dataSelectRect = dataRect.LeftHalf().Rounded();
                Rect dataDescRect = dataRect.RightHalf().Rounded();

                Text.Anchor = TextAnchor.MiddleRight;
                Widgets.Label(labelRect, "Compression level: ");

                Text.Anchor = TextAnchor.MiddleCenter;
                levelNew = Mathf.RoundToInt(Widgets.HorizontalSlider(dataSelectRect, levelOld, -1, +1));

                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(
                    dataDescRect,
                    levelNew < 0 ? " (uncompressed)" :
                    levelNew > 0 ? " (best compression)" : " (fastest compression)"
                );

                listing.Gap(listing.verticalSpacing + Text.LineHeight);
            }

            {
                Rect wholeRect = listing.GetRect(Text.LineHeight);
                Rect labelRect = wholeRect.LeftHalf().Rounded();
                Rect valueRect = wholeRect.RightHalf().Rounded();

                Text.Anchor = TextAnchor.MiddleRight;
                Widgets.Label(labelRect, "Pretty print XML (use indentation): ");

                Text.Anchor = TextAnchor.MiddleLeft;
                prettyNew = prettyOld;
                Widgets.CheckboxLabeled(valueRect, "", ref prettyNew, placeCheckboxNearText:true);

                listing.Gap(listing.verticalSpacing + Text.LineHeight);
            }

            var changed = false;
            if (prettyNew != prettyOld) {
                this.pretty = prettyNew;
                changed = true;
            }
            if (levelNew != levelOld) {
                this.level = levelNew;
                changed = true;
            }
            return changed;
        }
    }
}
