using Verse;

namespace DIL_PositiveConnections
{
    public class PositiveConnectionsModSettings : Verse.ModSettings
    {
        public bool EnableGenderAdjustment; // Remove static modifier
        public bool DisableAllMessages; // Remove static modifier
        public float BaseInteractionFrequency = 1f;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref EnableGenderAdjustment, "EnableGenderAdjustment", true);
            Scribe_Values.Look(ref DisableAllMessages, "DisableAllMessages", false); // Add this line to expose the DisableAllMessages setting
        }
    }
}

