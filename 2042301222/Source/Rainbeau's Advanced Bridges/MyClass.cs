using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Verse;

namespace RBB_Code {

	public class PlaceWorker_BasicBridge : PlaceWorker {
		public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null) {
			TerrainDef terrainDef = map.terrainGrid.TerrainAt(loc);
			if (terrainDef.defName == "MarshyTerrain" || terrainDef.defName == "WaterMovingChestDeep") {
			    return new AcceptanceReport("RAB.BasicBridge".Translate());
			}
			List<Thing> things = map.thingGrid.ThingsListAtFast(loc);
			for (int j = 0; j < things.Count; j++) {
				if (things[j] != thingToIgnore) {
					if (things[j].def.defName == "RBB_FishingSpot") {
						return new AcceptanceReport("RAB.NotOnFS".Translate());
					}
				}
			}
			return true;
		}
	}

	public class PlaceWorker_DeepWaterBridge : PlaceWorker {
		public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null) {
			List<Thing> things = map.thingGrid.ThingsListAtFast(loc);
			for (int j = 0; j < things.Count; j++) {
				if (things[j] != thingToIgnore) {
					if (things[j].def.defName == "RBB_FishingSpot") {
						return new AcceptanceReport("RAB.NotOnFS".Translate());
					}
				}
			}
			return true;
		}
	}
			
	public class PlaceWorker_Boardwalk : PlaceWorker {
		public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null) {
			TerrainDef terrainDef = map.terrainGrid.TerrainAt(loc);
			if (terrainDef.defName.Contains("Bridge")) {
			    return new AcceptanceReport("RAB.NoFloorOnBridge".Translate());
			}
			List<Thing> things = map.thingGrid.ThingsListAtFast(loc);
			for (int k = 0; k < things.Count; k++) {
				if (things[k] != thingToIgnore) {
					if (things[k].def.defName.Contains("RBB_") || things[k].def.defName.Contains("Wall")) {
						return new AcceptanceReport("RAB.NoFloorOnBridge".Translate());
					}
				}
			}
			if (terrainDef.defName.Equals("MarshyTerrain") || terrainDef.defName.Equals("Mud")) {
				return true;
			}
			string waterNear = "no";
			for (int i = -1; i < 2; i++) {
				for (int j = -1; j < 2; j++) {
					int x = loc.x + i;
					int z = loc.z + j;
					IntVec3 newSpot = new IntVec3(x, 0, z);
					string terrainCheck = map.terrainGrid.TerrainAt(newSpot).defName;
					if (terrainCheck.Contains("Water") || terrainCheck.Equals("Marsh") || terrainCheck.Contains("BridgeMarsh") || terrainCheck.Contains("Mud") || terrainCheck.Contains("MarshyTerrain")) {
						waterNear = "yes";
						break;
					}
				}
			}
			if (waterNear == "no") {
				for (int i = -2; i < 3; i++) {
					for (int j = -2; j < 3; j++) {
						int x = loc.x + i;
						int z = loc.z + j;
						IntVec3 newSpot = new IntVec3(x, 0, z);
						string terrainCheck = map.terrainGrid.TerrainAt(newSpot).defName;
						if (terrainCheck.Contains("Water") || terrainCheck.Equals("Marsh") || terrainCheck.Contains("BridgeMarsh") || terrainCheck.Contains("Mud") || terrainCheck.Contains("MarshyTerrain")) {
							waterNear = "yes";
							break;
						}
					}
				}
			}
			if (waterNear == "no") {
				for (int i = -3; i < 4; i++) {
					for (int j = -3; j < 4; j++) {
						int x = loc.x + i;
						int z = loc.z + j;
						IntVec3 newSpot = new IntVec3(x, 0, z);
						string terrainCheck = map.terrainGrid.TerrainAt(newSpot).defName;
						if (terrainCheck.Contains("Water") || terrainCheck.Equals("Marsh") || terrainCheck.Contains("BridgeMarsh") || terrainCheck.Contains("Mud") || terrainCheck.Contains("MarshyTerrain")) {
							waterNear = "yes";
							break;
						}
					}
				}
			}
			if (waterNear == "yes") {
				return true;
			}
			return new AcceptanceReport("RAB.Boardwalk".Translate());
		}
	}

	public class PlaceWorker_WatergazingSpot : PlaceWorker {
		public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null) {
			TerrainDef terrainDef = map.terrainGrid.TerrainAt(loc);
			if (!terrainDef.defName.Contains("Water") && !terrainDef.defName.Equals("Marsh")) {
				return new AcceptanceReport("RAB.FishingSpot1".Translate());
			}
			ThingDef thingDef = checkingDef as ThingDef;
			IntVec3 intVec3 = ThingUtility.InteractionCellWhenAt(thingDef, loc, rot, map);
			if (!intVec3.InBounds(map)) {
				return new AcceptanceReport("InteractionSpotOutOfBounds".Translate());
			}
			List<Thing> things = map.thingGrid.ThingsListAtFast(intVec3);
			for (int j = 0; j < things.Count; j++) {
				if (things[j] != thingToIgnore) {
					if (things[j].def.passability == Traversability.Impassable) {
						return new AcceptanceReport("InteractionSpotBlocked".Translate(new object[] { things[j].LabelNoCount }).CapitalizeFirst());
					}
					Blueprint blueprint = things[j] as Blueprint;
					if (blueprint != null && blueprint.def.entityDefToBuild.passability == Traversability.Impassable) {
						return new AcceptanceReport("InteractionSpotWillBeBlocked".Translate(new object[] { blueprint.LabelNoCount }).CapitalizeFirst());
					}
				}
			}
			TerrainDef landCheck = map.terrainGrid.TerrainAt(intVec3);
			if (!landCheck.defName.Contains("Water") && !(landCheck == TerrainDef.Named("Marsh"))) {
				return true;
			}
			if (landCheck.defName.Contains("Bridge")) {
				return true;
			}
			return new AcceptanceReport("RAB.FishingSpot2".Translate());
		}
	}

	public class Building_WatergazingSpot : Building_WorkTable {
		public IntVec3 gazingSpotCell = new IntVec3(0, 0, 0);
		public override void TickRare() {
			base.TickRare();
			gazingSpotCell = this.Position + new IntVec3(0, 0, 0).RotatedBy(this.Rotation);
			TerrainDef terrainDef = base.Map.terrainGrid.TerrainAt(this.gazingSpotCell);
			if (!terrainDef.defName.Contains("Water") && !terrainDef.defName.Equals("Marsh") && !terrainDef.defName.Contains("Ice")) {
				this.Destroy();
			}
			else {
				TerrainDef terrainDefIS = base.Map.terrainGrid.TerrainAt(this.InteractionCell);
				if (terrainDefIS.defName.Contains("Water") || terrainDefIS.defName.Equals("Marsh")) {
					if (!terrainDefIS.defName.Contains("Bridge")) {
						this.Destroy();
					}
				}
			}
		}
	}	
	
	public class PlaceWorker_VBridgeBase : PlaceWorker {
		public AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null) {
			TerrainDef terrainDef = map.terrainGrid.TerrainAt(loc);
			if (terrainDef.defName == "WaterMovingChestDeep") {
			    return new AcceptanceReport("RAB.BasicBridge".Translate());
			}
			List<Thing> things = map.thingGrid.ThingsListAtFast(loc);
			for (int j = 0; j < things.Count; j++) {
				if (things[j] != thingToIgnore) {
					if (things[j].def.defName == "RBB_FishingSpot") {
						return new AcceptanceReport("RAB.NotOnFS".Translate());
					}
				}
			}
			return true;
		}
	}
	
	public class PlaceWorker_FloorBase : PlaceWorker {
		public AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null) {
			TerrainDef terrainDef = map.terrainGrid.TerrainAt(loc);
			if (terrainDef.defName.Equals("Mud")) {
				return new AcceptanceReport("TerrainCannotSupport".Translate());
			}
			List<Thing> things = map.thingGrid.ThingsListAtFast(loc);
			for (int k = 0; k < things.Count; k++) {
				if (things[k] != thingToIgnore) {
					if (things[k].def.defName.Contains("RBB_") && things[k].def.defName.Contains("Bridge")) {
						return new AcceptanceReport("RAB.NoFloorOnBridge".Translate());
					}
				}
			}
			return true;
		}
	}
	
	public class Building_Bridge : Building {

		public readonly static TerrainDef BridgeSoilDef = TerrainDef.Named("BridgeLand");

		public string TerrainTypeAtBaseCellDefAsString;
		public override void Destroy(DestroyMode mode = 0) {
			base.Map.snowGrid.SetDepth(base.Position, 0f);
			if (this.TerrainTypeAtBaseCellDefAsString != null) {
				base.Map.terrainGrid.SetTerrain(base.Position, TerrainDef.Named(this.TerrainTypeAtBaseCellDefAsString));
			}
			base.Destroy(mode);
		}
		public override void ExposeData() {
			base.ExposeData();
			Scribe_Values.Look<string>(ref this.TerrainTypeAtBaseCellDefAsString, "TerrainTypeAtBaseCellDefAsString", null, false);
		}
		public override void SpawnSetup(Map map, bool flag) {
			base.SpawnSetup(map, flag);
			TerrainDef terrainDef = map.terrainGrid.TerrainAt(base.Position);
			if (!terrainDef.defName.Contains("Bridge") && !terrainDef.defName.Contains("Floor") && !terrainDef.defName.Contains("Carpet")
			  && !terrainDef.defName.Contains("Tile") && !terrainDef.defName.Contains("Flagstone") && !terrainDef.defName.Contains("Concrete")) {
				this.TerrainTypeAtBaseCellDefAsString = terrainDef.ToString();
				if (terrainDef == TerrainDef.Named("WaterShallow")) {
					map.terrainGrid.SetTerrain(base.Position, TerrainDef.Named("BridgeWaterShallow"));
				}
				else if (terrainDef == TerrainDef.Named("WaterOceanShallow")) {
					map.terrainGrid.SetTerrain(base.Position, TerrainDef.Named("BridgeWaterOceanShallow"));
				}
				else if (terrainDef == TerrainDef.Named("WaterMovingShallow")) {
					map.terrainGrid.SetTerrain(base.Position, TerrainDef.Named("BridgeWaterMovingShallow"));
				}
				else if (terrainDef == TerrainDef.Named("WaterMovingChestDeep")) {
					map.terrainGrid.SetTerrain(base.Position, TerrainDef.Named("BridgeWaterMovingChestDeep"));
				}
				else if (terrainDef == TerrainDef.Named("WaterDeep")) {
					map.terrainGrid.SetTerrain(base.Position, TerrainDef.Named("BridgeWaterDeep"));
				}
				else if (terrainDef == TerrainDef.Named("WaterOceanDeep")) {
					map.terrainGrid.SetTerrain(base.Position, TerrainDef.Named("BridgeWaterOceanDeep"));
				}
				else if (terrainDef == TerrainDef.Named("Marsh")) {
					map.terrainGrid.SetTerrain(base.Position, TerrainDef.Named("BridgeMarsh"));
				}
				else if (terrainDef == TerrainDef.Named("Mud")) {
					map.terrainGrid.SetTerrain(base.Position, TerrainDef.Named("BridgeMud"));
				}
				else if (terrainDef == TerrainDef.Named("Sand")) {
					map.terrainGrid.SetTerrain(base.Position, TerrainDef.Named("BridgeSand"));
				}
				else if (terrainDef == TerrainDef.Named("MarshyTerrain")) {
					map.terrainGrid.SetTerrain(base.Position, TerrainDef.Named("BridgeMarshyTerrain"));
				}
				else if (terrainDef == TerrainDef.Named("Ice")) {
					map.terrainGrid.SetTerrain(base.Position, TerrainDef.Named("BridgeIce"));
				}
				else if (terrainDef.defName == "IceST" || terrainDef.defName == "IceS") {
					this.TerrainTypeAtBaseCellDefAsString = "WaterShallow";
					map.terrainGrid.SetTerrain(base.Position, TerrainDef.Named("BridgeWaterShallow"));
				}
				else if (terrainDef.defName == "IceDT" || terrainDef.defName == "IceD") {
					this.TerrainTypeAtBaseCellDefAsString = "WaterDeep";
					map.terrainGrid.SetTerrain(base.Position, TerrainDef.Named("BridgeWaterDeep"));
				}
				else if (terrainDef.defName == "IceSMT" || terrainDef.defName == "IceSM") {
					this.TerrainTypeAtBaseCellDefAsString = "WaterMovingShallow";
					map.terrainGrid.SetTerrain(base.Position, TerrainDef.Named("BridgeWaterMovingShallow"));
				}
				else if (terrainDef.defName == "IceDMT" || terrainDef.defName == "IceDM") {
					this.TerrainTypeAtBaseCellDefAsString = "WaterMovingChestDeep";
					map.terrainGrid.SetTerrain(base.Position, TerrainDef.Named("BridgeWaterMovingChestDeep"));
				}
				else if (terrainDef.defName == "IceMarshT" || terrainDef.defName == "IceMarsh") {
					this.TerrainTypeAtBaseCellDefAsString = "Marsh";
					map.terrainGrid.SetTerrain(base.Position, TerrainDef.Named("BridgeMarsh"));
				}
				else if (terrainDef.defName.Contains("Water")) {
					map.terrainGrid.SetTerrain(base.Position, TerrainDef.Named("BridgeWaterShallow"));
				}
				else
				{

					var affordances = map.terrainGrid.TerrainAt(base.Position).affordances;

					//no need to replace if affordances are met
					foreach (var affordance in BridgeSoilDef.affordances)
					{
						if (!affordances.Contains(affordance))
						{
							map.terrainGrid.SetTerrain(base.Position, BridgeSoilDef);
							return;
						}
					}
				}
			}
		}
	}

	public class Building_Bridge_Stone : Building {

		public readonly static TerrainDef StoneBridgeSoilDef = TerrainDef.Named("StoneBridgeLand");

		public string TerrainTypeAtBaseCellDefAsString;
		public override void Destroy(DestroyMode mode = 0) {
			base.Map.snowGrid.SetDepth(base.Position, 0f);
			if (this.TerrainTypeAtBaseCellDefAsString != null) {
				base.Map.terrainGrid.SetTerrain(base.Position, TerrainDef.Named(this.TerrainTypeAtBaseCellDefAsString));
			}
			base.Destroy(mode);
		}
		public override void ExposeData() {
			base.ExposeData();
			Scribe_Values.Look<string>(ref this.TerrainTypeAtBaseCellDefAsString, "TerrainTypeAtBaseCellDefAsString", null, false);
		}
		public override void SpawnSetup(Map map, bool flag) {
			base.SpawnSetup(map, flag);
			TerrainDef terrainDef = map.terrainGrid.TerrainAt(base.Position);
			if (!terrainDef.defName.Contains("Bridge") && !terrainDef.defName.Contains("Floor") && !terrainDef.defName.Contains("Carpet")
			  && !terrainDef.defName.Contains("Tile") && !terrainDef.defName.Contains("Flagstone") && !terrainDef.defName.Contains("Concrete")) {
				this.TerrainTypeAtBaseCellDefAsString = terrainDef.ToString();
				if (terrainDef == TerrainDef.Named("WaterShallow")) {
					map.terrainGrid.SetTerrain(base.Position, TerrainDef.Named("StoneBridgeWaterShallow"));
				}
				else if (terrainDef == TerrainDef.Named("WaterOceanShallow")) {
					map.terrainGrid.SetTerrain(base.Position, TerrainDef.Named("StoneBridgeWaterOceanShallow"));
				}
				else if (terrainDef == TerrainDef.Named("WaterMovingShallow")) {
					map.terrainGrid.SetTerrain(base.Position, TerrainDef.Named("StoneBridgeWaterMovingShallow"));
				}
				else if (terrainDef == TerrainDef.Named("WaterMovingChestDeep")) {
					map.terrainGrid.SetTerrain(base.Position, TerrainDef.Named("StoneBridgeWaterMovingChestDeep"));
				}
				else if (terrainDef == TerrainDef.Named("WaterDeep")) {
					map.terrainGrid.SetTerrain(base.Position, TerrainDef.Named("StoneBridgeWaterDeep"));
				}
				else if (terrainDef == TerrainDef.Named("WaterOceanDeep")) {
					map.terrainGrid.SetTerrain(base.Position, TerrainDef.Named("StoneBridgeWaterOceanDeep"));
				}
				else if (terrainDef == TerrainDef.Named("Marsh")) {
					map.terrainGrid.SetTerrain(base.Position, TerrainDef.Named("StoneBridgeMarsh"));
				}
				else if (terrainDef == TerrainDef.Named("Mud")) {
					map.terrainGrid.SetTerrain(base.Position, TerrainDef.Named("StoneBridgeMud"));
				}
				else if (terrainDef == TerrainDef.Named("Sand")) {
					map.terrainGrid.SetTerrain(base.Position, TerrainDef.Named("StoneBridgeSand"));
				}
				else if (terrainDef == TerrainDef.Named("MarshyTerrain")) {
					map.terrainGrid.SetTerrain(base.Position, TerrainDef.Named("StoneBridgeMarshyTerrain"));
				}
				else if (terrainDef == TerrainDef.Named("Ice")) {
					map.terrainGrid.SetTerrain(base.Position, TerrainDef.Named("StoneBridgeIce"));
				}
				else if (terrainDef.defName == "IceST" || terrainDef.defName == "IceS") {
					this.TerrainTypeAtBaseCellDefAsString = "WaterShallow";
					map.terrainGrid.SetTerrain(base.Position, TerrainDef.Named("StoneBridgeWaterShallow"));
				}
				else if (terrainDef.defName == "IceDT" || terrainDef.defName == "IceD") {
					this.TerrainTypeAtBaseCellDefAsString = "WaterDeep";
					map.terrainGrid.SetTerrain(base.Position, TerrainDef.Named("StoneBridgeWaterDeep"));
				}
				else if (terrainDef.defName == "IceSMT" || terrainDef.defName == "IceSM") {
					this.TerrainTypeAtBaseCellDefAsString = "WaterMovingShallow";
					map.terrainGrid.SetTerrain(base.Position, TerrainDef.Named("StoneBridgeWaterMovingShallow"));
				}
				else if (terrainDef.defName == "IceDMT" || terrainDef.defName == "IceDM") {
					this.TerrainTypeAtBaseCellDefAsString = "WaterMovingChestDeep";
					map.terrainGrid.SetTerrain(base.Position, TerrainDef.Named("StoneBridgeWaterMovingChestDeep"));
				}
				else if (terrainDef.defName == "IceMarshT" || terrainDef.defName == "IceMarsh") {
					this.TerrainTypeAtBaseCellDefAsString = "Marsh";
					map.terrainGrid.SetTerrain(base.Position, TerrainDef.Named("StoneBridgeMarsh"));
				}
				else if (terrainDef.defName.Contains("Water")) {
					map.terrainGrid.SetTerrain(base.Position, TerrainDef.Named("StoneBridgeWaterShallow"));
				}
				else if (terrainDef.defName.Contains("WaterShallowLagoon"))
				{
					map.terrainGrid.SetTerrain(base.Position, TerrainDef.Named("StoneBridgeWaterShallowLagoon"));
				}
				else {
					var affordances = map.terrainGrid.TerrainAt(base.Position).affordances;

					//no need to replace if affordances are met
					foreach (var affordance in StoneBridgeSoilDef.affordances)
					{
						if (!affordances.Contains(affordance))
						{
							map.terrainGrid.SetTerrain(base.Position, StoneBridgeSoilDef);
							return;
						}
					}
				}
			}
		}
	}

}
