using HugsLib;
using HugsLib.Settings;
using UnityEngine;
using Verse;

namespace BlueprintMaterialDebt
{
    [StaticConstructorOnStartup]
    public class BlueprintMaterialDebt : ModBase
    {
        public override string ModIdentifier => "me.lubar.BlueprintMaterialDebt";

        public static bool SubtractResources = true;

        internal static Texture2D ToggleIcon = ContentFinder<Texture2D>.Get("BlueprintMaterialDebtOverlayIcon");

        internal static SettingHandle<bool> IncludeForbidden;

        public override void DefsLoaded()
        {
            IncludeForbidden = Settings.GetHandle<bool>(
                "includeForbidden",
                "BlueprintMaterialDebt_includeForbidden_title".Translate(),
                "BlueprintMaterialDebt_includeForbidden_desc".Translate(),
                true
            );
        }
    }
}
