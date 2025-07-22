using UnityEngine;
using Verse;

namespace YayoCombatWarcaskets
{
    public class YayoCombatWarcasketsSettings : ModSettings
    {
        public float forcedWarcasketDurabilityPercent = 1f;
        public float warcasketSpawnCostPercent = 1f;

        public bool patchBulletproof = true;
        public bool patchBioticWarp = true;

        public bool debugMode = false;

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref forcedWarcasketDurabilityPercent, nameof(forcedWarcasketDurabilityPercent), 1f);
            Scribe_Values.Look(ref warcasketSpawnCostPercent, nameof(warcasketSpawnCostPercent), 1f);
            Scribe_Values.Look(ref patchBulletproof, nameof(patchBulletproof), true);
            Scribe_Values.Look(ref patchBioticWarp, nameof(patchBioticWarp), true);
            Scribe_Values.Look(ref debugMode, nameof(debugMode), false);
        }

        public void DoSettingsWindowContents(Rect inRect)
        {
            var listing = new Listing_Standard();
            listing.Begin(inRect);
            listing.ColumnWidth = 270f;

            var text = forcedWarcasketDurabilityPercent switch
            {
                <= 0.0f => "WarcasketArmorStrengthAir",
                <= 0.2f => "WarcasketArmorStrengthPaper",
                <= 0.5f => "WarcasketArmorStrengthWood",
                <= 1.0f => "WarcasketArmorStrengthSteel",
                <= 1.5f => "WarcasketArmorStrengthSkySteel",
                <= 1.9f => "WarcasketArmorStrengthPlasteel",
                _ => "WarcasketArmorStrengthArchotech",
            };
            listing.Label($"{"WarcasketArmorStrength".Translate()} {forcedWarcasketDurabilityPercent * 100:f0}%");
            listing.Label(text.Translate());
            forcedWarcasketDurabilityPercent = GenMath.RoundTo(listing.Slider(forcedWarcasketDurabilityPercent, 0f, 2f), 0.05f);

            listing.Label($"{"WarcasketArmorSpawnCost".Translate()} {warcasketSpawnCostPercent * 100:f0}%");
            listing.Label("WarcasketArmorMoreIsLess".Translate());
            warcasketSpawnCostPercent = GenMath.RoundTo(listing.Slider(warcasketSpawnCostPercent, 0.25f, 3f), 0.05f);
            if (Mathf.Approximately(warcasketSpawnCostPercent, 1f))
            {
                if (HarmonyManualPatches.WarcasketCostInitialized)
                    HarmonyManualPatches.ToggleWarcasketPointChange();
            }
            else if (!HarmonyManualPatches.WarcasketCostInitialized)
                HarmonyManualPatches.ToggleWarcasketPointChange();

            var temp = patchBulletproof;
            listing.CheckboxLabeled("WarcasketPatchBulletproof".Translate(), ref patchBulletproof, "WarcasketPatchBulletproofTooltip".Translate());
            if (temp != patchBulletproof)
                HarmonyManualPatches.ToggleBulletproof();

            temp = patchBioticWarp;
            listing.CheckboxLabeled("WarcasketPatchBioticWarp".Translate(), ref patchBioticWarp, "WarcasketPatchBioticWarpTooltip".Translate());
            if (temp != patchBioticWarp)
                HarmonyManualPatches.ToggleBioticWarp();
            
            listing.CheckboxLabeled("WarcasketDebugMode".Translate(), ref debugMode, "WarcasketDebugModeTooltip".Translate());

            listing.End();
        }
    }
}
