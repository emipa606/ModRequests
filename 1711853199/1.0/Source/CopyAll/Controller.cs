//using CommunityCoreLibrary;
using RimWorld;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Verse;

namespace Copyprint
{
    public static class Controller
    {
        #region Private Fields

        private static readonly string _copyprintSaveExtension = ".xml";
        private static List<Copyprint> _copyprints;
        private static string _copyprintSaveLocation;
        private static List<Designator> _designators;
        private static bool _initialized = false;

        #endregion Private Fields

        #region Public Properties

        public static string CopyprintSaveLocation
        {
            get
            {
                if (_copyprintSaveLocation == null)
                    _copyprintSaveLocation = GetSaveLocation();
                return _copyprintSaveLocation;
            }
        }

        #endregion Public Properties

        #region Public Methods

        public static void Add(Copyprint copyprint, bool init = false)
        {
            if (!init && !_initialized)
                Initialize();
            _copyprints.Add(copyprint);
            var designator = new Designator_Copyprint(copyprint);
            _designators.Add(designator);

            // select the new designator
            Find.DesignatorManager.Select(designator);
        }

        public static void Remove(Designator_Copyprint designator, bool removeFromDisk)
        {
            _copyprints.Remove(designator.Copyprint);
            _designators.Remove(designator);

            if (removeFromDisk)
                DeleteXML(designator.Copyprint);
        }

        public static Copyprint FindCopyprint(string name)
        {
            if (!_initialized)
                Initialize();
            return _copyprints.FirstOrDefault(copyprint => copyprint.name == name);
        }

        public static Designator_Copyprint FindDesignator(string name)
        {
            if (!_initialized)
                Initialize();
            return _designators.FirstOrDefault(designator => (designator as Designator_Copyprint)?.Copyprint.name == name) as Designator_Copyprint;
        }

        // TODO: Call from bootstrap
        public static void Initialize()
        {
            if (_initialized)
                return;
            
            _copyprints = new List<Copyprint>();

            // find our designation category.
            DesignationCategoryDef desCatDef = DefDatabase<DesignationCategoryDef>.GetNamed("Copyprints");
            if (desCatDef == null)
                throw new Exception("Copyprints designation category not found");

            // create internal designators list as a reference to list in the category def.
            FieldInfo _designatorsFI = typeof(DesignationCategoryDef).GetField("resolvedDesignators", BindingFlags.NonPublic | BindingFlags.Instance);
            _designators = _designatorsFI.GetValue(desCatDef) as List<Designator>;

            // done!
            _initialized = true;
        }

        #endregion Public Methods

        #region Private Methods

        private static void DeleteXML(Copyprint copyprint)
        {
            File.Delete(FullFilePath(copyprint.name));
        }

        internal static List<FileInfo> GetSavedFilesList()
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(CopyprintSaveLocation);

            IOrderedEnumerable<FileInfo> files = from f in directoryInfo.GetFiles()
                                                 where f.Extension == _copyprintSaveExtension
                                                 orderby f.LastWriteTime descending
                                                 select f;

            return files.ToList();
        }

        private static string GetSaveLocation()
        {
            // Get method "FolderUnderSaveData" from GenFilePaths, which is private (NonPublic) and static.
            MethodInfo Folder = typeof(GenFilePaths).GetMethod("FolderUnderSaveData",
                                                                 BindingFlags.NonPublic |
                                                                 BindingFlags.Static);
            if (Folder == null)
                throw new Exception("copyprints :: FolderUnderSaveData is null [reflection]");

            // Call "FolderUnderSaveData" from null parameter, since this is a static method.
            return (string)Folder.Invoke(null, new object[] { "Copyprints" });
        }

        private static string FullFilePath(string name)
        {
#if DEBUG
            Log.Message( Path.Combine( _copyprintSaveLocation, name + _copyprintSaveExtension ) );
#endif
            return Path.Combine(CopyprintSaveLocation, name + _copyprintSaveExtension);
        }

        internal static bool FileExists(string name)
        {
            return File.Exists(FullFilePath(name));
        }

        internal static bool TryRenameFile(Copyprint copyprint, string newName)
        {
            if (!FileExists(newName))
            {
                RenameFile(copyprint, newName);
                return true;
            }
            return false;
        }

        private static void RenameFile(Copyprint copyprint, string newName)
        {
            DeleteXML(copyprint);
            copyprint.name = newName;
            SaveToXML(copyprint);
        }

        internal static Copyprint LoadFromXML(string name)
        {
            // set up empty copyprint
            Copyprint copyprint = new Copyprint();

#if DEBUG
            Log.Message( "Attempting to load from: " + name );
#endif

            // load stuff
            try
            {
                Scribe.loader.InitLoading(CopyprintSaveLocation + "/" + name);
                ScribeMetaHeaderUtility.LoadGameDataHeader(ScribeMetaHeaderUtility.ScribeHeaderMode.Map, true);
                Scribe.EnterNode("Copyprint");
                copyprint.ExposeData();
                Scribe.ExitNode();
            }
            catch (Exception e)
            {
                Log.Error("Exception while loading copyprint: " + e);
            }
            finally
            {
                // done loading
                Scribe.loader.FinalizeLoading();
                Scribe.mode = LoadSaveMode.Inactive;
            }

            // if a def used in the copyprint doesn't exist, exposeData will throw an error,
            // which is fine. in addition, it'll set the field to null - which may result in problems down the road.
            // Make sure each item in the copyprint has a def set, if not - remove it.
            // This check itself will throw another error, which is also fine. User will have to resolve the issue manually.
            copyprint.contents = copyprint.contents.Where(item => item.BuildableDef != null).ToList();

            // return copyprint.
            return copyprint;
        }

        public static void SaveToXML(Copyprint copyprint)
        {
            try
            {
                try
                {
                    Scribe.saver.InitSaving(FullFilePath(copyprint.name), "Copyprint");
                }
                catch (Exception ex)
                {
                    GenUI.ErrorDialog("ProblemSavingFile".Translate(ex.ToString()));
                    throw;
                }
                ScribeMetaHeaderUtility.WriteMetaHeader();
                Scribe_Deep.Look(ref copyprint, "Copyprint");
            }
            catch (Exception ex2)
            {
                Log.Error("Exception while saving copyprint: " + ex2);
            }
            finally
            {
                Scribe.saver.FinalizeSaving();
            }
            // set exported flag.
            copyprint.exported = true;
        }

        #endregion Private Methods
    }
}
