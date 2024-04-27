namespace RimWorld
{
	[DefOf]
	public static class IncidentDefOf2
	{
		public static IncidentDef ResourcePodCrash;

		public static IncidentDef MeteoriteImpact;

		public static IncidentDef DefoliatorShipPartCrash;

		public static IncidentDef PsychicEmanatorShipPartCrash;

		public static IncidentDef SelfTame;

		public static IncidentDef ManhunterPack;

		public static IncidentDef WandererJoin;

		static IncidentDefOf2()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(IncidentDefOf));
		}
	}
}
