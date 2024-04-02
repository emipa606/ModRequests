using HarmonyLib;
using Verse;
using System.Reflection;
using UnityEngine;

namespace GeneticDrift
{
    [StaticConstructorOnStartup]
    public class Main
    {
        static Main()
        {
            Harmony harmony = new Harmony("arr624.geneticdrift");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

    class GeneticDriftMod : Mod
    {
        public static GeneticDriftSettings settings;

        public GeneticDriftMod(ModContentPack content)
            : base(content)
        {
            ParseHelper.Parsers<GeneChancesList>.Register(GeneChancesList.FromString);
            settings = GetSettings<GeneticDriftSettings>();
        }

        public override string SettingsCategory()
        {
            return "Genetic Drift";
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            settings.DoWindowContents(inRect);
        }
    }
}
