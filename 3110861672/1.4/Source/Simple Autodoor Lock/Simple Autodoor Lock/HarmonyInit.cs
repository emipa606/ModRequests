using HarmonyLib;
using RimWorld;
using Verse;

namespace SimpleAutodoorLock
{
	[StaticConstructorOnStartup]
	internal static class HarmonyInit
	{

		public static void CheckCompatibleMods()
		{
		}

		static HarmonyInit()
		{
			CheckCompatibleMods();

			var harmony = new Harmony("Arkymn.SimpleAutodoorLock");
			harmony.PatchAll();
		}
	}
}