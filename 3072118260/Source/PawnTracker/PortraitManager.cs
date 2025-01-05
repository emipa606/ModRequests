using System.IO;
using UnityEngine;
using RimWorld;
using Verse;

namespace PawnTrackerMain
{
    public class PortraitManager
    {
        public static void SavePortrait(Pawn pawn)
        {
            string filePath = SaveFilePath(pawn.Name.ToString());
            if (!Directory.Exists(Path.GetDirectoryName(filePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            }

            RenderTexture portraitRT = PortraitsCache.Get(pawn, new Vector2(100f, 100f), Rot4.South);
            
            RenderTexture activeRenderTexture = RenderTexture.active;
            RenderTexture.active = portraitRT;

            Texture2D tex = new Texture2D(portraitRT.width, portraitRT.height, TextureFormat.ARGB32, false);

            // Clear the texture to full transparency
            Color[] clearPixels = new Color[tex.width * tex.height];
            for (int i = 0; i < clearPixels.Length; i++)
            {
                clearPixels[i] = new Color(0, 0, 0, 0);
            }
            tex.SetPixels(clearPixels);

            tex.ReadPixels(new Rect(0, 0, portraitRT.width, portraitRT.height), 0, 0);
            tex.Apply();

            RenderTexture.active = activeRenderTexture;

            byte[] pngData = tex.EncodeToPNG();
            if (pngData != null)
            {
                File.WriteAllBytes(filePath, pngData);
            }
            else
            {
                Log.Error("Couldn't save");
            }
        }

        public static string SaveFilePath(string pawnName)
        {
            string filename = "";
            if (Faction.OfPlayer.HasName)
                filename = $"{Faction.OfPlayer.Name}_{pawnName}.png";
            else
                filename = $"{pawnName}.png";
            return Path.Combine(PawnTrackerMainMod.RootDirectory, "SavedPortraits", filename);            
        }

        public static Texture2D GetSavedPortraitFromPawnName(string pawnName)
        {
            string filePath = SaveFilePath(pawnName);
            if (File.Exists(filePath))
            {
                byte[] fileData = File.ReadAllBytes(filePath);
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(fileData); // Automatically resizes the texture dimensions.
                return texture;
            }
            else
            {
                return null;
            }
        }

        public static Texture2D GetSavedPortrait(string filePath)
        {
            if (File.Exists(filePath))
            {
                byte[] fileData = File.ReadAllBytes(filePath);
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(fileData); // Automatically resizes the texture dimensions.
                return texture;
            }
            else
            {
                return null;
            }
        }
    }
}
