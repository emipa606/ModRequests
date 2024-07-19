using RimWorld;
using System.Linq;
using UnityEngine;
using Verse;

namespace FinishingTouchesByTheBestBuilder
{
    [StaticConstructorOnStartup]
    internal static class Images
    {
        internal static readonly Texture2D TEX_ICON_TINY = ContentFinder<Texture2D>.Get("UI/Icons/FT_ICON_TINY");
        internal static readonly Texture2D TEX_ICON_RELOAD_ON = ContentFinder<Texture2D>.Get("UI/Icons/FT_RELOAD_ON");
        internal static readonly Texture2D TEX_ICON_RELOAD_OFF = ContentFinder<Texture2D>.Get("UI/Icons/FT_RELOAD_OFF");

        internal static readonly Texture2D TEX_BG_ICON = ActiveTip.TooltipBGAtlas;
        internal static readonly Texture2D TEX_BG_BUTTON = Widgets.ButtonSubtleAtlas;
    }
}
