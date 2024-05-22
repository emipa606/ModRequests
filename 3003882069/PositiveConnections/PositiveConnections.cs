using UnityEngine;
using Verse;

namespace DIL_PositiveConnections
{
    public class Mod_PositiveConnections : Mod
    {
        public static Mod_PositiveConnections Instance;

        public PositiveConnectionsModSettings settings; // Update the type here

        public Mod_PositiveConnections(ModContentPack content) : base(content)
        {
            Instance = this;
            settings = GetSettings<PositiveConnectionsModSettings>(); // Update the type here
            Log.Message("Positive connections v1.1.5");
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            if (settings == null)
            {
                Log.Error("ModSettings for PositiveConnections is null");
                return;
            }

            // Open the settings window
            var settingsWindow = new PositiveConnectionsModSettingsWindow(settings);
            settingsWindow.DoWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "Positive Connections";
        }

        static void Mod_PositiveConnections_PostInit()
        {

        }
    }
}
