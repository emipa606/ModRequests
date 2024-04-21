using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace StatueOfAnimal {
    public class StatueGraphicGetter {
        public static Graphic GetNakedBodyGraphic(BodyTypeDef bodyType, Shader shader, Color skinColor, float scale) {
            if (bodyType == null) {
                Log.Error("Getting naked body graphic with undefined body type.");
                bodyType = BodyTypeDefOf.Male;
            }
            string str = "Naked_" + bodyType.ToString();
            string path = "Things/Pawn/Humanlike/Bodies/" + str;
            return GraphicDatabase.Get<Graphic_Multi>(path, shader, Vector2.one * scale, skinColor);
        }
    }
}
