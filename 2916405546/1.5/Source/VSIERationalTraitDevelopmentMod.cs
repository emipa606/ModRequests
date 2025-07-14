using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace VSIERationalTraitDevelopment
{
    [StaticConstructorOnStartup]
    public static class Startup
    {
        static Startup()
        {
            new Harmony("VSIERationalTraitDevelopmentMod").PatchAll();
        }
    }
    public class VSIERationalTraitDevelopmentMod : Mod
    {
        public static VSIERationalTraitDevelopmentSettings settings;
        public VSIERationalTraitDevelopmentMod(ModContentPack pack) : base(pack)
        {
            settings = GetSettings<VSIERationalTraitDevelopmentSettings>();
        }
        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            settings.DoSettingsWindowContents(inRect);
        }
        public override string SettingsCategory()
        {
            return Content.Name;
        }
    }

    public class VSIERationalTraitDevelopmentSettings : ModSettings
    {
        public static bool randomSelectionInsteadOfUI;

        public static int maxCountOfTraitsOfPools = 4;

        public static bool allowCancelUI = true;
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref randomSelectionInsteadOfUI, "randomSelectionInsteadOfUI", false);
            Scribe_Values.Look(ref allowCancelUI, "allowCancelUI", true);
            Scribe_Values.Look(ref maxCountOfTraitsOfPools, "maxCountOfTraitsOfPools", 4);
        }

        public void DoSettingsWindowContents(Rect inRect)
        {
            var ls = new Listing_Standard();
            ls.Begin(inRect);
            ls.CheckboxLabeled("VSIE.RandomSelectionInsteadOfUI".Translate(), ref randomSelectionInsteadOfUI);
            ls.CheckboxLabeled("VSIE.AddCancelButtonToUI".Translate(), ref allowCancelUI);
            maxCountOfTraitsOfPools = (int)ls.SliderLabeled("VSIE.MaxTraitCount".Translate(maxCountOfTraitsOfPools), maxCountOfTraitsOfPools, 1, 20);
            ls.End();
        }
	}
}
