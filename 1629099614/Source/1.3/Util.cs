using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace StatueOfAnimal {
    public enum BaseGraphicType {
        None,
        Circle,
        Rect
    }

    public static class Util {
        public static Texture2D Grayscale(Texture2D texSrc, float grayscaleWeightR = 0.299f, float grayscaleWeightG = 0.587f, float grayscaleWeightB = 0.114f) {
            int width = texSrc.width;
            int height = texSrc.height;
            Texture2D texCopy = CopyTexture(texSrc, width, height);

            Color[] inputColors = texCopy.GetPixels();
            Color[] outputColors = new Color[width * height];
            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    Color color = inputColors[(width * y) + x];
                    float gray = color.r * grayscaleWeightR + color.g * grayscaleWeightG + color.b * grayscaleWeightB;
                    outputColors[(width * y) + x] = new Color(gray, gray, gray, color.a);
                }
            }

            Texture2D texDest = new Texture2D(width, height);
            texDest.SetPixels(outputColors);
            texDest.Apply();

            return texDest;
        }

        private static Texture2D CopyTexture(Texture2D src, int width, int height) {
            RenderTexture tmp = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);

            Graphics.Blit(src, tmp);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = tmp;
            Texture2D tex = new Texture2D(width, height);
            tex.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
            tex.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(tmp);

            return tex;
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

        public static GraphicData GetGraphicData(this PawnKindLifeStage curKindLifeStage,Gender gender) {
            if (gender != Gender.Female || curKindLifeStage.femaleGraphicData == null) {
                return curKindLifeStage.bodyGraphicData;
            }
            return curKindLifeStage.femaleGraphicData;
        }
    }
}
