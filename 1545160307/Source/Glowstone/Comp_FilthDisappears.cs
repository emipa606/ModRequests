using RimWorld;
using Verse;

namespace Glowstone {

	internal class Comp_FilthDisappears : ThingComp {

		private int ticksToDisappear = int.MinValue;

		public CompProperties_FilthDisappears Props {
			get {
				return (CompProperties_FilthDisappears)props;
			}
		}

		public bool CompShouldRemove {
			get {
				return ticksToDisappear <= 0;
			}
		}


		public override void PostExposeData() {
			base.PostExposeData();
			Scribe_Values.Look(ref ticksToDisappear, "ticksToDisappear", int.MinValue);
		}


		public override void PostSpawnSetup(bool respawningAfterLoad) {
			base.PostSpawnSetup(respawningAfterLoad);
			if (ticksToDisappear == int.MinValue) {
				ticksToDisappear = Props.disappearsAfterRareTicks;
			}
		}


		public override void CompTickRare() {
			if (CompShouldRemove) {
				IntVec3 parentPos = parent.Position;
				Map parentMap = parent.Map;

				parent.Destroy();

				if (Props.filthLeaving != null && (!Props.disappearsInRain || ( parentMap.roofGrid.Roofed(parentPos) || parentMap.weatherManager.RainRate <= 0))) {
					LeaveFilth(parentPos, parentMap);
				}
			}
			else {
				int num = 1;
				if (Props.disappearsInRain && !parent.Map.roofGrid.Roofed(parent.Position) && parent.Map.weatherManager.RainRate > 0) {
					num = (int)(num * Props.rainMultiplier);
				}
				ticksToDisappear -= num;
			}
		}


		private void LeaveFilth(IntVec3 pos, Map map) {
			if (pos.InBounds(map) && pos.Walkable(map)) {
				FilthMaker.TryMakeFilth(pos, map, Props.filthLeaving, 1);
			}
		}
	}
}
