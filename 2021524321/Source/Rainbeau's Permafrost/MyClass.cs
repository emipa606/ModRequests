using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using HarmonyLib;

namespace RPF_Code {

	[StaticConstructorOnStartup]
	internal static class RPF_Initializer {
		static RPF_Initializer() {
			foreach (ThingDef current in DefDatabase<ThingDef>.AllDefsListForReading) {
				if (current.plant != null) {
					if (current.plant.wildBiomes != null) {
						for (int j = 0; j < current.plant.wildBiomes.Count; j++) {
							if (current.plant.wildBiomes[j].biome.defName == "Tundra") {
								PlantBiomeRecord newRecord = new PlantBiomeRecord();
								newRecord.biome = BiomeDef.Named("Permafrost");
								newRecord.commonality = current.plant.wildBiomes[j].commonality/2;
								current.plant.wildBiomes.Add(newRecord);
							}
						}
					}
				}
			}
			foreach (PawnKindDef current in DefDatabase<PawnKindDef>.AllDefs) {
				if (current.RaceProps.wildBiomes != null) {
					for (int j = 0; j < current.RaceProps.wildBiomes.Count; j++) {
						if (current.RaceProps.wildBiomes[j].biome.defName == "Tundra") {
							AnimalBiomeRecord newRecord = new AnimalBiomeRecord();
							newRecord.biome = BiomeDef.Named("Permafrost");
							newRecord.commonality = current.RaceProps.wildBiomes[j].commonality/2;
							current.RaceProps.wildBiomes.Add(newRecord);
						}
					}
				}
			}
		}
	}
	
	public class Controller : Mod {
		public static Settings Settings;
		public override string SettingsCategory() { return "RPF.Permafrost".Translate(); }
		public override void DoSettingsWindowContents(Rect canvas) { Settings.DoWindowContents(canvas); }
		public Controller(ModContentPack content) : base(content) {
			Settings = GetSettings<Settings>();
		}
	}

	public class Settings : ModSettings {
		public bool waterFreezes = true;
		public void DoWindowContents(Rect canvas) {
			Listing_Standard list = new Listing_Standard();
			list.ColumnWidth = canvas.width;
			list.Begin(canvas);
			list.Gap();
			list.CheckboxLabeled( "RPF.WaterFreezes".Translate(), ref waterFreezes, "RPF.WaterFreezesTip".Translate() );
			list.Gap();
			list.End();
		}
		public override void ExposeData() {
			base.ExposeData();
			Scribe_Values.Look(ref waterFreezes, "waterFreezes", true);
		}
	}		

