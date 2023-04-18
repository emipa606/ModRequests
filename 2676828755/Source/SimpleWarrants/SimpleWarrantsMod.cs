using HarmonyLib;
using UnityEngine;
using Verse;

namespace SimpleWarrants
{
    internal class SimpleWarrantsMod : Mod
    {
        public static SimpleWarrantsSettings Settings { get; private set; }

        public SimpleWarrantsMod(ModContentPack mod) : base(mod)
        {
            Settings = GetSettings<SimpleWarrantsSettings>();
            new Harmony("SimpleWarrants.Mod").PatchAll();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            Settings.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return Content.Name;
        }
    }
}
