using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace StatueOfColonist {
    public static class Util {

        public static string GetName(this BodyTypeDef bodyType) {
            return bodyType.defName;
        }

        public static BodyTypeDef ToBodyTypeDef(this int value) {
            BodyTypeDef[] bodyTypes = { BodyTypeDefOf.Male,BodyTypeDefOf.Female,BodyTypeDefOf.Thin,BodyTypeDefOf.Hulk,BodyTypeDefOf.Fat};
            return bodyTypes[value];
        }

        public static int ToInt(this BodyTypeDef def) {
            BodyTypeDef[] bodyTypes = { BodyTypeDefOf.Male, BodyTypeDefOf.Female, BodyTypeDefOf.Thin, BodyTypeDefOf.Hulk, BodyTypeDefOf.Fat };
            return bodyTypes.FirstIndexOf((BodyTypeDef d) => (d == def));
        }

        public static string GetName(this HeadType headType) {
            return Enum.GetName(typeof(HeadType), headType);
        }

        public static HeadType ToHeadType(this int value) {
            return (HeadType)Enum.ToObject(typeof(HeadType), value);
        }

        public static HairDef GetHairDefFromGraphicPath(string graphicPath) {
            Predicate<HairDef> pred = delegate (HairDef h) {
                return (h.texPath == graphicPath);
            };
            HairDef hair = DefDatabase<HairDef>.AllDefsListForReading.Find(pred);
            return hair;
        }

        public static HeadType GetHeadTypeFromGraphicPath(string graphicPath) {
            string headName = graphicPath.Split('/').LastOrDefault();
            HeadType headType;
            try {
                headType = (HeadType)Enum.Parse(typeof(HeadType), headName, true);
            } catch {
                headType = HeadType.Undefined;
            }
            return headType;
        }

        public static string GetHeadGraphicPath(this HeadType headType) {
            string strHeadType = Enum.GetName(typeof(HeadType), headType);
            string folder = "Things/Pawn/Humanlike/Heads/Male";
            if (strHeadType.StartsWith("Female")) {
                folder = "Things/Pawn/Humanlike/Heads/Female";
            }
            return folder + "/" + strHeadType;
        }

        public static CrownType GetCrownType(this HeadType headType) {
            string strHeadType = Enum.GetName(typeof(HeadType), headType);
            string[] splits = strHeadType.Split('_');
            CrownType crownType = CrownType.Undefined;
            if (splits.Length == 3) {
                crownType = (CrownType)Enum.Parse(typeof(CrownType), splits[1], true);
            }
            return crownType;
        }

        public static bool IsFullyTypedFloat(this string str) {
            if (str == string.Empty) {
                return false;
            }
            string[] array = str.Split(new char[]
                {
            '.'
                });
            if (array.Length > 2 || array.Length < 1) {
                return false;
            }
            if (!ContainsOnlyCharacters(array[0], "-0123456789")) {
                return false;
            }
            if (array.Length == 2 && !ContainsOnlyCharacters(array[1], "0123456789")) {
                return false;
            }
            return true;
        }

        private static bool ContainsOnlyCharacters(string str, string allowedChars) {
            for (int i = 0; i < str.Length; i++) {
                if (!allowedChars.Contains(str[i])) {
                    return false;
                }
            }
            return true;
        }

        public static bool CanWear(ThingDef apDef, BodyTypeDef bodyType) {
            string path;
            if (apDef.apparel.LastLayer == ApparelLayerDefOf.Overhead) {
                path = apDef.apparel.wornGraphicPath;
            } else {
                path = apDef.apparel.wornGraphicPath + "_" + bodyType.defName;
            }
            return ContentFinder<Texture2D>.Get(path + "_north", false) != null;
        }
    }
}
