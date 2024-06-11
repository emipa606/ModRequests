using System;
using System.Text.RegularExpressions;
using UnityEngine;
using Verse;

namespace Reload
{
    [StaticConstructorOnStartup]
    public static class CustomWidgets
    {
        static readonly Texture2D _defaultFillTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.2f, 0.8f, 0.85f));

        public static void TextFieldInt(Rect rect, ref int val, int min = 0, int max = int.MaxValue)
        {
            val = Mathf.Clamp(ConvertToInt(Widgets.TextField(rect, val.ToString())), min, max);
        }
        static int ConvertToInt(string str)
        {
            if (str == null)
                return 0;

            str = Regex.Replace(str, @"\D", "");
            if (str == string.Empty)
                return 0;
            return Convert.ToInt32(str);
        }
    }
}