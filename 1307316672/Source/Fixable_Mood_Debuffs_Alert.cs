using UnityEngine;
using Verse;

namespace Fixable_Mood_Debuffs_Alert
{
    class Fixable_Mood_Debuffs_Alert : Mod
    {
        public static Fixable_Mood_Debuffs_Alert_Settings settings;

        public Fixable_Mood_Debuffs_Alert(ModContentPack content) : base(content)
        {
            settings = GetSettings<Fixable_Mood_Debuffs_Alert_Settings>();
        }

        public override string SettingsCategory() => "Fixable Mood Debuffs Alert";

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listing_Standard = new Listing_Standard();
            listing_Standard.Begin(inRect);
            listing_Standard.CheckboxLabeled("Alert when colonists with animal bonds aren't masters: ", ref settings.alertOnWrongMaster);
            listing_Standard.CheckboxLabeled("Alert when night-owl colonists are working in the daytime: ", ref settings.alertOnNightOwlInDay);
            listing_Standard.CheckboxLabeled("Alert when nudists are wearing clothes: ", ref settings.alertOnClothedNudist);
            listing_Standard.CheckboxLabeled("Alert when colonists want a different bedroom: ", ref settings.alertOnBedroom);
            listing_Standard.End();
            settings.Write();
        }
    }
}
