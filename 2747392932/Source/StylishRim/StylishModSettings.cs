using Verse;

namespace StylishRim
{
	public class StylishModSettings : ModSettings
	{
		public static float ModVersion = StylishUpdater.MOD_VERSION;

		public static bool resolveUnfindedApparelBodytype = false;
		public static bool commonizationPortrait = false;
		public static bool skipCalc = false;
		public static bool includeFacialAnimation = false;

		public override void ExposeData()
		{
			base.ExposeData();
			Expose();
		}

		public void Expose()
		{
			Scribe_Values.Look<float>(ref ModVersion, "ModVersion", 0f, true);
			Scribe_Values.Look<bool>(ref StylishTextureAtlas.disableAdjustCache, nameof(StylishTextureAtlas.disableAdjustCache), false);
			Scribe_Values.Look<bool>(ref StylishTextureAtlas.ignoreHeadOnly, nameof(StylishTextureAtlas.ignoreHeadOnly), false);
			Scribe_Values.Look<bool>(ref StylishAtlasManager.disableAnimalCache, nameof(StylishAtlasManager.disableAnimalCache), false);
			Scribe_Values.Look<bool>(ref StylishAtlasManager.disableCacheColonist, nameof(StylishAtlasManager.disableCacheColonist), false);
			Scribe_Values.Look<bool>(ref StylishAtlasManager.leaveOneCache, nameof(StylishAtlasManager.leaveOneCache), false);
			Scribe_Values.Look<bool>(ref StylishAtlasManager.preCreateCache, nameof(StylishAtlasManager.preCreateCache), false);
			Scribe_Values.Look<bool>(ref StylishAtlasManager.forCorpseCache, nameof(StylishAtlasManager.forCorpseCache), false);
			Scribe_Values.Look<bool>(ref StylishAtlasManager.disableZoomCacheOff, nameof(StylishAtlasManager.disableZoomCacheOff), false);
			Scribe_Values.Look<bool>(ref resolveUnfindedApparelBodytype, nameof(resolveUnfindedApparelBodytype), false);
			Scribe_Values.Look<bool>(ref commonizationPortrait, nameof(commonizationPortrait), false);
			if (ModVersion < 1.1f)
			{
				Scribe_Collections.Look(ref PawnStyleSet._styles, "_styles", LookMode.Value, LookMode.Deep);
			}
			else
			{
				Scribe_Collections.Look(ref PawnStyleSet._styles, "styles", LookMode.Value, LookMode.Deep);
			}
		}
	}
}
