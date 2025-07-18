using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace IncidentTweaker
{
    class WidgetsExtra
    {
        public static bool FloatsEqual(float a, float b)
        {
            float diff = a - b;
            return diff >= -0.001f && diff <= 0.001f;
        }

        public static float RoundTo(float v, float round)
        {
            return Mathf.RoundToInt(v / round) * round;
        }

        public static bool HorizontalSlider(Rect rect, ref float x, SimpleCurve valueCurve)
        {
            float v = valueCurve.EvaluateInverted(x);
            float vv = Widgets.HorizontalSlider(rect, v, 0, 1, true, x.ToString());
            float newX = RoundTo(valueCurve.Evaluate(vv), 0.001f);

            if (!FloatsEqual(v, vv) && !FloatsEqual(x, newX)) {
                x = newX;
                return true;
            }

            return false;
        }
    }
}
