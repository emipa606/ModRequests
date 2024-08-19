using HarmonyLib;
using UnityEngine;
using Verse;

namespace WoodIsNaturalForTrees;

public class WoodIsNaturalForTreesMod : Mod
{
    private static Harmony harmony;
    internal static Harmony Harmony => harmony ??= new Harmony("Dra.WoodIsNaturalForTrees");
    public static WoodIsNaturalForTreesSettings settings;

    public WoodIsNaturalForTreesMod(ModContentPack content) : base(content)
    {
        LongEventHandler.ExecuteWhenFinished(() => settings = GetSettings<WoodIsNaturalForTreesSettings>());

        Harmony.PatchAll();

        if (!ModLister.RoyaltyInstalled && !ModLister.IdeologyInstalled
#if BIOTECH_PLUS
                                        && !ModLister.biotechInstalled
#endif
           )
            Log.Warning("[WoodIsNaturalForTrees] No supported DLC detected, this mod is (most likely) pointless");
    }

    public override void DoSettingsWindowContents(Rect inRect) => settings.DoSettingsWindowContents(inRect);

    public override string SettingsCategory() => "Wood Is Natural (For Trees)";
}