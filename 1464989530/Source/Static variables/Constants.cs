using System;

namespace NightVision
{
    public static class Constants
    {


        // Default values for various settings
        public static readonly float[] NVDefaultOffsets = { 0.2f, 0f };
        public static readonly float[] PSDefaultOffsets = { 0.4f, -0.2f };
        public const float DEFAULT_FULL_LIGHT_MULTIPLIER = 1f;
        public const float DEFAULT_MAX_CAP = 1.2f;
        public const float DEFAULT_MIN_CAP = 0.8f;
        public const float DEFAULT_ZERO_LIGHT_MULTIPLIER = 0.8f;

        // Fixed gameplay constants
        public const float MAX_GLOW_NO_GLOW = 0.7f;
        public const float MIN_GLOW_NO_GLOW = 0.3f;
        public const int THOUGHT_ACTIVE_TICKS_PAST = 240;

        // Calculation constants
        public const float TRIVIAL_GLOW = 0.5f;
        public const float TRIVIAL_FACTOR = 1f;
        public const float NV_EPSILON = 0.001f;

        // Float to string constants
        public const int NUMBER_OF_DIGITS = 3;
        public const MidpointRounding ROUNDING = MidpointRounding.ToEven;

        //Drawing constants
        public const float ROW_HEIGHT = 45f;
        public const float GEN_ROW_HEIGHT = 30f;
        public const float INDICATOR_SIZE = 12f;
        public const float ROW_GAP = 10f;
    }
}