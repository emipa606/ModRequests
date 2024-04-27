using Verse;

namespace RimWorld
{
	public class CompProperties_SpawnerExtraRaidsTwo : CompProperties
	{
		public IntRange spawnIntervalRange = new IntRange(100, 100);

		public int spawnMaxAdjacent = -1;

		public bool spawnForbidden;

		public bool requiresPower;

		public bool writeTimeLeftToSpawn;

		public bool showMessageIfOwned;

		public string saveKeysPrefix;

		public bool inheritFaction;

		public CompProperties_SpawnerExtraRaidsTwo()
		{
			compClass = typeof(CompSpawnerExtraRaidsTwo);
		}
	}
}
