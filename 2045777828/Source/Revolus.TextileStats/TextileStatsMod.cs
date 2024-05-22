using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;

namespace Revolus.TextileStats {
    public class TextileStatsMod : Mod {
        public static Settings settings;

        public TextileStatsMod(ModContentPack content) : base(content) {
            RuntimeHelpers.RunClassConstructor(typeof(SpecialDef).TypeHandle);
            settings = this.GetSettings<Settings>();
        }

        public override string SettingsCategory() {
            return "Textile Stats";
        }

        public override void DoSettingsWindowContents(Rect rect) {
            var listing = new Listing_Standard();
            listing.Begin(rect);
            try {
                TextAnchor oldAnchorValue = Text.Anchor;
                try {
                    settings.ShowAndChangeSettings(listing);
                } finally {
                    Text.Anchor = oldAnchorValue;
                }
            } finally {
                listing.End();
            }
        }
    }
}
