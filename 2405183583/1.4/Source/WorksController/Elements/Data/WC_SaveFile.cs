using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace WorksController
{
    public class WC_SaveFile : IComparable<WC_SaveFile>
    {
        public const string FILE_EXTENSION = "svcnf";
        private const string DATE_TIME_DATA_FORMAT = "F";
        private const string DATE_TIME_DISPLAY_FORMAT = "yyyy-MM-dd HH:mm";
        

        private int m_Version;
        private string m_FileName;
        private DateTime m_UpdateDT;

        public int Version => m_Version;
        public DateTime UpdateDT => m_UpdateDT;
        public string DisplayDT => UpdateDT.ToString(DATE_TIME_DISPLAY_FORMAT);
        public string Label => m_FileName;
        public string NameWithExtension => String.Format("{0}.{1}", m_FileName, FILE_EXTENSION);
        public string FullPath => Path.Combine(WorksControllerUtil.GetConfigDirectory(), NameWithExtension);
        public bool Exist => File.Exists(FullPath);
        
        public WC_SaveFile(string fileName, DateTime updateDt, int version = WC_DataContext.VERSION_LATEST)
        {
            m_Version = version;
            m_FileName = fileName;
            m_UpdateDT = updateDt;
        }

        public static AcceptanceReport TryRead(string path, out WC_SaveFile saveFile)
        {
            saveFile = null;
            if (!File.Exists(path))
            {
                return false;
            }
            try
            {
                using (StreamReader sr = new StreamReader(path, Encoding.Default))
                {
                    return TryParse(sr.ReadLine(), out saveFile);
                }
            }
            catch
            {
                return false;
            }
        }

        private static AcceptanceReport TryParse(string data, out WC_SaveFile saveFile)
        {
            saveFile = null;
            string[] splitData = data.Split('^');
            string name = splitData[0];
            DateTime updateDT = DateTime.Parse(splitData[1]);
            string[] splitData2 = splitData[2].Split('|');
            int version = int.Parse(splitData2[0]);
            saveFile = new WC_SaveFile(name, updateDT, version);
            return true;
        }

        public AcceptanceReport TryRestore()
        {
            string path = FullPath;
            if (!File.Exists(path))
            {
                return String.Format("{0}{1}:{2}", "LoadGameButton".Translate(), "WC_Failed".Translate(), "WC_FileNotExists".Translate()); //ロード失敗:対象ファイルが存在しません。
            }
            try
            {
                using (StreamReader sr = new StreamReader(path, Encoding.Default))
                {
                    string data = sr.ReadLine();
                    WC_DataContext.RestoreBySaveData(data);
                    return true;
                }
            }
            catch
            {
                return String.Format("{0}{1}:{2}", "LoadGameButton".Translate(), "WC_Failed".Translate(), "WC_FileFormatInvalid".Translate()); //ロード失敗:対象ファイルが正しい形式ではありません。
            }
        }

        public AcceptanceReport TryWrite()
        {
            // saveName|TimeStamp|rawString
            string fixString = String.Format("{0}^{1}^{2}", Label, DateTime.Now.ToString(DATE_TIME_DATA_FORMAT), WC_DataContext.GetSavedataString());
            try
            {
                using (StreamWriter sw = new StreamWriter(FullPath, false, Encoding.Default))
                {
                    sw.Write(fixString);
                }

                //List<char> damemojiList = new List<char>();
                //damemojiList.Add('^');
                //damemojiList.AddRange(Path.GetInvalidFileNameChars());
            }
            catch
            {
                return String.Format("{0}{1}:{2}", "SaveGameButton".Translate(), "WC_Failed".Translate(), "WC_FileSaveInvalid".Translate()); //セーブ失敗:正常にファイルを保存できませんでした。
            }
            return true;
        }

        public AcceptanceReport TryDelete()
        {
            string path = FullPath;
            if (!File.Exists(path))
            {
                return String.Format("{0}{1}:{2}", "WC_FileDelete".Translate(), "WC_Failed".Translate(), "WC_FileNotExists".Translate()); //削除失敗:対象ファイルが存在しません。
            }
            try
            {
                File.Delete(FullPath);
                return true;
            }
            catch
            {
                return String.Format("{0}{1}:{2}", "WC_FileDelete".Translate(), "WC_Failed".Translate(), "WC_FileDeleteInvalid".Translate()); //削除失敗:正常にファイルを削除できませんでした。;
            }
        }

        public int CompareTo(WC_SaveFile other)
        {
            return -this.UpdateDT.CompareTo(other.UpdateDT);
        }
    }
}
