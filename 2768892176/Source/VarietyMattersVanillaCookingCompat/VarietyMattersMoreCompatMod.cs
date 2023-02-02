using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;
using Verse;

namespace VarietyMattersMoreCompat
{
    [UsedImplicitly]
    public class VarietyMattersMoreCompatMod : Mod
    {
        public static VarietyMattersMoreCompatSettings Settings;

        public VarietyMattersMoreCompatMod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<VarietyMattersMoreCompatSettings>();
            new Harmony("Dra.VarietyMattersMoreCompat").PatchAll();
            LongEventHandler.ExecuteWhenFinished(DefLists.Init);

            var hasVariety = false;
            var hasSupported = false;

            foreach (var mod in LoadedModManager.RunningMods)
            {
                var id = mod.PackageId.ToLower().NoModIdSuffix();

                if (!hasVariety && id is "cozarkian.varietymattersimproved")
                {
                    hasVariety = true;
                    if (hasSupported) break;
                }
                else if (!hasSupported && id is "vanillaexpanded.vcooke" or "mrkociak.morearchotechstuffandthingsreupload")
                {
                    hasSupported = true;
                    if (hasVariety) break;
                }
            }

            if (!hasVariety)
                Log.Error("[Variety Matters - More Compat] - Variety Matters is not running, having this mod enabled is pointless.");
            else if (!hasSupported)
                Log.Error("[Variety Matters - More Compat] - no supported mod is running, having this mod enabled is pointless.");
        }

        public override string SettingsCategory() => "Variety Matters - More Compat";
        public override void DoSettingsWindowContents(Rect inRect) => Settings.DoSettingsWindowContents(inRect);
    }
}