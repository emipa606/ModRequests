using UnityEngine;
using Verse;

namespace TempoDeRecreacao
{
    public class RecreationDraftMod : Mod
    {
        public static RecreationDraftSettings settings;

        public RecreationDraftMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<RecreationDraftSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(inRect);
            listing.CheckboxLabeled("Enable Automatic Draft", ref settings.enableDrafting);
            listing.CheckboxLabeled("Show Notifications", ref settings.enableNotifications);
            listing.Label($"Draft Duration: {settings.draftDurationSeconds:0.0}s");
            settings.draftDurationSeconds = listing.Slider(settings.draftDurationSeconds, 0.1f, 1f);
            listing.End();
        }

        public override string SettingsCategory() => "Recreation Time";
    }
}
