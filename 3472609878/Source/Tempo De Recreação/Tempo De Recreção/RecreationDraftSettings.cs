using Verse;

namespace TempoDeRecreacao
{
    public class RecreationDraftSettings : ModSettings
    {
        public bool enableDrafting = true;
        public bool enableNotifications = true;
        public float draftDurationSeconds = 0.3f;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref enableDrafting, "enableDrafting", true);
            Scribe_Values.Look(ref enableNotifications, "enableNotifications", true);
            Scribe_Values.Look(ref draftDurationSeconds, "draftDurationSeconds", 0.3f);
        }
    }
}
