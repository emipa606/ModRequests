using System;
using Verse;
using UnityEngine;

namespace StevesDoors
{
    [StaticConstructorOnStartup]
    public static class EasingUtility
    {
        public static readonly Func<float, float> Linear = x => x;

        public static readonly Func<float, float> FadeOutLinear = x => 1 - x;

        public static readonly Func<float, float> FadeOutQuad = x => 1 - x * x;

        public static readonly Func<float, float> FadeOutCubic = x => 1 - x * x * x;

        public static readonly Func<float, float> Sine = x => Mathf.Sin(x * Mathf.PI);

        public static readonly Func<float, float> Cosine = x => Mathf.Cos(x * Mathf.PI);

        public static readonly Func<float, float> Tangent = x => Mathf.Tan(x * Mathf.PI);

        public static readonly Func<float, float> InverseSine = x => 1f - Sine(x);

        public static readonly Func<float, float> UnsignedSine = x => Mathf.Sin(2f * x * Mathf.PI);

        public static readonly Func<float, float> EaseInQuad = x => x * x;

        public static readonly Func<float, float> EaseOutQuad = x => 1 - (1 - x) * (1 - x);

        public static readonly Func<float, float> EaseInOutQuad = x =>
            x < 0.5 ? 2 * x * x : 1 - Mathf.Pow(-2 * x + 2, 2) / 2;

        public static readonly Func<float, float> EaseOutInQuad = x =>
            x < 0.5 ? (0.5f * EaseOutQuad(2f * x)) : (0.5f + 0.5f * EaseInQuad(2f * (x - 0.5f)));

        public static readonly Func<float, float> EaseInCubic = x => x * x * x;

        public static readonly Func<float, float> EaseOutCubic = x => 1 - Mathf.Pow(1 - x, 3);

        public static readonly Func<float, float> EaseInOutCubic =
            x => x < 0.5 ? 4 * x * x * x : 1 - Mathf.Pow(-2 * x + 2, 3) / 2;

        public static readonly Func<float, float> EaseOutInCubic = x =>
            x < 0.5 ? (0.5f * EaseOutCubic(2f * x)) : (0.5f + 0.5f * EaseInCubic(2f * (x - 0.5f)));

        public static readonly Func<float, float> Burst = x => Sine(EaseOutCubic(x));

        public static readonly Func<float, float> ReverseBurst = x => Sine(EaseInCubic(x));
    }
}
