using HarmonyLib;
using RimWorld;
using System.Linq;
using Verse;

namespace BionicleKanohiMasksOfPower
{
    [StaticConstructorOnStartup]
	public static class Core
    {
        public static Harmony harmony;
		static Core()
        {
            harmony = new Harmony("Core.Mod");
            harmony.PatchAll();
        }

		public static bool Wears(this Pawn pawn, ThingDef thingDef, out Apparel apparel)
        {
            apparel = pawn.apparel?.WornApparel?.FirstOrDefault(x => x.def == thingDef);
            return apparel != null;
        }
    }
}