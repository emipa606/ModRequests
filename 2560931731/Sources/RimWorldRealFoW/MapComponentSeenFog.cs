using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using RimWorldRealFoW;
using UnityEngine;
using Verse;
using RimWorldRealFoW.Utils;

namespace RimWorldRealFoW
{
    // Token: 0x02000004 RID: 4
    public class MapComponentSeenFog : MapComponent
    {
        // Token: 0x06000008 RID: 8 RVA: 0x000023F4 File Offset: 0x000005F4
        public MapComponentSeenFog(Map map) : base(map)
        {
            this.mapCellLength = map.cellIndices.NumGridCells;
            this.mapSizeX = map.Size.x;
            this.mapSizeZ = map.Size.z;


            this.mapDrawer = map.mapDrawer;

            this.fowWatchers = new List<CompFieldOfViewWatcher>(1000);

            this.maxFactionLoadId = 0;

            foreach (Faction faction in Find.World.factionManager.AllFactionsListForReading)
            {
                this.maxFactionLoadId = Math.Max(this.maxFactionLoadId, faction.loadID);
            }

            this.factionsShownCells = new short[this.maxFactionLoadId + 1][];
            this.factionsFullShow = new short[this.maxFactionLoadId + 1][];

            this.knownCells = new bool[this.mapCellLength];
            this.viewBlockerCells = new bool[this.mapCellLength];
            this.playerVisibilityChangeTick = new int[this.mapCellLength];
            this.mineDesignationGrid = new Designation[this.mapCellLength];
            this.idxToCellCache = new IntVec3[this.mapCellLength];
            this.compHideFromPlayerGrid = new List<CompHideFromPlayer>[this.mapCellLength];
            this.compHideFromPlayerGridCount = new byte[this.mapCellLength];
            this.compAffectVisionGrid = new List<CompAffectVision>[this.mapCellLength];
            for (int i = 0; i < this.mapCellLength; i++)
            {
                this.idxToCellCache[i] = CellIndicesUtility.IndexToCell(i, this.mapSizeX);

                this.compHideFromPlayerGrid[i] = new List<CompHideFromPlayer>(16);
                this.compHideFromPlayerGridCount[i] = 0;
                this.compAffectVisionGrid[i] = new List<CompAffectVision>(16);

                this.playerVisibilityChangeTick[i] = 0;
            }
        }

        public override void MapComponentTick()
        {
            this.currentGameTick = Find.TickManager.TicksGame;
            bool flag = !this.initialized;
            if (flag)
            {
                this.initialized = true;
                this.init();
            }
        }
        //New addition

        //Camera console
        //int memoryStorage = 0;

        public List<Building_CameraConsole> cameraConsoles = new List<Building_CameraConsole>();

        public void RegisterCameraConsole(Building_CameraConsole console)
        {
            cameraConsoles.Add(console);
        }
        public void DeregisterCameraConsole(Building_CameraConsole console)
        {
            cameraConsoles.Remove(console);
        }


        public bool workingCameraConsole
        {
            get
            {
                return
                (!RFOWSettings.needWatcher) ||
                (this.cameraConsoles.Any((Building_CameraConsole c) => c.WorkingNow && c.Manned));
            }
        }
        //Camera building
        public List<Building_SurveillanceCamera> surveillanceCameras = new List<Building_SurveillanceCamera>();

        public void RegisterSurveillanceCamera(Building_SurveillanceCamera camera)
        {
            surveillanceCameras.Add(camera);
        }

        public void DeregisterSurveillanceCamera(Building_SurveillanceCamera camera)
        {
            surveillanceCameras.Remove(camera);
        }

        public int SurveillanceCameraCount()
        {
            //Linq is cool but seem to have performance issure, a good old for loop seem beter
            //return surveillanceCameras.Count((Building_SurveillanceCamera x)=>x.isPowered());
            int count = 0;
            foreach (Building_SurveillanceCamera camera in surveillanceCameras)
            {
                if (camera.isPowered())
                    count++;
            }
            return count;
        }

