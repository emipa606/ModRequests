using AlienRace;
using RimWorld;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace StylishRim
{
    public class StylishBodyChanger
    {
        public static string modTexDir = null;
        public static string ModTexDir
        {
            get
            {
                if (modTexDir == null)
                {
                    modTexDir = Path.Combine(LoadedModManager.RunningMods.Where(x => x.Name == "Stylish Rim").FirstOrDefault().foldersToLoadDescendingOrder.Last(), "Textures");
                }
                return modTexDir;
            }
        }
        public static List<string> customFolders = new List<string>();
        public static void SetCustomFolders()
        {
            string dir = GenFilePaths.ModsFolderPath;
            string pre;
            foreach (ModContentPack m in LoadedModManager.RunningMods)
            {
                /*
                pre = Path.Combine(dir, m.FolderName, "Textures", CUR_DIR).Replace('\\', '/');
                */
                foreach (string s in m.foldersToLoadDescendingOrder)
                {
                    if (!string.IsNullOrEmpty(s))
                    {
                        pre = Path.Combine(s, "Textures", CUR_DIR).Replace('\\', '/');
                        if (Directory.Exists(pre))
                        {
                            customFolders.Add(pre);
                        }
                    }
                }
            }
        }
        public static readonly string CUR_DIR = "StylishBodies/";
        public static readonly string FEMALE_DIR = "Female/";

        public static StylishBodyChanger changer = null;
        public static bool change = false;
        public static string maleBody = null;
        public static string femaleBody = null;
        public static string body = null;
        public static string otherDirBody = null;
        public static BodyTypeDef type = null;
        public static Color skin = default;

        //public static ShaderTypeDef shader = default;

        public static void Init(Pawn pawn)
        {
            changer = PawnStyleSet.Get(pawn)?.Changer;
            if (changer == null) return;
            change = true;
            type = changer.bodyType ?? pawn.story.bodyType;
            skin = AdjustColor(pawn.story.SkinColor, changer.changeColor);
            if (changer.isCustom)
            {
                if (changer.folderPath == null)
                {
                    change = false;
                    changer = null;
                    return;
                }
                string pattern = $"Naked_{type}";
                maleBody = changer.folderPath;
                femaleBody = changer.FemaleFolderPath;
                //shader = changer.shaderType;
                if (pawn.gender == Gender.Male)
                {
                    if (TexFileExsits(maleBody, pattern))
                    {
                        body = CUR_DIR + maleBody;
                    }
                    else
                    {
                        if (TexFileExsits(femaleBody, pattern))
                        {
                            body = CUR_DIR + femaleBody;
                        }
                        else
                        {
                            body = null;
                        }
                    }
                }
                else
                {
                    if (TexFileExsits(femaleBody, pattern))
                    {
                        body = CUR_DIR + femaleBody;
                    }
                    else
                    {
                        if (TexFileExsits(maleBody, pattern))
                        {
                            body = CUR_DIR + maleBody;
                        }
                        else
                        {
                            body = null;
                        }
                    }
                }
            }
            else
            {
                if (changer.paths == null)
                {
                    change = false;
                    changer = null;
                    return;
                }
                body = changer.paths.body.path;
                //shader = changer.shaderType ?? graphicPaths.skinShader;
            }
        }
        public static void Finish()
        {
            change = false;
            changer = null;
        }
        public static IEnumerable<string> GetCustomFolderPaths()
        {
            foreach (string s in Directory.GetDirectories(Path.Combine(ModTexDir, CUR_DIR).Replace('\\', '/')))
            {
                yield return s.Substring(s.LastIndexOf('/') + 1);
            }
            foreach (string p in customFolders)
            {
                foreach (string s in Directory.GetDirectories(p.Replace('\\', '/')))
                {
                    yield return s.Substring(s.LastIndexOf('/') + 1);
                }
            }
        }
        public static IEnumerable<BodyTypeDef> ContainBodyTypes(string path)
        {
            foreach (BodyTypeDef type in DefDatabase<BodyTypeDef>.AllDefsListForReading)
            {
                if (ContainTexture(path, $"Naked_{type}"))
                {
                    yield return type;
                }
            }
        }
        private static List<Texture2D> GetTexList(string path)
        {
            return ContentFinder<Texture2D>.GetAllInFolder(path).ToList();
        }
        private static bool ContainTexture(string path, string pattern)
        {
            List<Texture2D> files = GetTexList(CUR_DIR + path);
            return files.Any(x => x.name.Contains(pattern + "_north"));
        }
        private static bool TexFileExsits(string path, string pattern, bool onlyTop = true)
        {
            if (Directory.Exists(Path.Combine(ModTexDir, CUR_DIR) + path))
            {
                if (Directory.GetFiles(Path.Combine(ModTexDir, CUR_DIR) + path, $"{pattern}_north*", onlyTop ? SearchOption.TopDirectoryOnly : SearchOption.AllDirectories).ToList().Count > 0)
                {
                    return true;
                }
            }
            foreach (string s in customFolders)
            {
                if (Directory.Exists(s + path))
                {
                    if (Directory.GetFiles(s + path, $"{pattern}_north*", onlyTop ? SearchOption.TopDirectoryOnly : SearchOption.AllDirectories).ToList().Count > 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public static string ChangeBody(string path)
        {
            return change ? body ?? path : path;
        }
        public static BodyTypeDef ChangeBodyType(BodyTypeDef body)
        {
            return change ? type : body;
        }
        public static Color ChangeColor(Color c)
        {
            return change ? skin : c;
        }
        public static Color AdjustColor(Color c, Vector3 v3)
        {
            if (v3 == Vector3.zero) return c;
            Color.RGBToHSV(c, out float h, out float s, out float v);
            return Color.HSVToRGB((h + v3.x / 255) % 1, s + v3.y / 255, v + v3.z / 255);
        }
        public static BodyTypeDef ChangeBodyTypeByPawn(BodyTypeDef body, Pawn pawn)
        {
            return PawnStyleSet.Get(pawn)?.changer?.bodyType ?? body;
        }
        /*
        public static List<ShaderTypeDef> Shaders
        {
            get
            {
                if (shaders == null) shaders = DefDatabase<ShaderTypeDef>.AllDefsListForReading;
                return shaders;
            }
        }
        public static ShaderTypeDef ChangeShader(ShaderTypeDef s)
        {
            return change ? shader ?? s : s;
        }
        */

        public string folderPath = null;
        public string FemaleFolderPath => folderPath + FEMALE_DIR;
        public BodyTypeDef bodyType = null;
        //public ShaderTypeDef shaderType = null;
        public GraphicPaths paths;
        public bool isCustom = false;
        public Vector3 changeColor = Vector3.zero;

        public StylishBodyChanger(ThingDef_AlienRace alienDef)
        {
            paths = alienDef.alienRace.graphicPaths;
        }
        public StylishBodyChanger(string folderPath)
        {
            if (Directory.Exists(Path.Combine(ModTexDir, CUR_DIR, folderPath).Replace('/', '\\')))
            {
                isCustom = true;
                this.folderPath = folderPath.Replace('\\', '/') + '/';
                return;
            }
            foreach (string s in customFolders)
            {
                if (Directory.Exists(Path.Combine(s, folderPath).Replace('/', '\\')))
                {
                    isCustom = true;
                    this.folderPath = folderPath.Replace('\\', '/') + '/';
                    return;
                }
            }
        }
    }
}
