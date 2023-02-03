﻿using System.Collections.Generic;
using System.IO;
using Verse;

namespace FactionLoadout
{
    [HotSwappable]
    public static class IO
    {
        public static string SaveDataPath => Path.Combine(ModCore.ModFolder, "SaveData");

        public static bool DeleteFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return true;
            }
            return false;
        }

        public static void SaveToFile(IExposable item, string filePath)
        {
            var info = new FileInfo(filePath);
            if (!info.Directory.Exists)
                info.Directory.Create();

            Scribe.saver.InitSaving(filePath, "FactionEditData");

            item.ExposeData();

            Scribe.saver.FinalizeSaving();
        }

        public static void LoadFromFile(IExposable item, string filePath)
        {
            Scribe.loader.InitLoading(filePath);

            Scribe.loader.EnterNode("FactionEditData");
            item.ExposeData();

            Scribe.loader.FinalizeLoading();
        }

        public static IEnumerable<FileInfo> ListXmlFiles(string directory)
        {
            var dir = new DirectoryInfo(directory);
            if (!dir.Exists)
                yield break;

            foreach (var file in dir.EnumerateFiles("*.xml", SearchOption.TopDirectoryOnly))
            {
                yield return file;
            }
        }
    }
}
