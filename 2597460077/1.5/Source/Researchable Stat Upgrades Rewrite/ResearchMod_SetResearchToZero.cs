using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using Verse;

namespace ResearchableStatUpgrades
{
    public class ResearchMod_SetResearchToZero : ResearchMod
    {
        public override void Apply()
        {
            Dictionary<ResearchProjectDef, float> dictionary = (Dictionary<ResearchProjectDef, float>)typeof(ResearchManager).GetField("progress", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Find.ResearchManager);
            if (this.def == null)
            {
                this.def = Find.ResearchManager.GetProject();
            }
            if (XmlExtensions.SettingsManager.GetSetting("SSResearchableStatUpgrades","repeatables_are_repeatable") == "True") dictionary[this.def] = 0f;
        }

        public ResearchProjectDef def;
    }
}