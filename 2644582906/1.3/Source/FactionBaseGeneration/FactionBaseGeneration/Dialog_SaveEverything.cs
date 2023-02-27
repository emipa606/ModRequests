using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using RimWorld;
using Verse;

namespace FactionBaseGeneration
{
    public class Dialog_SaveEverything : Dialog_Rename
    {
        public Dialog_SaveEverything(string name)
        {
            this.name = name;
        }

        protected override void SetName(string name)
        {
            this.name = name;
            Map map = Find.CurrentMap;
            var curModName = LoadedModManager.RunningMods.Where(x => x.assemblies.loadedAssemblies.Contains(Assembly.GetExecutingAssembly())).FirstOrDefault().Name;
            ModMetaData modMetaData = ModLister.AllInstalledMods.FirstOrDefault((ModMetaData x) => x != null && x.Name != null && x.Active && x.Name == curModName);

            string path = Path.GetFullPath(modMetaData.RootDir.ToString() + "/Presets/" + this.name + ".xml");
            BlueprintUtility.SaveEverything(path, map, "Blueprint");
        }

        private string name;
    }
}

