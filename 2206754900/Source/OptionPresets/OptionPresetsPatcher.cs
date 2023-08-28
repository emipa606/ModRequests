using HarmonyLib;
using Verse;

namespace NMeijer.OptionPresets
{
	[StaticConstructorOnStartup]
	public static class OptionPresetsPatcher
	{
		static OptionPresetsPatcher()
		{
			Harmony harmony = new Harmony("nl.nmeijer.optionpresets");
			harmony.PatchAll();
		}
	}
}