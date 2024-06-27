using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace IntentionalProselytism {
	public class IntentionalProselytismSettings : ModSettings {

		public static float certaintyReduceFactor = 0.2f;
		public static bool unlockVFEMIndoctrinationPod = false;
		public static bool disableInterColonistProselytizing = false;

		public override void ExposeData() {
			if(certaintyReduceFactor < 0.05f) certaintyReduceFactor = 0.05f;
			Scribe_Values.Look(ref certaintyReduceFactor, "certaintyReduceFactor", 0.2f);
			if(certaintyReduceFactor < 0.05f) certaintyReduceFactor = 0.05f;
			Scribe_Values.Look(ref unlockVFEMIndoctrinationPod, "unlockVFEMIndoctrinationPod", false);
			Scribe_Values.Look(ref disableInterColonistProselytizing, "disableInterColonistProselytizing", false);
		}
	}
}
