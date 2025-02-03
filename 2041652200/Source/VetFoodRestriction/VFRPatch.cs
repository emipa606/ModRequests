using HarmonyLib;
using Verse;

namespace VetFoodRestriction
{
	[StaticConstructorOnStartup]
	public static class VFRPatch
	{
		public const string Id = "Sielfyr.VetFoodRestriction";
		public const string ModName = "Animals Don't Need Lavish Meal To Heal";
		public const string RestrictionName = "VetFoodRestriction";
		public const string Version = "1.0.0";


        static VFRPatch()
        {
            var harmony = new Harmony(Id);
            harmony.PatchAll();
            Log.Message("Initialized " + ModName + " v" + Version);
        }
    }
}
