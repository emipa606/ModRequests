using Verse;

namespace RimWorld
{
	public class CompProperties_SpawnerResearch : CompProperties
	{
		public IntRange spawnIntervalRange = new IntRange(100, 100);

		public bool requiresPower;

		public bool writeTimeLeftToSpawn;

		public bool showMessageIfOwned;

		public string saveKeysPrefix;

		public bool inheritFaction;

		public CompProperties_SpawnerResearch()
		{
			compClass = typeof(CompSpawnerResearch);
		}
	}
}
