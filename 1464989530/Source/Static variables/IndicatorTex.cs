// Nightvision NightVision IndicatorTex.cs
// 
// 28 04 2018
// 
// 21 07 2018

using JetBrains.Annotations;
using UnityEngine;
using Verse;

namespace NightVision
{
    /// <summary>
    /// Textures for the mod settings menu
    /// </summary>
    [StaticConstructorOnStartup]
    [UsedImplicitly]
    public class IndicatorTex
    {
        public static readonly Texture2D DefIndicator =
                    ContentFinder<Texture2D>.Get("UI/Indicators/DefaultArrow");

        public static readonly Texture2D NvIndicator = ContentFinder<Texture2D>.Get("UI/Indicators/NVarrow");
        public static readonly Texture2D PsIndicator = ContentFinder<Texture2D>.Get("UI/Indicators/PSarrow");
    }
}