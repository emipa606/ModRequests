using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace ProximityFuze {
	public class ProximityFuzeSettings : ModSettings {

		public static Dictionary<string, float> radiusFactors = new Dictionary<string, float>();
		public static float defaultFactor;

		public override void ExposeData() {
			Scribe_Collections.Look(ref radiusFactors, "radiusFactor");
			Scribe_Values.Look(ref defaultFactor, "defaultFactor", 0.8f);
		}
	}
}
