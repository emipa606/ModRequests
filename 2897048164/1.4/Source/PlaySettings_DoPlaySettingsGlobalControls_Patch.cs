using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace RedAlert
{
    [StaticConstructorOnStartup]
    [HarmonyPatch(typeof(PlaySettings), "DoPlaySettingsGlobalControls")]
    public static class PlaySettings_DoPlaySettingsGlobalControls_Patch
    {
        public static readonly Texture2D icon = ContentFinder<Texture2D>.Get("UI/Icons/ToggleRedAlert");
        public static void Postfix(WidgetRow row, bool worldView)
        {
            if (worldView) return;
            var map = Find.CurrentMap; 
            if (map == null) return;
            var comp = map.GetComponent<MapComponent_RedAlert>();
            row.ToggleableIcon(ref comp.manualTrigger, icon, "RL.ToggleAlertDesc".Translate());
        }
    }
}
