namespace RimWorld
{
	[DefOf]
	public static class IncidentDefOfH
	{
		public static IncidentDef AnimalInsanityMass;

		public static IncidentDef PsychicSoothe;

		public static IncidentDef DefoliatorShipPartCrash;

		public static IncidentDef PsychicEmanatorShipPartCrash;

		public static IncidentDef ManhunterPack;

		public static IncidentDef ShortCircuit;

		public static IncidentDef CropBlight;

		public static IncidentDef ToxicFallout;

		public static IncidentDef ResourcePodCrash;

		public static IncidentDef MeteoriteImpact;

		public static IncidentDef RefugeePodCrash;

		static IncidentDefOfH()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(IncidentDefOf));
		}
	}
}
