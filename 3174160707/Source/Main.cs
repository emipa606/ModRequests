using Reload.Data;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;

namespace Reload
{
    public class Main : Mod
    {
        Setting _settings;

        public Main(ModContentPack content) : base(content)
        {
            Func<string, ReloadData> func = ReloadData_Parse.func;
            if (func == null)
            {
                ReloadData_Parse.func = new Func<string, ReloadData>(ReloadData.FromString);
                func = ReloadData_Parse.func;
            }
            ParseHelper.Parsers<ReloadData>.Register(func);

            _settings = GetSettings<Setting>();
        }
        public override string SettingsCategory()
        {
            return "ModTitle".Translate();
        }
        public override void DoSettingsWindowContents(Rect inRect)
        {
            _settings.DrawWindow(inRect);
        }
        public static void LogMessage(string message)
        {
            Log.Message($"[Reload] {message}");
        }
        public static void LogWarning(string message)
        {
            Log.Warning($"[Reload] {message}");
        }

        [CompilerGenerated]
        static class ReloadData_Parse
        {
            public static Func<string, ReloadData> func;
        }
    }
}
