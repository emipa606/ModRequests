using RimWorld;
using HarmonyLib;
using Verse;

namespace HDream
{
	[HarmonyPatch(typeof(Game), "FinalizeInit")]
	public static class SettingInit
    {
		public static void Postfix()
		{
			SettingMenu.UpdatePendingDef();
		}
	}
}
