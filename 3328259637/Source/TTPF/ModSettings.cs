using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace TTPF
{
    public class Settings : ModSettings
    {
        public List<CustomResearchData> customResearchTabs = new();

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref customResearchTabs, "customResearchTabs", LookMode.Deep);
        }

        public void AddCustomResearchData(string researchDefName, string researchTabDefName, float researchViewX, float researchViewY)
        {
            foreach (var customResearchData in customResearchTabs)
            {
                if (customResearchData.researchDefName == researchDefName)
                {
                    customResearchData.researchTabDefName = researchTabDefName;
                    customResearchData.researchViewX = researchViewX;
                    customResearchData.researchViewY = researchViewY;
                    return;
                }
            }
            customResearchTabs.Add(new CustomResearchData(researchDefName, researchTabDefName, researchViewX, researchViewY));
        }
    }

    public class TTPF_Mod : Mod
    {
        public static Settings settings;

        public TTPF_Mod(ModContentPack content) : base(content)
        {
            settings = GetSettings<Settings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Rect buttonRect = new Rect(inRect.width / 2 - 100, inRect.height / 2 - 30, 200, 60);

            if (Widgets.ButtonText(buttonRect, "Restore Default Tech Tree"))
            {
                // Show confirmation dialog
                Find.WindowStack.Add(new Dialog_MessageBox(
                    "The game needs to be restarted to apply the changes. Please save your game and restart the game manually.",
                    "OK", () => ClearUserChanges(), // OK button
                    "Cancel" // Cancel button
                ));
            }
        }

        private void ClearUserChanges()
        {
            settings.customResearchTabs.Clear();
            settings.Write();
        }

        public override string SettingsCategory()
        {
            return "Tech Tree Patch Framework";
        }
    }

    public class CustomResearchData : IExposable
    {
        public string researchDefName;
        public float researchViewX;
        public float researchViewY;
        public string researchTabDefName;

        public CustomResearchData() { }

        public CustomResearchData(string researchDefName, string researchTabDefName, float researchViewX, float researchViewY)
        {
            this.researchDefName = researchDefName;
            this.researchTabDefName = researchTabDefName;
            this.researchViewX = researchViewX;
            this.researchViewY = researchViewY;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref researchDefName, "researchDefName");
            Scribe_Values.Look(ref researchTabDefName, "researchTabDefName");
            Scribe_Values.Look(ref researchViewX, "researchViewX");
            Scribe_Values.Look(ref researchViewY, "researchViewY");
        }
    }

    public class ResearchTabSaver : IExposable
    {
        public List<CustomResearchData> customResearchTabs = new List<CustomResearchData>();

        public void ExposeData()
        {
            Scribe_Collections.Look(ref customResearchTabs, "customResearchTabs", LookMode.Deep);
        }
    }
}
