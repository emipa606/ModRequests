using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;
using Verse;

namespace YayoCombatWarcaskets
{
    [UsedImplicitly]
    public class YayoCombatWarcasketsMod : Mod
    {
        private static Harmony harmony;
        internal static Harmony Harmony => harmony ??= new Harmony("Dra.YayoCombatWarcaskets");
        public static YayoCombatWarcasketsSettings settings;

        public YayoCombatWarcasketsMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<YayoCombatWarcasketsSettings>();

            Harmony.PatchAll();
            if (!Mathf.Approximately(settings.warcasketSpawnCostPercent, 1f))
                HarmonyManualPatches.ToggleWarcasketPointChange();
            if (settings.patchBulletproof)
                HarmonyManualPatches.ToggleBulletproof();
            if (settings.patchBioticWarp)
                HarmonyManualPatches.ToggleBioticWarp();

            var hasYayo = false;
            var hasVfeCore = false;
            var hasVfePirates = false;
            var hasRimEffect = false;

            foreach (var mod in LoadedModManager.RunningMods)
            {
                var id = mod.PackageId.ToLower().NoModIdSuffix();

                if (!hasYayo && id is "com.yayo.combat3" or "mlie.yayoscombat3")
                    hasYayo = true;
                else if (!hasVfeCore && id is "oskarpotocki.vanillafactionsexpanded.core")
                    hasVfeCore = true;
                else if (!hasVfePirates && id is "oskarpotocki.vfe.pirates")
                    hasVfePirates = true;
                else if (!hasRimEffect && id is "rimeffect.core")
                    hasRimEffect = true;
            }

            switch (hasYayo, hasVfeCore, hasVfePirates, hasRimEffect)
            {
                case (_, _, _, true):
                    break;
                case (false, _, true, _):
                    Log.Warning("[Yayo's Combat Warcaskets] - no Yayo's Combat is running, having this mod enabled is pointless unless you're changing warcasket raid point cost.");
                    break;
                case (false, _, _, _):
                    Log.Error("[Yayo's Combat Warcaskets] - no Yayo's Combat is running, having this mod enabled is pointless.");
                    break;
                case (true, false, false, false):
                    Log.Error("[Yayo's Combat Warcaskets] - no supported VFE mod is running, having this mod enabled is pointless.");
                    break;
            }
        }

        public override void DoSettingsWindowContents(Rect inRect) => settings.DoSettingsWindowContents(inRect);

        public override string SettingsCategory() => "Yayo's Combat Warcaskets";
    }
}