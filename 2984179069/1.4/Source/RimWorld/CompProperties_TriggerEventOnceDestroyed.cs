using Verse;

namespace RimWorld
{
	public class CompProperties_TriggerEventOnceDestroyed : CompProperties
	{
		public ThoughtDef thought;

		[MustTranslate]
		public string message;

		public CompProperties_TriggerEventOnceDestroyed()
		{
			compClass = typeof(CompTriggerEventOnceDestroyed);
		}
	}
}
