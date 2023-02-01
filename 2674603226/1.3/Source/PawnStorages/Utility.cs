using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace PawnStorages
{
    public static class Utility
    {
        public static bool IsWall(this ThingDef def)
        {
            if (def.category != ThingCategory.Building) return false;
            if (!def.graphicData?.Linked ?? true) return false;
            return (def.graphicData.linkFlags & LinkFlags.Wall) != LinkFlags.None &&
                   def.graphicData.linkType == LinkDrawerType.CornerFiller &&
                   def.fillPercent >= 1f &&
                   def.blockWind &&
                   def.coversFloor &&
                   def.castEdgeShadows &&
                   def.holdsRoof &&
                   def.blockLight;
        }

        public static Mesh SetUVs(this Mesh mesh, bool flipped)
        {
            var array2 = new Vector2[4];
            if (!flipped)
            {
                array2[0] = new Vector2(0f, 0f);
                array2[1] = new Vector2(0f, 1f);
                array2[2] = new Vector2(1f, 1f);
                array2[3] = new Vector2(1f, 0f);
            }
            else
            {
                array2[0] = new Vector2(1f, 0f);
                array2[1] = new Vector2(1f, 1f);
                array2[2] = new Vector2(0f, 1f);
                array2[3] = new Vector2(0f, 0f);
            }

            mesh.uv = array2;
            return mesh;
        }

        public static Texture2D GetGreyscale(this RenderTexture source)
        {
            var texture = MakeReadableTextureInstance(source);
            var colors = texture.GetPixels();

            for (int i = 0; i < colors.Length; i++)
            {
                var c = colors[i];
                float gray = ((c.r * 0.3f) + (c.g * 0.59f) + (c.b * 0.11f));
                colors[i] = new Color(gray, gray, gray, c.a);
            }
            texture.SetPixels(colors);
            texture.Apply();
            return texture;
        }

        public static Texture2D MakeReadableTextureInstance(this RenderTexture source)
        {
            RenderTexture temporary = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
            temporary.name = "MakeReadableTexture_Temp";
            Graphics.Blit(source, temporary);
            RenderTexture active = RenderTexture.active;
            RenderTexture.active = temporary;
            Texture2D texture2D = new Texture2D(source.width, source.height);
            texture2D.ReadPixels(new Rect(0f, 0f, temporary.width, temporary.height), 0, 0);
            texture2D.Apply();
            RenderTexture.active = active;
            RenderTexture.ReleaseTemporary(temporary);
            return texture2D;
        }
    }
}
