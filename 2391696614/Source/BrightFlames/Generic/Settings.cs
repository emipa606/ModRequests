using System;
using HugsLib.Settings;

namespace BrightFlames
{
	internal class Settings
	{
		//public static SettingHandle<int> combineLevel;

		public Settings(ModSettingsPack settings)
		{
			//combineLevel = settings.GetHandle("combineLevel", "combineLevel".Translate(), "combineLevelDesc".Translate(), 2, CombineLevelIsValid);
		}

		// Make sure that it still works when referenced settings are null!
		private static SettingHandle.ValueIsValid CombineLevelIsValid => AtLeast(() => 0);
		//private static SettingHandle.ValueIsValid MaxIncidentsPer3DaysLimitsMin { get { return AtLeast(() => 1); } }
		//private static SettingHandle.ValueIsValid GroupSizeLimitsMin { get { return Between(() => 1, () => maxGuestGroupSize?.Value ?? DefaultMaxGroupSize); } }
		//private static SettingHandle.ValueIsValid GroupSizeLimitsMax { get { return AtLeast(() => Mathf.Max(minGuestGroupSize?.Value ?? 0, disableLimits?.Value != false ? 1 : 8)); } }
		//
		//private static SettingHandle.ValueIsValid Between(Func<int> amountMin, Func<int> amountMax)
		//{
		//	return value => int.TryParse(value, out var actual) && actual >= amountMin() && actual <= amountMax();
		//}
		
		private static SettingHandle.ValueIsValid AtLeast(Func<int> amount)
		{
			return value => int.TryParse(value, out var actual) && actual >= amount();
		}
		
		//private static SettingHandle.ValueIsValid AtMost(Func<int> amount)
		//{
		//	return value => int.TryParse(value, out var actual) && actual <= amount();
		//}
	}
}