	public class Lakes : MapComponent {
		public int cycleIndex;
		public Lakes(Map map) : base(map) { }
		public override void MapComponentTick() {
			base.MapComponentTick();
			if (Controller.Settings.waterFreezes.Equals(false)) { return; }
			int num = Mathf.RoundToInt((float)this.map.Area * 0.00005f);
			int area = this.map.Area;
			float cellTemp;
			for (int i = 0; i < num; i++) {
				this.cycleIndex++;
				if (this.cycleIndex >= area) { this.cycleIndex = 0; }
				IntVec3 c = this.map.cellsInRandomOrder.Get(this.cycleIndex);
				var terrainDef = map.terrainGrid.TerrainAt(c);
				if (!GenTemperature.TryGetTemperatureForCell(c, map, out cellTemp)) { cellTemp = 0f; }
				float shallowChance = (cellTemp * cellTemp) / 100f;
				float deepChance = (cellTemp * cellTemp * 0.2f) / 100f;
				float permafrostChance = deepChance;
				if (permafrostChance > 0.2f) { permafrostChance = 0.2f; }
				if (cellTemp < 0f) {
					if (terrainDef.defName == "WaterShallow" || terrainDef.defName == "IceST" ) {
						int freezable = 0;
						for (int k = -1; k < 2; k++) {
							for (int j = -1; j < 2; j++) {
								int x = c.x + k;
								int z = c.z + j;
								if ((k == 0 && j == 1) || (k == 0 && j == -1) || (k == 1 && j == 0) || (k == -1 && j == 0)) {
									if (x > 0 && x < this.map.Size.x && z > 0 && z < this.map.Size.z) {
										IntVec3 newSpot = new IntVec3(x, 0, z);
										if (!map.terrainGrid.TerrainAt(newSpot).defName.Contains("Water") && map.terrainGrid.TerrainAt(newSpot).defName != "Marsh" && map.terrainGrid.TerrainAt(newSpot).defName != "BridgeMarsh" && map.terrainGrid.TerrainAt(newSpot).defName != "Bridge") {
											freezable++;
										}
									}
								}
							}
						}
						if (Rand.Value < shallowChance * freezable) {
							List<Thing> thingList = c.GetThingList(map);
							for (int j = 0; j < thingList.Count; j++) {
								Thing item = thingList[j];
								if (item.def.defName == "Fishing Spot" || item.def.defName == "ZARS_FishingSpot") {
									item.Destroy(DestroyMode.Vanish);
								}
							}
							string iceType;
							if (terrainDef.defName == "WaterShallow") { iceType = "IceST"; }
							else { iceType = "IceS"; }
							map.terrainGrid.SetTerrain (c, TerrainDef.Named(iceType));
						}
					}
					if (terrainDef.defName == "WaterDeep" || terrainDef.defName == "IceDT" ) {
						int freezable = 0;
						for (int k = -1; k < 2; k++) {
							for (int j = -1; j < 2; j++) {
								int x = c.x + k;
								int z = c.z + j;
								if ((k == 0 && j == 1) || (k == 0 && j == -1) || (k == 1 && j == 0) || (k == -1 && j == 0)) {
									if (x > 0 && x < this.map.Size.x && z > 0 && z < this.map.Size.z) {
										IntVec3 newSpot = new IntVec3(x, 0, z);
										if (!map.terrainGrid.TerrainAt(newSpot).defName.Contains("Water") && map.terrainGrid.TerrainAt(newSpot).defName != "Marsh" && map.terrainGrid.TerrainAt(newSpot).defName != "BridgeMarsh" && map.terrainGrid.TerrainAt(newSpot).defName != "Bridge") {
											freezable++;
										}
									}
								}
							}
						}
						if (Rand.Value < deepChance * freezable) {
							List<Thing> thingList = c.GetThingList(map);
							for (int j = 0; j < thingList.Count; j++) {
								Thing item = thingList[j];
								if (item.def.defName == "Fishing Spot" || item.def.defName == "ZARS_FishingSpot") {
									item.Destroy(DestroyMode.Vanish);
								}
							}
							string iceType;
							if (terrainDef.defName == "WaterDeep") { iceType = "IceDT"; }
							else { iceType = "IceD"; }
							map.terrainGrid.SetTerrain (c, TerrainDef.Named(iceType));
						}
					}
					if (terrainDef.defName == "Marsh" || terrainDef.defName == "IceMarshT") {
						int freezable = 0;
						for (int k = -1; k < 2; k++) {
							for (int j = -1; j < 2; j++) {
								int x = c.x + k;
								int z = c.z + j;
								if ((k == 0 && j == 1) || (k == 0 && j == -1) || (k == 1 && j == 0) || (k == -1 && j == 0)) {
									if (x > 0 && x < this.map.Size.x && z > 0 && z < this.map.Size.z) {
										IntVec3 newSpot = new IntVec3(x, 0, z);
										if (!map.terrainGrid.TerrainAt(newSpot).defName.Contains("Water") && map.terrainGrid.TerrainAt(newSpot).defName != "Marsh" && map.terrainGrid.TerrainAt(newSpot).defName != "BridgeMarsh" && map.terrainGrid.TerrainAt(newSpot).defName != "Bridge") {
											freezable++;
										}
									}
								}
							}
						}
						if (Rand.Value < shallowChance * freezable) {
							List<Thing> thingList = c.GetThingList(map);
							for (int j = 0; j < thingList.Count; j++) {
								Thing item = thingList[j];
								if (item.def.defName == "Fishing Spot" || item.def.defName == "ZARS_FishingSpot") {
									item.Destroy(DestroyMode.Vanish);
								}
							}
							string iceType;
							if (terrainDef.defName == "Marsh") { iceType = "IceMarshT"; }
							else { iceType = "IceMarsh"; }
							map.terrainGrid.SetTerrain (c, TerrainDef.Named(iceType));
						}
					}
					if ((terrainDef.defName == "Soil" || terrainDef.defName == "Gravel" || terrainDef.defName == "MossyTerrain") && map.Biome.defName.Contains("Permafrost")) {
						int freezable = 0;
						for (int k = -1; k < 2; k++) {
							for (int j = -1; j < 2; j++) {
								int x = c.x + k;
								int z = c.z + j;
								if ((k == 0 && j == 1) || (k == 0 && j == -1) || (k == 1 && j == 0) || (k == -1 && j == 0)) {
									if (x > 0 && x < this.map.Size.x && z > 0 && z < this.map.Size.z) {
										IntVec3 newSpot = new IntVec3(x, 0, z);
										if (map.terrainGrid.TerrainAt(newSpot).defName == "Ice") {
											freezable++;
										}
									}
								}
							}
						}
						if (Rand.Value < (permafrostChance/8) * freezable) {
							List<Thing> thingList = c.GetThingList(map);
							for (int j = 0; j < thingList.Count; j++) {
								Thing item = thingList[j];
								if (item.def.thingClass == typeof(Plant)) {
									item.Destroy(DestroyMode.Vanish);
								}
							}
							map.terrainGrid.SetTerrain (c, TerrainDef.Named("Ice"));
						}
					}
				}
				if (cellTemp > -4f) {
					if (terrainDef.defName == "IceS" || terrainDef.defName == "IceD" || terrainDef.defName == "IceST" || terrainDef.defName == "IceDT" ) {
						int quickThaw = 1;
						for (int k = -1; k < 2; k++) {
							for (int j = -1; j < 2; j++) {
								int x = c.x + k;
								int z = c.z + j;
								if ((k == 0 && j == 1) || (k == 0 && j == -1) || (k == 1 && j == 0) || (k == -1 && j == 0)) {
									if (x > 0 && x < this.map.Size.x && z > 0 && z < this.map.Size.z) {
										IntVec3 newSpot = new IntVec3(x, 0, z);
										if (!terrainDef.label.Contains("Thin")) {
											if (map.terrainGrid.TerrainAt(newSpot).defName.Contains("Water") || map.terrainGrid.TerrainAt(newSpot).defName == "Marsh" || map.terrainGrid.TerrainAt(newSpot).label.Contains("Thin")) {
												quickThaw++;
											}
									    }
										else {
											if (map.terrainGrid.TerrainAt(newSpot).defName.Contains("Water") || map.terrainGrid.TerrainAt(newSpot).defName == "Marsh") {
												quickThaw++;
											}
										}
									}
								}
							}
						}
						if (quickThaw == 1) {
							if (Rand.Value < 0.67f) { quickThaw = 0; }
						}
						if (Rand.Value < (shallowChance * quickThaw)) {
							string waterType;
							if (terrainDef.defName == "IceST") { waterType = "WaterShallow"; }
							else if (terrainDef.defName == "IceS") { waterType = "IceST"; }
							else if (terrainDef.defName == "IceDT") { waterType = "WaterDeep"; }
							else { waterType = "IceDT"; }
							if (waterType.Contains("Water")) {
								map.snowGrid.SetDepth(c, 0f);
							}
							map.terrainGrid.SetTerrain(c, TerrainDef.Named(waterType));
						}
					}
					if (terrainDef.defName.Contains("IceMarsh")) {
						int quickThaw = 1;
						for (int k = -1; k < 2; k++) {
							for (int j = -1; j < 2; j++) {
								int x = c.x + k;
								int z = c.z + j;
								if ((k == 0 && j == 1) || (k == 0 && j == -1) || (k == 1 && j == 0) || (k == -1 && j == 0)) {
									if (x > 0 && x < this.map.Size.x && z > 0 && z < this.map.Size.z) {
										IntVec3 newSpot = new IntVec3(x, 0, z);
										if (!terrainDef.label.Contains("Thin")) {
											if (map.terrainGrid.TerrainAt(newSpot).defName.Contains("Water") || map.terrainGrid.TerrainAt(newSpot).defName == "Marsh" || map.terrainGrid.TerrainAt(newSpot).label.Contains("Thin")) {
												quickThaw++;
											}
									    }
										else {
											if (map.terrainGrid.TerrainAt(newSpot).defName.Contains("Water") || map.terrainGrid.TerrainAt(newSpot).defName == "Marsh") {
												quickThaw++;
											}
										}
									}
								}
							}
						}
						if (quickThaw == 1) {
							if (Rand.Value < 0.67f) { quickThaw = 0; }
						}
						if (Rand.Value < (shallowChance * quickThaw)) {
							string waterType;
							if (terrainDef.defName == "IceMarsh") { waterType = "IceMarshT"; }
							else { 
								waterType = "Marsh";
								map.snowGrid.SetDepth(c, 0f);
							}
							map.terrainGrid.SetTerrain(c, TerrainDef.Named(waterType));
						}
					}
					if (terrainDef.defName == "Ice" && map.Biome.defName.Contains("Permafrost")) {
						int quickThaw = 0;
						for (int k = -1; k < 2; k++) {
							for (int j = -1; j < 2; j++) {
								int x = c.x + k;
								int z = c.z + j;
								if ((k == 0 && j == 1) || (k == 0 && j == -1) || (k == 1 && j == 0) || (k == -1 && j == 0)) {
									if (x > 0 && x < this.map.Size.x && z > 0 && z < this.map.Size.z) {
										IntVec3 newSpot = new IntVec3(x, 0, z);
										if (!map.terrainGrid.TerrainAt(newSpot).defName.Contains("Ice")) {
											quickThaw++;
										}
									}
								}
							}
						}
						if (Rand.Value < ((permafrostChance/4) * quickThaw)) {
							map.terrainGrid.SetTerrain(c, TerrainDef.Named("Gravel"));
						}
					}
				}
			}
		}
	}

