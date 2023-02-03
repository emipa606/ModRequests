using Verse;

namespace ImprovedInsectoids
{
    public class ImprovedInsectoidsModSettings : ModSettings
    {
        public bool fearAffectsPlayerFaction = true;

        public bool fearEnabled = true;

        /// <summary>
        /// Saves and loads settings.
        /// </summary>
        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref fearAffectsPlayerFaction, "fearAffectsPlayerFaction", true, false);
            Scribe_Values.Look(ref fearEnabled, "fearEnabled", true, false);
        }
    }
}
