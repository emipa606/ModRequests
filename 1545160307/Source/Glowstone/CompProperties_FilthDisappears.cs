using Verse;

namespace Glowstone {

	internal class CompProperties_FilthDisappears : CompProperties {

		public int disappearsAfterRareTicks = 20;
		public ThingDef filthLeaving;
		public bool disappearsInRain = true;
		public float rainMultiplier = 5f;


		public CompProperties_FilthDisappears() {
			compClass = typeof(Comp_FilthDisappears);
		}
	}
}
