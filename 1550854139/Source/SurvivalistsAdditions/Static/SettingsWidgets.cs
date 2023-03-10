using System;

using UnityEngine;
using Verse;

namespace SurvivalistsAdditions {

  public static class SettingsWidgets {


    public static void IntSlider(Rect rect, ref int value, int minValue, int maxValue, int roundTo = 1) {
      value = RoundToAsInt(roundTo, Widgets.HorizontalSlider(
          new Rect(rect.xMin + rect.height + 10f, rect.y, rect.width - ((rect.height * 2) + 20f), rect.height),
          value, minValue, maxValue, true));
    }


    public static void FloatSlider(Rect rect, ref float value, float minValue, float maxValue, bool rounded = false, int roundTo = 1, float floatRound = -1f) {
      value = (rounded) ?
        RoundToAsInt(roundTo, Widgets.HorizontalSlider(
          new Rect(rect.xMin + rect.height + 10f, rect.y, rect.width - ((rect.height * 2) + 20f), rect.height),
          value, minValue, maxValue, true, roundTo: floatRound))
      :
        Widgets.HorizontalSlider(
          new Rect(rect.xMin + rect.height + 10f, rect.y, rect.width - ((rect.height * 2) + 20f), rect.height),
          value, minValue, maxValue, true, roundTo: floatRound)
      ;
    }


    public static void IntSliderWithButtons(Rect rect, ref int value, int minValue, int maxValue, int increment = 1, int roundTo = 1) {
      if (Widgets.ButtonText(new Rect(rect.xMin, rect.y, rect.height, rect.height), "-", true, false, true)) {
        if (value >= minValue + increment) {
          value -= increment;
        }
        else {
          value = minValue;
        }
      }

      IntSlider(rect, ref value, minValue, maxValue, roundTo);

      if (Widgets.ButtonText(new Rect(rect.xMax - rect.height, rect.y, rect.height, rect.height), "+", true, false, true)) {
        if (value <= maxValue - increment) {
          value += increment;
        }
        else {
          value = maxValue;
        }
      }
    }


    public static void FloatSliderWithButtons(Rect rect, ref float value, float minValue, float maxValue, float increment, bool rounded = false, int intRound = 1, float floatRound = -1f) {
      if (Widgets.ButtonText(new Rect(rect.xMin, rect.y, rect.height, rect.height), "-", true, false, true)) {
        if (value >= minValue + increment) {
          value -= increment;
        }
        else {
          value = minValue;
        }
      }

      FloatSlider(rect, ref value, minValue, maxValue, rounded, intRound, floatRound);
      
      if (Widgets.ButtonText(new Rect(rect.xMax - rect.height, rect.y, rect.height, rect.height), "+", true, false, true)) {
        if (value <= maxValue - increment) {
          value += increment;
        }
        else {
          value = maxValue;
        }
      }
    }


    private static int RoundToAsInt(int factor, float num) {
      return (int)(Math.Round(num / (double)factor, 0) * factor);
    }
  }
}
