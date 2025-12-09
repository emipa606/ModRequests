using Verse;

namespace TempoDeRecreacao
{
    public static class RecreationDraftSettingsHelper
    {
        public static bool Enabled => RecreationDraftMod.settings.enableDrafting;

        public static bool DraftingEnabled => RecreationDraftMod.settings.enableDrafting;

        public static bool NotificationsEnabled => RecreationDraftMod.settings.enableNotifications;

        public static int GetDraftDurationTicks()
        {
            // Converte segundos para ticks (60 ticks = 1 segundo no jogo)
            return (int)(RecreationDraftMod.settings.draftDurationSeconds * 60);
        }
    }
}
