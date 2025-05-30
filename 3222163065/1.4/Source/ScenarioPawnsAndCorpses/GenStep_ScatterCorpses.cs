﻿using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace ScenarioPawnsAndCorpses
{
    /// <summary>
	/// Class enabling the inserting of corpses into the game.
	/// </summary>
	public class GenStep_ScatterCorpses : GenStep_Scatterer
    {
		/// <summary>
		/// Ran seed; generated by random int
		/// </summary>
		public override int SeedPart => 1145469797;

		/// <summary>
		/// The list of corpses that can be added. This will actually generally be Pawns (ie pre-corpse corpses) to allow for post-generate killing to preserve weapons etc.
		/// </summary>
		public IEnumerable<Thing> Corpses = null;

		/// <summary>
		/// Ugly hacky index into Corpses
		/// </summary>
		private int currentIndex = 0;

		/// <summary>
		/// Amount of space to clear for a corpse
		/// </summary>
		public int clearSpaceSize;

		/// <summary>
		/// Number of corpses to shove together. Will almost certainly always be 1.
		/// </summary>
		public int clusterSize = 1;

		/// <summary>
		/// Center of the cluster.
		/// </summary>
		[Unsaved(false)]
		private IntVec3 clusterCenter;

		/// <summary>
		/// Number of corpses left in the cluster (ie max clusterSize, min 0)
		/// </summary>
		[Unsaved(false)]
		private int leftInCluster;

		/// <summary>
		/// Cached calculation of possible rotations. Might be issues for random corpses if there are corpses without rotations?
		/// </summary>
		private List<Rot4> possibleRotationsInt;

		/// <summary>
		/// Get the possible rotations of corpses/pawns.
		/// </summary>
		/// <param name="thingDef">Sample corpse/pawn def</param>
		/// <returns>The possible rotations of the provided thing def</returns>
		private List<Rot4> GetPossibleRotations(ThingDef thingDef)
		{
			if (this.possibleRotationsInt == null)
			{
				this.possibleRotationsInt = new List<Rot4>();
				if (thingDef.rotatable)
				{
					this.possibleRotationsInt.Add(Rot4.North);
					this.possibleRotationsInt.Add(Rot4.East);
					this.possibleRotationsInt.Add(Rot4.South);
					this.possibleRotationsInt.Add(Rot4.West);
				}
				else
				{
					this.possibleRotationsInt.Add(Rot4.North);
				}
			}
			return this.possibleRotationsInt;
		}

		/// <summary>
		/// Construct new instance of GenStep_ScatterCorpses
		/// </summary>
		/// <param name="corpses">The list of corpses that will be generated</param>
		public GenStep_ScatterCorpses(IEnumerable<Thing> corpses)
        {
            this.Corpses = corpses;
			this.count = corpses?.Count() ?? 0;
			this.currentIndex = 0;
		}

		/// <summary>
		/// Create and spawn the corpses on the map
		/// </summary>
		/// <param name="map">The map to spawn the corpses into</param>
		/// <param name="parms">unused</param>
		public override void Generate(Map map, GenStepParams parms)
		{
			this.currentIndex = 0;

			for (int i = 0; i < this.count; i++)
			{
				if (!this.TryFindScatterCell(map, out var result))
				{
					return;
				}
				this.ScatterAt(result, map, parms);
				this.usedSpots.Add(result);
			}
			this.usedSpots.Clear();
			this.clusterCenter = IntVec3.Invalid;
			this.leftInCluster = 0;
		}

		/// <summary>
		/// Find an available cell to add the corpse into
		/// </summary>
		/// <param name="map">The map the corpses will end up being spanwed into</param>
		/// <param name="result">An available cell</param>
		/// <returns>Whether there is am available cell</returns>
		protected override bool TryFindScatterCell(Map map, out IntVec3 result)
		{
			if (this.clusterSize > 1)
			{
				if (this.leftInCluster <= 0)
				{
					if (!base.TryFindScatterCell(map, out this.clusterCenter))
					{
						Log.Error("Could not find cluster center to scatter corpse");
					}
					this.leftInCluster = this.clusterSize;
				}
				this.leftInCluster--;
				result = CellFinder.RandomClosewalkCellNear(this.clusterCenter, map, 4, (IntVec3 x) => this.TryGetRandomValidRotation(x, map, this.Corpses.ElementAt(this.currentIndex).def, out var _));
				return result.IsValid;
			}
			return base.TryFindScatterCell(map, out result);
		}

		/// <summary>
		/// Find a valid rotation for the corpse.
		/// </summary>
		/// <param name="loc">The cell the corpse will be added into</param>
		/// <param name="map">The hosting map</param>
		/// <param name="thingDef">The corpse's def</param>
		/// <param name="rot">The rotation</param>
		/// <returns>Whether there is a valid rotation</returns>
		private bool TryGetRandomValidRotation(IntVec3 loc, Map map, ThingDef thingDef, out Rot4 rot)
		{
			List<Rot4> tmpRotations = new List<Rot4>();

			for (int i = 0; i < this.GetPossibleRotations(thingDef).Count; i++)
			{
				if (this.IsRotationValid(loc, this.GetPossibleRotations(thingDef)[i], thingDef, map))
				{
					tmpRotations.Add(this.GetPossibleRotations(thingDef)[i]);
				}
			}
			if (tmpRotations.TryRandomElement(out rot))
			{
				tmpRotations.Clear();
				return true;
			}
			rot = Rot4.Invalid;
			return false;
		}

		/// <summary>
		/// Whether the rotation is valid
		/// </summary>
		/// <param name="loc">The cell the corpse will be added into</param>
		/// <param name="rot">The rotation to check</param>
		/// <param name="thingDef">The corpse's def</param>
		/// <param name="map">The hosting map</param>
		/// <returns>Whether this rotation is valid</returns>
		private bool IsRotationValid(IntVec3 loc, Rot4 rot, ThingDef thingDef, Map map)
		{
			if (!GenAdj.OccupiedRect(loc, rot, thingDef.size).InBounds(map))
			{
				return false;
			}
			if (GenSpawn.WouldWipeAnythingWith(loc, rot, thingDef, map, (Thing x) => x.def == thingDef || (x.def.category != ThingCategory.Plant && x.def.category != ThingCategory.Filth)))
			{
				return false;
			}
			return true;
		}

		/// <summary>
		/// Attempt to add the chunk of corpses to the provided cell (and surrounds)
		/// </summary>
		/// <param name="loc">The cell the corpse will be added into</param>
		/// <param name="map">The hosting map</param>
		/// <param name="parms">unused</param>
		/// <param name="stackCount">The number of corpses to add</param>
		protected override void ScatterAt(IntVec3 loc, Map map, GenStepParams parms, int stackCount = 1)
		{
			Thing corpse = this.Corpses.ElementAt(this.currentIndex);

			if (!this.TryGetRandomValidRotation(loc, map, corpse.def, out var rot))
			{
				Log.Warning("Could not find any valid rotation for " + corpse.Label);
				return;
			}
			this.currentIndex++;

			if (this.clearSpaceSize > 0)
			{
				foreach (IntVec3 item in GridShapeMaker.IrregularLump(loc, map, clearSpaceSize))
				{
					item.GetEdifice(map)?.Destroy();
				}
			}

			GenSpawn.Spawn(corpse, loc, map, rot);

			if (corpse is Pawn pawn)
            {
				HealthUtility.DamageUntilDead(pawn);
				corpse = pawn.Corpse;
			}

			ThingDef filthDef = (corpse as Corpse)?.InnerPawn.RaceProps.BloodDef;
			float filthChance = 0.5f;
			int filthExpandBy = 2;

			if (filthDef != null)
			{
				foreach (IntVec3 item2 in corpse.OccupiedRect().ExpandedBy(filthExpandBy))
				{
					if (Rand.Chance(filthChance) && item2.InBounds(corpse.Map))
					{
						FilthMaker.TryMakeFilth(item2, corpse.Map, filthDef, (corpse as Corpse).InnerPawn.LabelShort);
					}
				}
			}
		}
	}
}
