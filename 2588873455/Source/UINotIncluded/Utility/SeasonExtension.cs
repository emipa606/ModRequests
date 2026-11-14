using System;
using RimWorld;
using UnityEngine;

namespace UINotIncluded
{
    public static class SeasonExtension
    {
        public static Texture2D GetIconTex(this Season season)
        {
            switch (season)
            {
                case Season.Undefined:
                    return ModTextures.weatherUnknown;

                case Season.Spring:
                    return ModTextures.seasonSpring;

                case Season.Summer:
                    return ModTextures.seasonSummer;

                case Season.Fall:
                    return ModTextures.seasonFall;

                case Season.Winter:
                    return ModTextures.seasonWinter;

                case Season.PermanentSummer:
                    return ModTextures.seasonPermaSummer;

                case Season.PermanentWinter:
                    return ModTextures.seasonPermaWinter;

                default:
                    throw new NotImplementedException();
            }
        }
    }
}