        private short[] GetFactionFullShow(Faction faction)
        {
            if (this.factionsFullShow[faction.loadID] == null)
            {
                this.factionsFullShow[faction.loadID] = new short[this.mapCellLength];
                for (int i = 0; i < factionsFullShow[faction.loadID].Length; i++)
                {
                    factionsFullShow[faction.loadID][i] = 1;
                }
            }
            return factionsFullShow[faction.loadID];
        }
        public short[] GetFactionShownCells(Faction faction)
        {
            short[] result;
            if (faction == null)
            {
                result = null;
            }
            else
            {
                if (this.maxFactionLoadId < faction.loadID)
                {
                    this.maxFactionLoadId = faction.loadID + 1;
                    Array.Resize<short[]>(ref this.factionsShownCells, this.maxFactionLoadId + 1);
                }
                if (this.factionsShownCells[faction.loadID] == null)
                {
                    this.factionsShownCells[faction.loadID] = new short[this.mapCellLength];
                }
                if (this.map.Biome.defName == "OuterSpaceBiome")
                {
                    result = GetFactionFullShow(faction);
                }
                else
                {
                    result = this.factionsShownCells[faction.loadID];
                }
            }
            return result;
        }

        public bool isShown(Faction faction, IntVec3 cell)
        {
            return this.isShown(faction, cell.x, cell.z);
        }

        // Token: 0x0600000C RID: 12 RVA: 0x000026FC File Offset: 0x000008FC
        public bool isShown(Faction faction, int x, int z)
        {
            return this.GetFactionShownCells(faction)[z * this.mapSizeX + x] != 0;
        }

        // Token: 0x0600000D RID: 13 RVA: 0x00002724 File Offset: 0x00000924
        public void RegisterCompHideFromPlayerPosition(CompHideFromPlayer comp, int x, int z)
        {
            if (x >= 0 && z >= 0 && x < this.mapSizeX && z < this.mapSizeZ)
            {
                int idx = z * this.mapSizeX + x;
                compHideFromPlayerGrid[idx].Add(comp);
                compHideFromPlayerGridCount[idx]++;
            }
        }

        // Token: 0x0600000E RID: 14 RVA: 0x00002784 File Offset: 0x00000984
        public void DeregisterCompHideFromPlayerPosition(CompHideFromPlayer comp, int x, int z)
        {
            if (x >= 0 && z >= 0 && x < this.mapSizeX && z < this.mapSizeZ)
            {
                int idx = z * this.mapSizeX + x;
                compHideFromPlayerGrid[idx].Remove(comp);
                compHideFromPlayerGridCount[idx]--;
            }
        }

        public void RegisterCompAffectVisionPosition(CompAffectVision comp, int x, int z)
        {
            if (x >= 0 && z >= 0 && x < this.mapSizeX && z < this.mapSizeZ)
            {
                this.compAffectVisionGrid[z * this.mapSizeX + x].Add(comp);
            }
        }

        public void DeregisterCompAffectVisionPosition(CompAffectVision comp, int x, int z)
        {
            if (x >= 0 && z >= 0 && x < this.mapSizeX && z < this.mapSizeZ)
            {
                this.compAffectVisionGrid[z * this.mapSizeX + x].Remove(comp);
            }
        }

        public void RegisterMineDesignation(Designation des)
        {
            IntVec3 cell = des.target.Cell;
            this.mineDesignationGrid[cell.z * this.mapSizeX + cell.x] = des;
        }

        public void DeregisterMineDesignation(Designation des)
        {
            IntVec3 cell = des.target.Cell;
            this.mineDesignationGrid[cell.z * this.mapSizeX + cell.x] = null;
        }

