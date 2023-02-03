﻿using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace FactionLoadout
{
    public static class StyleHelper
    {
        public static IEnumerable<(ThingStyleDef style, string name, Texture2D exampleIcon)> GetValidStyles(ThingDef def)
        {
            if (def?.apparel == null)
                yield break;

            foreach(var item in DefDatabase<StyleCategoryDef>.AllDefsListForReading)
            {
                if (item.thingDefStyles == null)
                    continue;

                foreach(var pair in item.thingDefStyles)
                {
                    if (pair.ThingDef == def)
                        yield return (pair.StyleDef, item.LabelCap, Widgets.GetIconFor(pair.ThingDef, null, pair.StyleDef));
                }
            }
        }
    }
}
