using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace TerrainOverhaul
{
    public static class LoadedTextureHelper
    {
        public static List<(Texture2D texture, string modName)> GetTexturesInLoadOrder(string itemPath, ModContentPack selfMod, string alt)
        {
            var list = new List<(Texture2D, string)>();

            List<ModContentPack> runningModsListForReading = LoadedModManager.RunningModsListForReading;
            for (int i = runningModsListForReading.Count - 1; i >= 0; i--)
            {
                var mod = runningModsListForReading[i];
                var fromMod = mod.GetContentHolder<Texture2D>().Get(itemPath);
                if (fromMod != null)
                {
                    list.Add((fromMod, mod.Name));
                }

                if (selfMod == mod && fromMod == null)
                {
                    var fromMod2 = mod.GetContentHolder<Texture2D>().Get(alt);
                    if (fromMod2 != null)
                    {
                        list.Add((fromMod2, mod.Name));
                    }
                }
            }

            var vanilla = Resources.Load<Texture2D>(GenFilePaths.ContentPath<Texture2D>() + itemPath);
            if (vanilla != null)
                list.Add((vanilla, "Vanilla"));

            list.Reverse();

            return list;
        }
    }
}