        private void init()
        {
            Section[,] array = (Section[,])Traverse.Create(this.mapDrawer).Field("sections").GetValue();
            this.sectionsSizeX = array.GetLength(0);
            this.sectionsSizeY = array.GetLength(1);
            this.sections = new Section[this.sectionsSizeX * this.sectionsSizeY];
            for (int i = 0; i < this.sectionsSizeY; i++)
            {
                for (int j = 0; j < this.sectionsSizeX; j++)
                {
                    this.sections[i * this.sectionsSizeX + j] = array[j, i];
                }
            }
            List<Designation> allDesignations = this.map.designationManager.AllDesignations;
            for (int k = 0; k < allDesignations.Count; k++)
            {
                Designation designation = allDesignations[k];
                if (designation.def == DesignationDefOf.Mine && !designation.target.HasThing)
                {
                    this.RegisterMineDesignation(designation);
                }
            }
            if (this.map.IsPlayerHome && this.map.mapPawns.ColonistsSpawnedCount == 0)
            {
                IntVec3 playerStartSpot = MapGenerator.PlayerStartSpot;

                //int radius = Mathf.RoundToInt(DefDatabase<RealFoWModDefaultsDef>.GetNamed(RealFoWModDefaultsDef.DEFAULT_DEF_NAME, true).baseViewRange);
                int radius = RFOWSettings.baseViewRange;
                ShadowCaster.computeFieldOfViewWithShadowCasting(playerStartSpot.x, playerStartSpot.z, radius, this.viewBlockerCells, this.map.Size.x, this.map.Size.z, false, null, null, null, this.knownCells, 0, 0, this.mapSizeX, null, 0, 0, 0, 0, 0, byte.MaxValue, -1, -1);
                for (int l = 0; l < this.mapCellLength; l++)
                {
                    bool flag3 = this.knownCells[l];
                    if (flag3)
                    {
                        IntVec3 c = CellIndicesUtility.IndexToCell(l, this.mapSizeX);
                        foreach (Thing this2 in this.map.thingGrid.ThingsListAtFast(c))
                        {
                            CompMainComponent compMainComponent = (CompMainComponent)this2.TryGetComp(CompMainComponent.COMP_DEF);
                            bool flag4 = compMainComponent != null && compMainComponent.compHideFromPlayer != null;
                            if (flag4)
                            {
                                compMainComponent.compHideFromPlayer.forceSeen();
                            }
                        }
                    }
                }
            }
            foreach (Thing thing in this.map.listerThings.AllThings)
            {
                if (thing.Spawned)
                {
                    CompMainComponent compMainComponent2 = (CompMainComponent)thing.TryGetComp(CompMainComponent.COMP_DEF);
                    if (compMainComponent2 != null)
                    {
                        if (compMainComponent2.compComponentsPositionTracker != null)
                        {
                            compMainComponent2.compComponentsPositionTracker.updatePosition();
                        }
                        if (compMainComponent2.compFieldOfViewWatcher != null)
                        {
                            compMainComponent2.compFieldOfViewWatcher.updateFoV(false);
                        }
                        if (compMainComponent2.compHideFromPlayer != null)
                        {
                            compMainComponent2.compHideFromPlayer.updateVisibility(true, false);
                        }
                    }
                }
            }
            if (this.map.Biome.defName == "OuterSpaceBiome" || RFOWSettings.mapRevealAtStart)
            {
                for (int l = 0; l < this.mapCellLength; l++)
                {
                    this.knownCells[l] = true;
                }
            }
            this.mapDrawer.RegenerateEverythingNow();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            DataExposeUtility.BoolArray(ref this.knownCells, this.map.Size.x * this.map.Size.z, "revealedCells");
        }

