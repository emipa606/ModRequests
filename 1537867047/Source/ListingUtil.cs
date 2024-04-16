using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace AppearanceClothes {
    public static class ListingUtil {
        public static bool ButtonThing(this Listing_Standard listing,ThingDef thingDef, float width, float height,Color color,BodyTypeDef bodyType,Rot4 rot) {
            Traverse traverse = Traverse.Create(listing);
            traverse.Method("NewColumnIfNeeded", new object[] { height }).GetValue();
            //listing.NewColumnIfNeeded(height);
            float curX = traverse.Field("curX").GetValue<float>();
            float curY = traverse.Field("curY").GetValue<float>();
            Rect rect = new Rect(curX, curY, width, height);
            Texture2D tex = null;
            float scale = 1f;
            bool flipped = false;
            if (!AppearanceClothesMod.Settings.showClothForDisplay || !thingDef.TryGetApparelTexture(bodyType, out tex, rot, out flipped)) {
                tex = thingDef.uiIcon;
            } else if(thingDef.apparel.LastLayer != ApparelLayerDefOf.Overhead) {
                scale = AppearanceClothesMod.Settings.GetScaleOfCloth(bodyType);
            }
            bool result = ButtonScaledImage(rect, tex, color, scale, flipped);
            TooltipHandler.TipRegion(rect, thingDef.LabelCap);

            listing.Gap(height + listing.verticalSpacing);
            return result;
        }

        public static bool ButtonScaledImage(Rect butRect, Texture2D tex,Color color, float texScale, bool flipped) {
            if (Mouse.IsOver(butRect)) {
                GUI.color = GenUI.MouseoverColor;
            } else {
                GUI.color = color;
            }
            Rect scaledRect = new Rect(butRect);
            if (flipped) {
                scaledRect.x += scaledRect.width;
                scaledRect.width *= -1;
            }
            GUI.DrawTexture(scaledRect.ScaledBy(texScale), tex);
            GUI.color = color;
            return Widgets.ButtonInvisible(butRect, false);
        }
    }
}
