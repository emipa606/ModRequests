using UnityEngine;
using RimWorld;
using Verse;

namespace JobEffects
{
    /// <summary>
    /// Season-driven multiply tints for foliage/soil flecks. Designed for the near-white
    /// LeafPale / soil motes so the multiply reads true. Fall returns a fresh random pick each
    /// call so a leaf shower looks like a mix of turning colours rather than one flat hue.
    /// </summary>
    public static class SeasonalTint
    {
        // Autumn palette — amber / orange / rust / brown.
        private static readonly Color[] FallLeaves =
        {
            new Color(0.93f, 0.62f, 0.22f), // amber
            new Color(0.86f, 0.42f, 0.16f), // orange
            new Color(0.74f, 0.27f, 0.15f), // rust red
            new Color(0.70f, 0.52f, 0.24f), // tan-brown
            new Color(0.88f, 0.74f, 0.30f), // gold
        };

        public static Color Leaf(Map map)
        {
            switch (SeasonOf(map))
            {
                case Season.Spring: return new Color(0.62f, 0.84f, 0.42f);
                case Season.Summer:
                case Season.PermanentSummer: return new Color(0.44f, 0.66f, 0.32f);
                case Season.Fall: return FallLeaves[Rand.Range(0, FallLeaves.Length)];
                case Season.Winter:
                case Season.PermanentWinter: return new Color(0.80f, 0.86f, 0.82f);
                default: return new Color(0.50f, 0.72f, 0.36f);
            }
        }

        public static Color Soil(Map map)
        {
            // Earthy browns; lighten/grey toward winter as the ground frosts.
            switch (SeasonOf(map))
            {
                case Season.Winter:
                case Season.PermanentWinter: return new Color(0.62f, 0.58f, 0.52f);
                case Season.Fall: return new Color(0.50f, 0.40f, 0.27f);
                case Season.Spring: return new Color(0.46f, 0.36f, 0.24f);
                default: return new Color(0.52f, 0.41f, 0.27f);
            }
        }

        private static Season SeasonOf(Map map)
        {
            if (map == null) return Season.Summer;
            return GenLocalDate.Season(map);
        }
    }
}
