using UnityEngine;
using Verse;

namespace StrayBullets
{
    public class Main : Mod
    {
        Settings _settings;

        public Main(ModContentPack content) : base(content)
        {
            _settings = GetSettings<Settings>();
        }
        public override string SettingsCategory()
        {
            return Translator.Translate("setting_title");
        }
        public override void DoSettingsWindowContents(Rect inRect)
        {
            _settings.DrawWindow(inRect);
        }
        public static void LogMessage(string message, bool forceLog = false, bool ignoreLoggingLimit = false)
        {
           if(Settings.Debug || forceLog)
                Log.Message($"[Stray Bullets] {message}", ignoreLoggingLimit);
        }
        public static void LogWarning(string message)
        {
            Log.Warning($"[Stray Bullets] {message}");
        }
    }
}
