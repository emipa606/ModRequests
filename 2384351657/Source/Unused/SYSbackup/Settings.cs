using Verse;

namespace SYS
{
    /// <summary>
    /// Definition of the settings for the mod
    /// </summary>
    internal class SYSSettings : ModSettings
    {
        public bool AlwaysVisible = true;

        /// <summary>
        /// Saving and loading the values
        /// </summary>
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref AlwaysVisible, "AlwaysVisible", true, false);
        }
    }
}