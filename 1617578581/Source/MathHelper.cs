﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Radiology
{
    public class MathHelper
    {
        public static bool IsSameSign(int a, int b)
        {
            return a > 0 && b > 0 || a < 0 && b < 0;
        }

        public static bool IsSameSign(float a, float b)
        {
            return a > 0 && b > 0 || a < 0 && b < 0;
        }

        public static bool IsBetween(int c, int a, int b)
        {
            return (a <= c && c <= b) || (b <= c && c <= a);
        }

        public static bool CheckMtthDays(float mtthDays, int ticksPassed = 1)
        {
            if (mtthDays < 0) return false;
            return ticksPassed >= Rand.Range(0, mtthDays*60000);
        }

        public static float AngleDifference(float a, float b)
        {
            return ((a - b + 180) % 360 - 180);
        }
    }
}