        public void RevealCell(int idx)
        {
            if (!this.knownCells[idx])
            {
                ref IntVec3 ptr = ref this.idxToCellCache[idx];
                this.knownCells[idx] = true;
                Designation designation = this.mineDesignationGrid[idx];
                if (designation != null && ptr.GetFirstMineable(this.map) == null)
                {
                    designation.Delete();
                }
                if (this.initialized)
                {
                    this.setMapMeshDirtyFlag(idx);
                    this.map.fertilityGrid.Drawer.SetDirty();
                    this.map.roofGrid.Drawer.SetDirty();
                    this.map.terrainGrid.Drawer.SetDirty();
                }
                if (this.compHideFromPlayerGridCount[idx] > 0)
                {
                    List<CompHideFromPlayer> list = this.compHideFromPlayerGrid[idx];
                    int count = list.Count;
                    for (int i = 0; i < count; i++)
                    {
                        list[i].updateVisibility(true, false);
                    }
                }
            }
        }

        // Token: 0x06000016 RID: 22 RVA: 0x00002DB4 File Offset: 0x00000FB4
        public void IncrementSeen(Faction faction, short[] factionShownCells, int idx)
        {
            short num = (short)(factionShownCells[idx] + 1);


            factionShownCells[idx] = num;
            if (num == 1 && faction.def.isPlayer)
            {
                ref IntVec3 ptr = ref this.idxToCellCache[idx];
                if (!this.knownCells[idx])
                {
                    this.knownCells[idx] = true;
                    if (this.initialized)
                    {
                        this.map.fertilityGrid.Drawer.SetDirty();
                        this.map.roofGrid.Drawer.SetDirty();
                        this.map.terrainGrid.Drawer.SetDirty();
                    }
                    Designation designation = this.mineDesignationGrid[idx];
                    bool hideMineGrid = designation != null && ptr.GetFirstMineable(this.map) == null;
                    if (hideMineGrid)
                    {
                        designation.Delete();
                    }
                }
                if (this.initialized)
                {
                    this.setMapMeshDirtyFlag(idx);
                }
                if (this.compHideFromPlayerGridCount[idx] > 0)
                {
                    List<CompHideFromPlayer> list = this.compHideFromPlayerGrid[idx];
                    int count = list.Count;
                    for (int i = 0; i < count; i++)
                    {
                        list[i].updateVisibility(true, false);
                    }
                }
            }
        }

        // Token: 0x06000017 RID: 23 RVA: 0x00002F04 File Offset: 0x00001104
        public void decrementSeen(Faction faction, short[] factionShownCells, int idx)
        {
            short num = (short)(factionShownCells[idx] - 1);
            factionShownCells[idx] = num;
            bool flag = num == 0 && faction.def.isPlayer;
            if (flag)
            {
                this.playerVisibilityChangeTick[idx] = this.currentGameTick;
                bool flag2 = this.initialized;
                if (flag2)
                {
                    this.setMapMeshDirtyFlag(idx);
                }
                bool flag3 = this.compHideFromPlayerGridCount[idx] > 0;
                if (flag3)
                {
                    List<CompHideFromPlayer> list = this.compHideFromPlayerGrid[idx];
                    int count = list.Count;
                    for (int i = 0; i < count; i++)
                    {
                        list[i].updateVisibility(true, false);
                    }
                }
            }
        }