	public class Rivers : MapComponent
	{
		public int cycleIndex;
		public Rivers(Map map) : base(map) { }
		public override void MapComponentTick()
		{
			base.MapComponentTick();
			if (Controller.Settings.waterFreezes.Equals(false)) { return; }
			int num = Mathf.RoundToInt((float)this.map.Area * 0.00005f);
			int area = this.map.Area;
			float cellTemp;
			for (int i = 0; i < num; i++)
			{
				this.cycleIndex++;
				if (this.cycleIndex >= area) { this.cycleIndex = 0; }
				IntVec3 c = this.map.cellsInRandomOrder.Get(this.cycleIndex);
				var terrainDef = map.terrainGrid.TerrainAt(c);
				if (!GenTemperature.TryGetTemperatureForCell(c, map, out cellTemp)) { cellTemp = 0f; }
				float shallowChance = (cellTemp) * (cellTemp) * 0.8f/ 100f;
				float deepChance = ((cellTemp) * (cellTemp) * 0.16f) / 100f;
				float permafrostChance = deepChance;
				if (permafrostChance > 0.2f) { permafrostChance = 0.2f; }
				if (cellTemp < -8f)
				{
					if (terrainDef.defName == "WaterMovingShallow" ||terrainDef.defName == "IceSMT")
					{
						int freezable = 0;
						for (int k = -1; k < 2; k++)
						{
							for (int j = -1; j < 2; j++)
							{
								int x = c.x + k;
								int z = c.z + j;
								if ((k == 0 && j == 1) || (k == 0 && j == -1) || (k == 1 && j == 0) || (k == -1 && j == 0))
								{
									if (x > 0 && x < this.map.Size.x && z > 0 && z < this.map.Size.z)
									{
										IntVec3 newSpot = new IntVec3(x, 0, z);
										if (!map.terrainGrid.TerrainAt(newSpot).defName.Contains("Water") && map.terrainGrid.TerrainAt(newSpot).defName != "Marsh" && map.terrainGrid.TerrainAt(newSpot).defName != "BridgeMarsh" && map.terrainGrid.TerrainAt(newSpot).defName != "Bridge")
										{
											freezable++;
										}
									}
								}
							}
						}
						if (Rand.Value < shallowChance * freezable)
						{
							List<Thing> thingList = c.GetThingList(map);
							for (int j = 0; j < thingList.Count; j++)
							{
								Thing item = thingList[j];
								if (item.def.defName == "Fishing Spot" || item.def.defName == "ZARS_FishingSpot")
								{
									item.Destroy(DestroyMode.Vanish);
								}
							}
							string iceType;
							if (terrainDef.defName == "WaterMovingShallow") { iceType = "IceSMT"; }
							else { iceType = "IceSM"; }
							map.terrainGrid.SetTerrain(c, TerrainDef.Named(iceType));
						}
					}
					if (terrainDef.defName == "WaterMovingChestDeep" || terrainDef.defName == "IceDMT")
					{
						int freezable = 0;
						for (int k = -1; k < 2; k++)
						{
							for (int j = -1; j < 2; j++)
							{
								int x = c.x + k;
								int z = c.z + j;
								if ((k == 0 && j == 1) || (k == 0 && j == -1) || (k == 1 && j == 0) || (k == -1 && j == 0))
								{
									if (x > 0 && x < this.map.Size.x && z > 0 && z < this.map.Size.z)
									{
										IntVec3 newSpot = new IntVec3(x, 0, z);
										if (!map.terrainGrid.TerrainAt(newSpot).defName.Contains("Water") && map.terrainGrid.TerrainAt(newSpot).defName != "Marsh" && map.terrainGrid.TerrainAt(newSpot).defName != "BridgeMarsh" && map.terrainGrid.TerrainAt(newSpot).defName != "Bridge")
										{
											freezable++;
										}
									}
								}
							}
						}
						if (Rand.Value < deepChance * freezable)
						{
							List<Thing> thingList = c.GetThingList(map);
							for (int j = 0; j < thingList.Count; j++)
							{
								Thing item = thingList[j];
								if (item.def.defName == "Fishing Spot" || item.def.defName == "ZARS_FishingSpot")
								{
									item.Destroy(DestroyMode.Vanish);
								}
							}
							string iceType;
							if (terrainDef.defName == "WaterMovingChestDeep") { iceType = "IceDMT"; }
							else { iceType = "IceDM"; }
							map.terrainGrid.SetTerrain(c, TerrainDef.Named(iceType));
						}
					}
				}
				if (cellTemp > -20f)
				{
					if (terrainDef.defName.Contains("IceSM") || terrainDef.defName.Contains("IceDM"))
					{
						int quickThaw = 1;
						for (int k = -1; k < 2; k++)
						{
							for (int j = -1; j < 2; j++)
							{
								int x = c.x + k;
								int z = c.z + j;
								if ((k == 0 && j == 1) || (k == 0 && j == -1) || (k == 1 && j == 0) || (k == -1 && j == 0))
								{
									if (x > 0 && x < this.map.Size.x && z > 0 && z < this.map.Size.z)
									{
										IntVec3 newSpot = new IntVec3(x, 0, z);
										if (!terrainDef.label.Contains("Thin"))
										{
											if (map.terrainGrid.TerrainAt(newSpot).defName.Contains("Water") || map.terrainGrid.TerrainAt(newSpot).defName == "Marsh" || map.terrainGrid.TerrainAt(newSpot).label.Contains("Thin"))
											{
												quickThaw++;
											}
										}
										else
										{
											if (map.terrainGrid.TerrainAt(newSpot).defName.Contains("Water") || map.terrainGrid.TerrainAt(newSpot).defName == "Marsh")
											{
												quickThaw++;
											}
										}
									}
								}
							}
						}
						if (quickThaw == 1)
						{
							if (Rand.Value < 0.67f) { quickThaw = 0; }
						}
						if (Rand.Value < (shallowChance * quickThaw))
						{
							string waterType;
							if (terrainDef.defName == "IceSMT") { waterType = "WaterMovingShallow"; }
							else if (terrainDef.defName == "IceSM") { waterType = "IceSMT"; }
							else if (terrainDef.defName == "IceDMT") { waterType = "WaterMovingChestDeep"; }
							else { waterType = "IceDMT"; }
							if (waterType.Contains("Water"))
							{
								map.snowGrid.SetDepth(c, 0f);
							}
							map.terrainGrid.SetTerrain(c, TerrainDef.Named(waterType));
						}
					}
				}
			}
		}
	}



	public class BiomeWorker_Permafrost : BiomeWorker {
        public BiomeWorker_Permafrost() { }
        public override float GetScore(Tile tile, int tileID) {
            if (tile.WaterCovered) {
                return -100f;
            }
            if (tile.temperature < -18f && tile.temperature > -24f) {
                return 100f;
            }
            return 0f;
        }
    }

}
