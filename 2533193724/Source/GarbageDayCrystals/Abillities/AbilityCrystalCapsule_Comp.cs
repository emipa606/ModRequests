using System;
using System.Collections.Generic;
using System.Linq;
using Crystosentient.Dictionary;
using RimWorld;
using UnityEngine;
using Verse;

namespace Crystosentient.Abillities
{
	// Token: 0x0200001E RID: 30
	public class AbilityCrystalCapsule_Comp : CompAbilityEffect
	{
		// Token: 0x0600007E RID: 126 RVA: 0x00004A50 File Offset: 0x00002C50
		public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
		{
			base.Apply(target, dest);
			Map map = this.parent.pawn.Map;
			new List<Thing>().AddRange(this.WallCells(target, map).SelectMany((IntVec3 c) => from t in c.GetThingList(map)
			where t.def.category == ThingCategory.Item
			select t));
			foreach (IntVec3 loc in this.WallCells(target, map))
			{
				Thing thing = ThingMaker.MakeThing(this.Props.spawnBuildingDef, null);
				thing.SetFaction(Faction.OfPlayer, null);
				GenSpawn.Spawn(thing, loc, map, WipeMode.VanishOrMoveAside);
				FleckMaker.ThrowDustPuffThick(loc.ToVector3Shifted(), map, Rand.Range(1.5f, 3f), this.Props.dustColor);
			}
			foreach (IntVec3 intVec in this.RoofCells(target, map))
			{
				map.roofGrid.SetRoof(intVec, DefOfRoof.GDCRYST_ROOF_Amethyst);
				MoteMaker.MakeStaticMote(intVec, map, DefOfMote.GDCRYST_MOTE_AmethystRoof, 1f);
				FleckMaker.ThrowDustPuffThick(intVec.ToVector3Shifted(), map, Rand.Range(1.5f, 3f), this.Props.dustColor);
			}
			foreach (IntVec3 c2 in this.TerrainCells(target, map))
			{
				map.terrainGrid.SetTerrain(c2, DefOfTerrain.GDCRYST_BUILDABLE_FloorAmethystRough);
				FleckMaker.ThrowDustPuffThick(c2.ToVector3Shifted(), map, Rand.Range(1.5f, 3f), this.Props.dustColor);
			}
		}

		// Token: 0x0600007F RID: 127 RVA: 0x00004C7C File Offset: 0x00002E7C
		public override void DrawEffectPreview(LocalTargetInfo target)
		{
			GenDraw.DrawFieldEdges(this.WallCells(target, this.parent.pawn.Map).ToList<IntVec3>(), this.Valid(target, false) ? Color.white : Color.red, null);
		}

		// Token: 0x06000080 RID: 128 RVA: 0x0000256A File Offset: 0x0000076A
		private IEnumerable<IntVec3> WallCells(LocalTargetInfo target, Map map)
		{
			foreach (IntVec2 intVec in this.Props.pattern)
			{
				IntVec3 intVec2 = target.Cell + new IntVec3(intVec.x, 0, intVec.z);
				bool flag = intVec2.InBounds(map);
				if (flag)
				{
					yield return intVec2;
				}
				intVec2 = default(IntVec3);		
			}
			List<IntVec2>.Enumerator enumerator2 = default(List<IntVec2>.Enumerator);
			List<IntVec2>.Enumerator enumerator = default(List<IntVec2>.Enumerator);
			yield break;
			yield break;
		}

		// Token: 0x1700000F RID: 15
		// (get) Token: 0x06000081 RID: 129 RVA: 0x00004CCC File Offset: 0x00002ECC
		public new CrystalSpawners_CompProperties Props
		{
			get
			{
				return (CrystalSpawners_CompProperties)this.props;
			}
		}

		// Token: 0x06000082 RID: 130 RVA: 0x00002588 File Offset: 0x00000788
		private IEnumerable<IntVec3> RoofCells(LocalTargetInfo target, Map map)
		{
			foreach (IntVec2 intVec in this.Props.patternRoof)
			{
				IntVec3 intVec2 = target.Cell + new IntVec3(intVec.x, 0, intVec.z);
				bool flag = intVec2.InBounds(map);
				if (flag)
				{
					yield return intVec2;
				}
				intVec2 = default(IntVec3);
			}
			List<IntVec2>.Enumerator enumerator2 = default(List<IntVec2>.Enumerator);
			List<IntVec2>.Enumerator enumerator = default(List<IntVec2>.Enumerator);
			yield break;
		}

		// Token: 0x06000083 RID: 131 RVA: 0x00004CEC File Offset: 0x00002EEC
		public static void PlaceTempGemRoof(IntVec3 cell, Map map)
		{
			bool flag = !cell.ShouldSpawnMotesAt(map);
			if (!flag)
			{
				Mote mote = (Mote)ThingMaker.MakeThing(DefOfMote.GDCRYST_MOTE_AmethystRoof, null);
				mote.exactPosition = cell.ToVector3Shifted();
				GenSpawn.Spawn(mote, cell, map, WipeMode.Vanish);
			}
		}

		// Token: 0x06000084 RID: 132 RVA: 0x000025A6 File Offset: 0x000007A6
		private IEnumerable<IntVec3> TerrainCells(LocalTargetInfo target, Map map)
		{
			foreach (IntVec2 intVec in this.Props.patternTerrain)
			{
				IntVec3 intVec2 = target.Cell + new IntVec3(intVec.x, 0, intVec.z);
				bool flag = intVec2.InBounds(map);
				if (flag)
				{
					yield return intVec2;
				}
				intVec2 = default(IntVec3);
			}
			List<IntVec2>.Enumerator enumerator2 = default(List<IntVec2>.Enumerator);
			List<IntVec2>.Enumerator enumerator = default(List<IntVec2>.Enumerator);
			yield break;

		}
	}
}
