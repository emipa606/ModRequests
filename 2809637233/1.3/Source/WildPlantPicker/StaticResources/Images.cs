using RimWorld;
using System.Linq;
using UnityEngine;
using Verse;

namespace WildPlantPicker.StaticResources
{
    [StaticConstructorOnStartup]
    internal static class Images
    {
        internal static readonly Texture2D TEX_ICON_BIG = ContentFinder<Texture2D>.Get("UI/Icons/WPP_ICON_BIG");
        internal static readonly Texture2D TEX_ICON_TINY = ContentFinder<Texture2D>.Get("UI/Icons/WPP_ICON_TINY");
        internal static readonly Texture2D TEX_ICON_EARTH = ContentFinder<Texture2D>.Get("UI/Icons/WPP_ICON_EARTH");
        internal static readonly Texture2D TEX_HARVEST_PLANT = ContentFinder<Texture2D>.Get("Designations/HarvestPlant");

        internal static readonly Texture2D TEX_BG_ICON = ActiveTip.TooltipBGAtlas;
        internal static readonly Texture2D TEX_BG_BUTTON = Widgets.ButtonSubtleAtlas;
    }
}
