using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using UnityEngine;
using RimWorld;

namespace ProximityFuze {
	public class Building_TrapExplosiveProximityFuze : Building_TrapExplosive {

		private const int tickInterval = 60;

		private int lastTick = new System.Random().Next(0, tickInterval) + Find.TickManager.TicksGame;
		internal float radiusFactor;

		public override void PostMake() {
			base.PostMake();
			radiusFactor = ProximityFuzeSettings.radiusFactors[def.defName];
		}

		public override void Tick() {
			if(Find.TickManager.TicksGame - tickInterval > lastTick) {
				lastTick = Find.TickManager.TicksGame;
				if(base.Spawned) {
					var pawns = Map.mapPawns.AllPawnsSpawned.FindAll(x => !x.Downed && x.HostileTo(this));
					foreach(var pawn in pawns)
						if((pawn.Position.DistanceTo(this.Position) < Mathf.Max(1f, GetComp<CompExplosive>().Props.explosiveRadius * radiusFactor)) && GenSight.LineOfSight(Position, pawn.Position, Map))
							Spring(pawn);
				}
			}
			foreach(var comp in AllComps) comp.CompTick();
		}

		public override void ExposeData() {
			base.ExposeData();
			Scribe_Values.Look(ref radiusFactor, "ProximityFuze.radiusFactor");
		}

	}
}