        // Token: 0x06000018 RID: 24 RVA: 0x00002FA8 File Offset: 0x000011A8
        private void setMapMeshDirtyFlag(int idx)
        {
            int num = idx % this.mapSizeX;
            int num2 = idx / this.mapSizeX;
            int num3 = num / 17;
            int num4 = num2 / 17;
            int num5 = Math.Max(0, num - 1);
            int num6 = Math.Min(num2 + 2, this.mapSizeZ);
            int num7 = Math.Min(num + 2, this.mapSizeX) - num5;
            for (int i = Math.Max(0, num2 - 1); i < num6; i++)
            {
                int num8 = i * this.mapSizeX + num5;
                for (int j = 0; j < num7; j++)
                {
                    this.playerVisibilityChangeTick[num8 + j] = this.currentGameTick;
                }
            }
            this.sections[num4 * this.sectionsSizeX + num3].dirtyFlags |= SectionLayer_FoVLayer.mapMeshFlag;
            int num9 = num % 17;
            int num10 = num2 % 17;
            bool flag = num9 == 0;
            if (flag)
            {
                bool flag2 = num3 != 0;
                if (flag2)
                {
                    this.sections[num4 * this.sectionsSizeX + num3].dirtyFlags |= SectionLayer_FoVLayer.mapMeshFlag;
                    bool flag3 = num10 == 0;
                    if (flag3)
                    {
                        bool flag4 = num4 != 0;
                        if (flag4)
                        {
                            this.sections[(num4 - 1) * this.sectionsSizeX + (num3 - 1)].dirtyFlags |= SectionLayer_FoVLayer.mapMeshFlag;
                        }
                    }
                    else
                    {
                        bool flag5 = num10 == 16;
                        if (flag5)
                        {
                            bool flag6 = num4 < this.sectionsSizeY;
                            if (flag6)
                            {
                                this.sections[(num4 + 1) * this.sectionsSizeX + (num3 - 1)].dirtyFlags |= SectionLayer_FoVLayer.mapMeshFlag;
                            }
                        }
                    }
                }
            }
            else
            {
                bool flag7 = num9 == 16;
                if (flag7)
                {
                    bool flag8 = num3 < this.sectionsSizeX;
                    if (flag8)
                    {
                        this.sections[num4 * this.sectionsSizeX + (num3 + 1)].dirtyFlags |= SectionLayer_FoVLayer.mapMeshFlag;
                        bool flag9 = num10 == 0;
                        if (flag9)
                        {
                            bool flag10 = num4 != 0;
                            if (flag10)
                            {
                                this.sections[(num4 - 1) * this.sectionsSizeX + (num3 + 1)].dirtyFlags |= SectionLayer_FoVLayer.mapMeshFlag;
                            }
                        }
                        else
                        {
                            bool flag11 = num10 == 16;
                            if (flag11)
                            {
                                bool flag12 = num4 < this.sectionsSizeY;
                                if (flag12)
                                {
                                    this.sections[(num4 + 1) * this.sectionsSizeX + (num3 + 1)].dirtyFlags |= SectionLayer_FoVLayer.mapMeshFlag;
                                }
                            }
                        }
                    }
                }
            }
            bool flag13 = num10 == 0;
            if (flag13)
            {
                bool flag14 = num4 != 0;
                if (flag14)
                {
                    this.sections[(num4 - 1) * this.sectionsSizeX + num3].dirtyFlags |= SectionLayer_FoVLayer.mapMeshFlag;
                }
            }
            else
            {
                bool flag15 = num10 == 16;
                if (flag15)
                {
                    bool flag16 = num4 < this.sectionsSizeY;
                    if (flag16)
                    {
                        this.sections[(num4 + 1) * this.sectionsSizeX + num3].dirtyFlags |= SectionLayer_FoVLayer.mapMeshFlag;
                    }
                }
            }
        }

        public short[][] factionsShownCells = null;

        public short[][] factionsFullShow = null;

        public bool[] knownCells = null;
        public int[] playerVisibilityChangeTick = null;
        public bool[] viewBlockerCells = null;
        private IntVec3[] idxToCellCache;
        private List<CompHideFromPlayer>[] compHideFromPlayerGrid;
        private byte[] compHideFromPlayerGridCount;
        public List<CompAffectVision>[] compAffectVisionGrid;
        private Designation[] mineDesignationGrid;
        private int maxFactionLoadId;
        private int mapCellLength;
        public int mapSizeX;
        public int mapSizeZ;
        private MapDrawer mapDrawer;
        public bool initialized = false;
        public List<CompFieldOfViewWatcher> fowWatchers;
        private Section[] sections = null;
        private int sectionsSizeX;
        private int sectionsSizeY;
        private int currentGameTick = 0;
    }
}
