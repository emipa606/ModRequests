using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace StatueOfColonist {
    public static class ListingUtil {
        public static bool ButtonThing(this Listing_Standard listing,ThingDef thingDef, float width, float height,Color color,BodyTypeDef bodyType) {
            Traverse traverse = Traverse.Create(listing);
            traverse.Method("NewColumnIfNeeded", new object[] { height }).GetValue();
            //listing.NewColumnIfNeeded(height);
            float curX = traverse.Field("curX").GetValue<float>();
            float curY = traverse.Field("curY").GetValue<float>();
            Rect rect = new Rect(curX, curY, width, height);
            Texture2D tex = null;
            float scale = 1f;
            if (!StatueOfColonistMod.Settings.showClothForDisplay || !thingDef.TryGetApparelTexture(bodyType, out tex)) {
                tex = thingDef.uiIcon;
            } else if(thingDef.apparel.LastLayer != ApparelLayerDefOf.Overhead) {
                scale = StatueOfColonistMod.Settings.GetScaleOfCloth(bodyType);
            }
            bool result = ButtonScaledImage(rect, tex, color, scale);
            TooltipHandler.TipRegion(rect, thingDef.LabelCap);

            listing.Gap(height + listing.verticalSpacing);
            return result;
        }

        //public static bool TryGetApparelTexture(this ThingDef def, BodyTypeDef bodyType, out Texture2D tex) {
        //    tex = null;
        //    if (bodyType == null) {
        //        bodyType = BodyTypeDefOf.Male;
        //    }
        //    if (def.apparel == null || def.apparel.wornGraphicPath.NullOrEmpty()) {
        //        return false;
        //    }

        //    string path;
        //    if (def.apparel.LastLayer == ApparelLayerDefOf.Overhead) {
        //        path = def.apparel.wornGraphicPath;
        //    } else {
        //        path = def.apparel.wornGraphicPath + "_" + bodyType.defName;
        //    }

        //    tex = ContentFinder<Texture2D>.Get(path + "_south", false);
        //    if (tex == null) {
        //        Log.Message(path + " is not found.");
        //        return false;
        //    }
        //    return true;
        //}

        public static bool TryGetApparelTexture(this ThingDef def, BodyTypeDef bodyType, out Texture2D tex, string postfix = "") {
            tex = null;
            if (bodyType == null) {
                bodyType = BodyTypeDefOf.Male;
            }
            if (def.apparel == null || def.apparel.wornGraphicPath.NullOrEmpty()) {
                return false;
            }
            return HasTexture(def.apparel.wornGraphicPath, out tex, postfix) || HasTexture(def.apparel.wornGraphicPath + "_" + bodyType.defName, out tex, postfix);
        }

        public static bool HasTexture(string path, out Texture2D tex, string postfix = "") {
            tex = ContentFinder<Texture2D>.Get(path + postfix, false);
            if (tex == null) {
                Log.Message(path + " is not found.");
                return false;
            }
            return true;
        }

        public static bool ButtonScaledImage(Rect butRect, Texture2D tex,Color color, float texScale) {
            if (Mouse.IsOver(butRect)) {
                GUI.color = GenUI.MouseoverColor;
            } else {
                GUI.color = color;
            }
            Rect scaledRect = new Rect(butRect);
            GUI.DrawTexture(scaledRect.ScaledBy(texScale), tex);
            GUI.color = color;
            return Widgets.ButtonInvisible(butRect, false);
        }
    }
}
