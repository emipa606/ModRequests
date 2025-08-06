using RimWorld;
using Verse;

namespace MaliExtinguishRefuelables
{
	public class CompProperties_FlickableExtinguishable : CompProperties
	{
		[NoTranslate]
		public string commandTexture = "UI/Commands/DesirePower";

		[NoTranslate]
		public string commandLabelKey = "CommandDesignateTogglePowerLabel";

		[NoTranslate]
		public string commandDescKey = "CommandDesignateTogglePowerDesc";

		public CompProperties_FlickableExtinguishable()
		{
			compClass = typeof(CompFlickableExtinguishable);
		}
	}

}
