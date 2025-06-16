using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

namespace TTPF
{
    [StaticConstructorOnStartup]
    public static class TTPF
    {
        public static string Author => "GonDragon";
        public static string Name => Assembly.GetName().Name;
        public static string Id => Author + "." + Name;

        public static string Version => Assembly.GetName().Version.ToString();

        private static Assembly Assembly
        {
            get
            {
                return Assembly.GetAssembly(typeof(TTPF));
            }
        }

        public static readonly Harmony Harmony;

        static TTPF()
        {
            Harmony = new Harmony(Id);
            Harmony.PatchAll();

//            foreach (var customTab in TTPF_Mod.settings.customResearchTabs)
//            {
//#if DEBUG
//            TTPF.Warning(string.Format("Loading user settings for {0}", customTab.researchDefName));
//#endif
//                var researchDef = DefDatabase<ResearchProjectDef>.GetNamed(customTab.researchDefName, false);
//                if (researchDef != null)
//                {
//                    researchDef.tab = DefDatabase<ResearchTabDef>.GetNamed(customTab.researchTabDefName, false);
//                    researchDef.researchViewX = customTab.researchViewX;
//                    researchDef.researchViewY = customTab.researchViewY;
//#if DEBUG
//                    TTPF.Warning(string.Format("User settings loaded at X:{0} - Y:{1}", customTab.researchViewX, customTab.researchViewY));
//#endif
//                }
//            }

#if DEBUG
            // Why there are hidden prerequisites? Just add them to the normal prerequisites. At least for debugging.
            foreach (ResearchProjectDef researchProject in DefDatabase<ResearchProjectDef>.AllDefs)
            {
                researchProject.prerequisites?.AddRange(researchProject.hiddenPrerequisites ?? new List<ResearchProjectDef>());
            }
#endif

        }

        public static void Log(string message) => Verse.Log.Message(PrefixMessage(message));

        public static void Warning(string message) => Verse.Log.Warning(PrefixMessage(message));

        public static void Error(string message) => Verse.Log.Error(PrefixMessage(message));

        public static void ErrorOnce(string message, string key) => Verse.Log.ErrorOnce(PrefixMessage(message), key.GetHashCode());

        public static void Message(string message) => Messages.Message(message, MessageTypeDefOf.TaskCompletion, false);

        private static string PrefixMessage(string message) => $"[{Name} v{Version}] {message}";

    }
}
