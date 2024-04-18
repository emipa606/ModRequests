using HarmonyLib;
using RimWorld;
using System;
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

        internal static readonly Texture2D TexButton_Add;
        internal static readonly Texture2D TexButton_Empty;
        internal static readonly Texture2D TexButton_CloseXSmall;
        internal static readonly Texture2D TexButton_ShowExpandingIcons;
        internal static readonly Texture2D TexButton_ShowZones;
        internal static readonly Texture2D TexButton_Collapse;
        internal static readonly Texture2D TexButton_Reveal;
        internal static readonly Texture2D TexButton_Plus;
        internal static readonly Texture2D TexButton_Minus;

        static Images()
        {
            Type typeTexButton = AccessTools.TypeByName("Verse.TexButton");
            if (typeTexButton != null)
            {
                TexButton_Add = AccessTools.Field(typeTexButton, "Add")?.GetValue(null) as Texture2D ?? BaseContent.BadTex;
                TexButton_Empty = AccessTools.Field(typeTexButton, "Empty")?.GetValue(null) as Texture2D ?? BaseContent.BadTex;
                TexButton_CloseXSmall = AccessTools.Field(typeTexButton, "CloseXSmall")?.GetValue(null) as Texture2D ?? BaseContent.BadTex;
                TexButton_ShowExpandingIcons = AccessTools.Field(typeTexButton, "ShowExpandingIcons")?.GetValue(null) as Texture2D ?? BaseContent.BadTex;
                TexButton_ShowZones = AccessTools.Field(typeTexButton, "ShowZones")?.GetValue(null) as Texture2D ?? BaseContent.BadTex;
                TexButton_Collapse = AccessTools.Field(typeTexButton, "Collapse")?.GetValue(null) as Texture2D ?? BaseContent.BadTex;
                TexButton_Reveal = AccessTools.Field(typeTexButton, "Reveal")?.GetValue(null) as Texture2D ?? BaseContent.BadTex;
                TexButton_Plus = AccessTools.Field(typeTexButton, "Plus")?.GetValue(null) as Texture2D ?? BaseContent.BadTex;
                TexButton_Minus = AccessTools.Field(typeTexButton, "Minus")?.GetValue(null) as Texture2D ?? BaseContent.BadTex;
            }
        }
    }
}
