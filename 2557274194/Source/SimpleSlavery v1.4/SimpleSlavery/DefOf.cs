using RimWorld;
using Verse;

namespace SimpleSlaveryCollars
{
	[DefOf]
	public static class SS_HediffDefOf
	{
		static SS_HediffDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(HediffDefOf));
		}
		public static HediffDef Crypto_Stasis;
		public static HediffDef Electrocuted;
		public static HediffDef Enslaved;
	}
	[DefOf]
	public static class SS_JobDefOf
	{
		static SS_JobDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(JobDefOf));
		}
		public static JobDef SetSlaveCollar;
		public static JobDef ShackleSlave;
	}
	[DefOf]
	public static class SS_MentalStateDefOf
	{
		static SS_MentalStateDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(MentalStateDefOf));
		}
		public static MentalStateDef CryptoStasis;
	}
	[DefOf]
	public static class SS_ThoughtDefOf
	{
		static SS_ThoughtDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(ThoughtDefOf));
		}
		public static ThoughtDef SlaveCollar;
		public static ThoughtDef ExplosiveCollar;
		public static ThoughtDef WasEnslaved_Assimilation;
	}
	[DefOf]
	public static class SS_RecordDefOf
	{
		static SS_RecordDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(RecordDefOf));
		}
		public static RecordDef TimeAsSlave;
	}
}