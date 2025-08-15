using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Verse;

namespace StylishRim
{
    public class StylishExposer
    {
        public readonly string DIR_PATH = GenFilePaths.SaveDataFolderPath;
        public string folder;
        public string elementName;
        private string fullPath;
        private string labelSaveAs = "Save";
        private string labelOverwrite = "Save";
        private string labelLoad = "Load";
        private string labelDelete = "Delete";
        private string labelReturn = "Back";
        private string defaultFileName = "Untitled";
        private string fileNameText = string.Empty;
        private Vector2 saveScrollPos = Vector2.zero;
        private Vector2 loadScrollPos = Vector2.zero;
        public string FullPath => fullPath;
            

        internal List<StylishFileDTO> fileList;
        public StylishExposer(string folderName, string element = "ModSettings")
        {
            Init(folderName, element);
            fullPath = Path.Combine(DIR_PATH, folder);
        }
        public StylishExposer(string folderName, string element = "ModSettings", string subFolder = null)
        {
            Init(folderName, element);
            if (!subFolder.NullOrEmpty()) SetFullPath(subFolder);
        }
        public StylishExposer(string folderName, string element = "ModSettings", List<string> subFolders = null)
        {
            Init(folderName, element);
            if (subFolders != null) SetFullPath(subFolders);
        }
        private void Init(string folderName, string element)
        {
            folder = folderName;
            elementName = element;
        }
        private void SetFullPath(string subFolder)
        {
            fullPath = Path.Combine(DIR_PATH, folder, subFolder);
        }
        private void SetFullPath(List<string> subFolders)
        {
            fullPath = Path.Combine(Path.Combine(DIR_PATH, folder), Path.Combine(subFolders.ToArray()));
        }
        public void SetSaveAsLabel(string labelBeforeTrans) => labelSaveAs = labelBeforeTrans;
        public void SetOverwriteLabel(string labelBeforeTrans) => labelOverwrite = labelBeforeTrans;
        public void SetLoadLabel(string labelBeforeTrans) => labelLoad = labelBeforeTrans;
        public void SetDeleteLabel(string labelBeforeTrans) => labelDelete = labelBeforeTrans;
        public void SetReturnLabel(string labelBeforeTrans) => labelReturn = labelBeforeTrans;
        public void SetDefaultFileName(string fileName) => defaultFileName = fileName;
        public void Save(string name, Action exposeData, Action prefix = null, Action<bool> postfix = null)
        {
            prefix?.Invoke();
            bool result;
            if (DirExsists)
            {
                Scribe.saver.InitSaving(Path.Combine(FullPath, $"{name}.xml"), elementName);
                try { exposeData(); }
                catch { result = false; }
                finally
                {
                    Scribe.saver.FinalizeSaving();
                    result = true;
                }
            }
            else
            {
                result = false;
            }
            postfix?.Invoke(result);
        }
        public void Load(string name, Action exposeData, Action prefix = null, Action<bool> postfix = null)
        {
            prefix?.Invoke();
            bool result;
            if (DirExsists)
            {
                Scribe.loader.InitLoading(Path.Combine(FullPath, $"{name}.xml"));
                try { exposeData(); }
                catch { result = false; }
                finally
                {
                    Scribe.loader.FinalizeLoading();
                    result = true;
                }
            }
            else
            {
                result = false;
            }
            postfix?.Invoke(result);
        }
        public void Delete(string name, Action prefix = null, Action<bool> postfix = null)
        {
            prefix?.Invoke();
            bool result;
            string fullPath = Path.Combine(FullPath, $"{name}.xml");
            if (!File.Exists(fullPath))
            {
                result = false;
            }
            else
            {
                try { File.Delete(fullPath); }
                catch { result = false; }
                finally { result = !File.Exists(fullPath); }
            }
            postfix?.Invoke(result);
        }
        private bool DirExsists
        {
            get
            {
                try
                {
                    if (!Directory.Exists(FullPath))
                    {
                        Directory.CreateDirectory(FullPath);
                    }
                    else
                    {
                        return true;
                    }
                    return Directory.Exists(FullPath);
                }
                catch
                {
                    return false;
                }
            }
        }
        public List<string> GetFiles(string folderPath, string extention)
        {
            return Directory.GetFiles(folderPath, extention).ToList();
        }
        public int SetFileList(bool resetFileName, string extention = "*.xml")
        {
            if (!DirExsists) return -1;
            fileList = new List<StylishFileDTO>();
            List<string> paths = Directory.GetFiles(FullPath, extention).ToList();
            foreach (string path in paths)
            {
                fileList.Add(new StylishFileDTO(new FileInfo(path)));
            }
            fileList = fileList.OrderByDescending(x => x.date).ToList();
            if (resetFileName) fileNameText = GetUntitleName();
            return fileList.Count;
        }
        public int AddFileList(bool resetFileName, string extention = "*.xml")
        {
            if (fileList.NullOrEmpty()) return SetFileList(resetFileName, extention);
            List<string> paths = Directory.GetFiles(FullPath, extention).ToList();
            foreach (string path in paths)
            {
                fileList.Add(new StylishFileDTO(new FileInfo(path)));
            }
            fileList = fileList.OrderByDescending(x => x.date).ToList();
            if (resetFileName) fileNameText = GetUntitleName();
            return fileList.Count;
        }
        private string GetUntitleName()
        {
            string name = defaultFileName;
            int i = 0;
            while (ContainsName($"{name}{++i}.xml"));
            return $"{name}{i}";
        }
        public void RunSaveMenu(Rect view, string menuLabel, Action exposeData, Action returnAction, Action savePrefix = null, Action<bool> savePostfix = null, Action deletePrefix = null, Action<bool> deletePostfix = null) =>
            Run(view, true, menuLabel, exposeData, returnAction, savePrefix, savePostfix, deletePrefix, deletePostfix);
        public void RunLoadMenu(Rect view, string menuLabel, Action exposeData, Action returnAction, Action loadPrefix = null, Action<bool> loadPostfix = null, Action deletePrefix = null, Action<bool> deletePostfix = null) =>
            Run(view, false, menuLabel, exposeData, returnAction, loadPrefix, loadPostfix, deletePrefix, deletePostfix);
        public void Run(Rect rect, bool isSave, string menuLabel, Action exposeData, Action returnAction, Action prefix = null, Action<bool> postfix = null, Action deletePrefix = null, Action<bool> deletePostfix = null)
        {
            Text.Font = GameFont.Medium;
            Rect top = rect;
            float rowHeight = 36;
            Widgets.Label(top.TopPartPixels(rowHeight), menuLabel.Translate());
            Text.Font = GameFont.Small;
            top.yMin += rowHeight;
            Rect viewRect = new Rect(2, 0, rect.width - 4, fileList.Count * rowHeight);
            Rect bottom;
            Rect temp1;
            Rect temp2;
            if (isSave)
            {
                top.SplitHorizontally(top.height - rowHeight * 2, out top, out bottom/*, 2*/);
                Widgets.DrawBoxSolidWithOutline(top, Color.clear, Color.gray);
                Widgets.BeginScrollView(top, ref saveScrollPos, viewRect);
            }
            else
            {
                top.SplitHorizontally(top.height - rowHeight, out top, out bottom/*, 2*/);
                Widgets.DrawBoxSolidWithOutline(top, Color.clear, Color.gray);
                Widgets.BeginScrollView(top, ref loadScrollPos, viewRect);
            }
            for (int i = 0; i < fileList.Count; i++)
            {
                string name = Path.GetFileNameWithoutExtension(fileList[i].fileName);
                string date = fileList[i].date.ToString("G", System.Globalization.CultureInfo.InstalledUICulture).Replace(' ', '\n');
                viewRect.SplitHorizontally(rowHeight, out Rect r, out viewRect);
                Text.Font = GameFont.Medium;
                r.SplitVertically(r.width - 192, out r, out temp2);
                Widgets.DrawLineHorizontal(r.x + 6, r.y + rowHeight, r.width - 8);
                r.SplitVertically(r.width - 64, out r, out temp1);
                Widgets.Label(r.ContractedBy(16, 2), name);
                Text.Font = GameFont.Tiny;
                Widgets.Label(temp1.ContractedBy(2, 2), date);
                Text.Font = GameFont.Small;
                Action act;
                if (isSave)
                {
                    act = delegate { Save(name, exposeData, prefix, postfix); };
                }
                else
                {
                    act = delegate { Load(name, exposeData, prefix, postfix); };
                }
                SetButtons(
                    temp2.ContractedBy(2, 4),
                    new List<Action> { delegate { act(); }, delegate { Delete(name, deletePrefix, deletePostfix); } },
                    new List<string> { isSave ? labelOverwrite : labelLoad, labelDelete },
                    new List<bool> { false, true }
                    );
            }
            Widgets.EndScrollView();
            if (isSave)
            {
                bottom.SplitHorizontally(bottom.height * 0.5f, out temp1, out bottom);
                temp1.SplitVertically(temp1.width - 96, out temp1, out temp2);
                fileNameText = Widgets.TextField(temp1.ContractedBy(2, 2), fileNameText);
                if (Widgets.ButtonText(temp2.ContractedBy(2, 2), labelSaveAs.Translate())) Save(fileNameText, exposeData, prefix, postfix);
            }
            if (Widgets.ButtonText(bottom.ContractedBy(bottom.width * 0.4f, 2), labelReturn.Translate())) returnAction();
        }
        public static void SetButtons(Rect r, List<Action> a, List<string> labels = null, List<bool> hasMenu = null)
        {
            if (hasMenu == null)
            {
                hasMenu = new List<bool>();
                for (int i = 0; i < a.Count; i++)
                {
                    hasMenu.Add(item: false);
                }
            }

            if (labels == null)
            {
                labels = new List<string>();
                for (int j = 0; j < a.Count; j++)
                {
                    labels.Add(string.Empty);
                }
            }

            int num = labels.Count();
            float num2 = r.width / num;
            for (int k = 0; k < num; k++)
            {
                if (Widgets.ButtonText(new Rect(r.x + num2 * k, r.y, num2 * 0.9f, r.height), labels[k].Translate()))
                {
                    if (hasMenu[k])
                    {
                        Find.WindowStack.Add(new FloatMenu(new List<FloatMenuOption>
                        {
                            new FloatMenuOption("Yes", a[k])
                        }));
                    }
                    else
                    {
                        a[k]();
                    }
                }
            }
        }
        public bool ContainsName(string name)
        {
            return fileList.Any(x => x.ContainName(name));
        }
        public class StylishFileDTO
        {
            public DateTime date;
            public string fileName;
            public StylishFileDTO(FileInfo fi)
            {
                fileName = fi.Name;
                date = fi.LastWriteTime;
            }
            public bool ContainName(string name)
            {
                return fileName.Contains(name);
            }
        }
    }
}
