using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace AppearanceClothes {
    [StaticConstructorOnStartup]
    public static class Util {
        public static readonly List<string> bodyTypes = new List<string>() { "Male", "Female", "Thin", "Hulk", "Fat" };

        public static string GetName(this BodyTypeDef bodyType) {
            return bodyType.defName;
        }

        public static BodyTypeDef ToBodyTypeDef(this int value) {
            return DefDatabase<BodyTypeDef>.GetNamed(bodyTypes[value]);
        }

        public static BodyTypeDef GetAppearanceBodyTypeDef(this Pawn pawn) {
            if (pawn.story == null) {
                return BodyTypeDefOf.Male;
            }
            BodyTypeDef bodyType = pawn.story.bodyType;
            CompAppearanceClothes compAppearanceClothes = pawn.GetComp<CompAppearanceClothes>();
            if (compAppearanceClothes != null && compAppearanceClothes.ShowAppearanceClothes && pawn.EnableAppearanceBody()) {
                bodyType = compAppearanceClothes.appearanceBodyTypeDef;
            }
            return bodyType;
        }

        public static void Reorder<Type>(this List<Type> list, Type elem, int offset) {
            int num = list.IndexOf(elem);
            num += offset;
            if (num >= 0) {
                list.Remove(elem);
                list.Insert(num, elem);
            }
        }

        public static bool IsApparelAC(this ThingDef def) {
            return def.IsApparel && def.thingClass != null && (def.thingClass == typeof(Apparel) || def.thingClass.IsSubclassOf(typeof(Apparel)));
        }

        public static bool IsAvailableForBody(this ThingDef def,BodyTypeDef bodyType) {
            Texture2D tex = null;
            bool flipped = false;
            return def.TryGetApparelTexture(bodyType, out tex, Rot4.South, out flipped);
        }

        public static bool TryGetApparelTexture(this ThingDef def, BodyTypeDef bodyType, out Texture2D tex, Rot4 rot, out bool flipped) {
            tex = null;
            flipped = false;
            if (def.apparel == null) {
                return false;
            }
            if (def.apparel.wornGraphicPath.NullOrEmpty() && !def.apparel.wornGraphicPaths.NullOrEmpty()) {
                foreach (string wornGraphicPath in def.apparel.wornGraphicPaths) {
                    if (HasApparelTexture(wornGraphicPath, bodyType, rot)) {
                        return true;
                    }
                }
                return false;
            }
            return HasApparelTexture(def.apparel.wornGraphicPath, bodyType,rot);
        }

        private static bool HasApparelTexture(string wornGraphicPath, BodyTypeDef bodyType, Rot4 rot) {
            Texture2D tex = null;
            bool flipped = false;
            if (wornGraphicPath.NullOrEmpty()) {
                return false;
            }
            return HasTexture(wornGraphicPath, out tex, rot, out flipped) || HasTexture(wornGraphicPath + "_" + bodyType.ToString(), out tex, rot, out flipped);
        }

        private static bool HasTexture(string path, out Texture2D tex, Rot4 rot, out bool flipped) {
            flipped = false;

            string postfix = "";
            if (rot == Rot4.North) {
                postfix = "_north";
            } else if (rot == Rot4.East) {
                postfix = "_east";
            } else if (rot == Rot4.South) {
                postfix = "_south";
            } else if (rot == Rot4.West) {
                postfix = "_west";
            }
            tex = ContentFinder<Texture2D>.Get(path + postfix, false);
            if (tex == null) {
                if (rot == Rot4.East) {
                    tex = ContentFinder<Texture2D>.Get(path + "_west", false);
                    flipped = true;
                } else if (rot == Rot4.West) {
                    tex = ContentFinder<Texture2D>.Get(path + "_east", false);
                    flipped = true;
                }
            }
            return tex != null;
        }

        public static bool EnableAppearanceBody(this Pawn pawn) {
            return pawn.def.defName == "Human";
        }

        public static List<ThingStyleDef> GetThingStyles(ThingDef thingDef) {
            List<ThingStyleDef> result = new List<ThingStyleDef>();
            foreach (StyleCategoryDef styleCategoryDef in DefDatabase<StyleCategoryDef>.AllDefsListForReading) {
                foreach (ThingDefStyle thingDefStyle in styleCategoryDef.thingDefStyles) {
                    if (thingDefStyle.ThingDef == thingDef) {
                        result.Add(thingDefStyle.StyleDef);
                    }
                }
            }
            return result;
        }
    }
}
