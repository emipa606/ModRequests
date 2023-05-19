using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using System.Text.RegularExpressions;
using System.IO;

namespace WorksController
{
    public class WorksControllerUtil
    {
        public static bool WorkGiver_ShouldSkip(WorkGiverDef workGiverDef, Pawn pawn, bool forced = false)
        {
            if (forced)
            {
                return false;
            }

            if (!WC_DataContext.m_ModEnable)
            {
                return false;
            }

            if (pawn.Dead || !pawn.Spawned || (pawn.Faction != Faction.OfPlayer && !pawn.IsQuestLodger()))
            {
                return false;
            }

            WC_DataObject data = WC_DataContext.GetData(workGiverDef.defName);
            if (data == null || !data.Active)
            {
                return false;
            }

            for (int i = 0; i < data.SkillDefs.Count; i++)
            {
                SkillRecord skill = pawn.skills?.GetSkill(data.SkillDefs[i]);
                if (skill == null || skill.TotallyDisabled)
                {
                    return false; //スキル非所持者には関与しない
                }
                if (skill.Level < data.SkillRanges[i].min || ((skill.Level <= 20 || data.SkillRanges[i].max < 20) && skill.Level > data.SkillRanges[i].max))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool WorkGiver_AllowPawn(WorkGiverDef workGiverDef, Pawn pawn)
        {
            if (!WC_DataContext.m_ModEnable)
            {
                return false;
            }

            if (pawn.Dead || !pawn.Spawned || (pawn.Faction != Faction.OfPlayer && !pawn.IsQuestLodger()))
            {
                return false;
            }

            WC_DataObject data = WC_DataContext.GetData(workGiverDef.defName);
            if (data == null || !data.Active)
            {
                return false;
            }

            for (int i = 0; i < data.SkillDefs.Count; i++)
            {
                SkillRecord skill = pawn.skills?.GetSkill(data.SkillDefs[i]);
                if (skill == null || skill.TotallyDisabled)
                {
                    return false; //スキル非所持者には関与しない
                }
                if (skill.Level < data.SkillRanges[i].min || ((skill.Level <= 20 || data.SkillRanges[i].max < 20) && skill.Level > data.SkillRanges[i].max))
                {
                    return false;
                }
            }

            return true;
        }


        private const string CONFIG_DIR_NAME = "WorksController";

        public readonly static string CONFIG_NAME_PATTERN = String.Format("*.{0}", WC_SaveFile.FILE_EXTENSION);

        //public static string GetConfigDirectory() => Path.GetFullPath(String.Format("{0}/{1}", GenFilePaths.SaveDataFolderPath, CONFIG_DIR_NAME));
        public static string GetConfigDirectory() => Path.GetFullPath(Path.Combine(GenFilePaths.SaveDataFolderPath, CONFIG_DIR_NAME));


        public static void CheckAndCreateConfigDirectory()
        {
            string WCConfigPath = GetConfigDirectory();
            bool folderExist = Directory.Exists(WCConfigPath);
            if (!folderExist)
            {
                try
                {
                    Directory.CreateDirectory(WCConfigPath);
                    folderExist = Directory.Exists(WCConfigPath);
                }
                catch
                {
                    Log.Error("Works Controller error reports: I can't create Settings directory.");
                }
            }
        }

        public static IEnumerable<string> GetSaveFileNames()
        {
            foreach (string file in Directory.GetFiles(GetConfigDirectory(), CONFIG_NAME_PATTERN))
            {
                yield return file;
            }
            yield break;
        }

        public static IEnumerable<WC_SaveFile> GetSaveFiles()
        {
            foreach (string file in GetSaveFileNames())
            {
                AcceptanceReport report = WC_SaveFile.TryRead(file, out WC_SaveFile saveFile);
                if (report.Accepted)
                {
                    yield return saveFile;
                }
            }
            yield break;
        }
    }
}
