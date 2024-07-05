using System;
using System.Collections.Generic;
using RimWorld;
using RimWorldRealFoW;
using UnityEngine;
using Verse;

namespace RimWorldRealFoW
{
	// Token: 0x02000007 RID: 7
	public class SectionLayer_FoVLayer : SectionLayer
	{
		// Token: 0x06000027 RID: 39 RVA: 0x000042CC File Offset: 0x000024CC
		static SectionLayer_FoVLayer()
		{
			bool flag = SectionLayer_FoVLayer.mapMeshFlag == MapMeshFlag.None;
			if (flag)
			{
				List<MapMeshFlag> allFlags = MapMeshFlagUtility.allFlags;
				MapMeshFlag mapMeshFlag = MapMeshFlag.None;
				foreach (MapMeshFlag mapMeshFlag2 in allFlags)
				{
					bool flag2 = mapMeshFlag2 > mapMeshFlag;
					if (flag2)
					{
						mapMeshFlag = mapMeshFlag2;
					}
				}
				SectionLayer_FoVLayer.mapMeshFlag = (MapMeshFlag) ((int)(mapMeshFlag) << 1);
				allFlags.Add(SectionLayer_FoVLayer.mapMeshFlag);
				Log.Message("Injected new mapMeshFlag: " + SectionLayer_FoVLayer.mapMeshFlag);
			}
		}

		// Token: 0x06000028 RID: 40 RVA: 0x00004388 File Offset: 0x00002588
		public SectionLayer_FoVLayer(Section section) : base(section)
		{
			this.relevantChangeTypes = (SectionLayer_FoVLayer.mapMeshFlag | MapMeshFlag.FogOfWar);
		}

		// Token: 0x17000001 RID: 1
		// (get) Token: 0x06000029 RID: 41 RVA: 0x000043F8 File Offset: 0x000025F8
		public override bool Visible
		{
			get
			{
				return DebugViewSettings.drawFog;
			}
		}

		// Token: 0x0600002A RID: 42 RVA: 0x00004410 File Offset: 0x00002610
		public static void MakeBaseGeometry(Section section, LayerSubMesh sm, AltitudeLayer altitudeLayer)
		{
			CellRect cellRect = new CellRect(section.botLeft.x, section.botLeft.z, 17, 17);
			cellRect.ClipInsideMap(section.map);
			float y = altitudeLayer.AltitudeFor();
			sm.verts.Capacity = cellRect.Area * 9;
			for (int i = cellRect.minX; i <= cellRect.maxX; i++)
			{
				for (int j = cellRect.minZ; j <= cellRect.maxZ; j++)
				{
					sm.verts.Add(new Vector3((float)i, y, (float)j));
					sm.verts.Add(new Vector3((float)i, y, (float)j + 0.5f));
					sm.verts.Add(new Vector3((float)i, y, (float)(j + 1)));
					sm.verts.Add(new Vector3((float)i + 0.5f, y, (float)(j + 1)));
					sm.verts.Add(new Vector3((float)(i + 1), y, (float)(j + 1)));
					sm.verts.Add(new Vector3((float)(i + 1), y, (float)j + 0.5f));
					sm.verts.Add(new Vector3((float)(i + 1), y, (float)j));
					sm.verts.Add(new Vector3((float)i + 0.5f, y, (float)j));
					sm.verts.Add(new Vector3((float)i + 0.5f, y, (float)j + 0.5f));
				}
			}
			int num = cellRect.Area * 8 * 3;
			sm.tris.Capacity = num;
			int num2 = 0;
			while (sm.tris.Count < num)
			{
				sm.tris.Add(num2 + 7);
				sm.tris.Add(num2);
				sm.tris.Add(num2 + 1);
				sm.tris.Add(num2 + 1);
				sm.tris.Add(num2 + 2);
				sm.tris.Add(num2 + 3);
				sm.tris.Add(num2 + 3);
				sm.tris.Add(num2 + 4);
				sm.tris.Add(num2 + 5);
				sm.tris.Add(num2 + 5);
				sm.tris.Add(num2 + 6);
				sm.tris.Add(num2 + 7);
				sm.tris.Add(num2 + 7);
				sm.tris.Add(num2 + 1);
				sm.tris.Add(num2 + 8);
				sm.tris.Add(num2 + 1);
				sm.tris.Add(num2 + 3);
				sm.tris.Add(num2 + 8);
				sm.tris.Add(num2 + 3);
				sm.tris.Add(num2 + 5);
				sm.tris.Add(num2 + 8);
				sm.tris.Add(num2 + 5);
				sm.tris.Add(num2 + 7);
				sm.tris.Add(num2 + 8);
				num2 += 9;
			}
			sm.FinalizeMesh(MeshParts.Verts | MeshParts.Tris);
		}

		// Token: 0x0600002B RID: 43 RVA: 0x00004774 File Offset: 0x00002974
		public override void Regenerate()
		{
			bool flag = Current.ProgramState != ProgramState.Playing;
			if (!flag)
			{
				bool flag2 = this.pawnFog == null;
				if (flag2)
				{
					this.pawnFog = base.Map.getMapComponentSeenFog();
				}
				bool flag3 = this.pawnFog != null && this.pawnFog.initialized;
				if (flag3)
				{
					LayerSubMesh subMesh = base.GetSubMesh(MatBases.FogOfWar);
					bool flag4 = subMesh.mesh.vertexCount == 0;
					bool flag5;
					if (flag4)
					{
						flag5 = true;
						subMesh.mesh.MarkDynamic();
						SectionLayer_FoVLayer.MakeBaseGeometry(this.section, subMesh, AltitudeLayer.FogOfWar);
						this.targetAlphas = new byte[subMesh.mesh.vertexCount];
						this.alphaChangeTick = new int[subMesh.mesh.vertexCount];
						this.meshColors = new Color32[subMesh.mesh.vertexCount];
					}
					else
					{
						flag5 = false;
					}
					int num = 0;
					bool[] fogGrid = base.Map.fogGrid.fogGrid;
					bool flag6 = this.factionShownGrid == null;
					if (flag6)
					{
						this.factionShownGrid = this.pawnFog.GetFactionShownCells(Faction.OfPlayer);
					}
					short[] array = this.factionShownGrid;
					int[] playerVisibilityChangeTick = this.pawnFog.playerVisibilityChangeTick;
					bool[] knownCells = this.pawnFog.knownCells;
					int x = base.Map.Size.x;
					CellRect cellRect = this.section.CellRect;
					int num2 = base.Map.Size.z - 1;
					int num3 = x - 1;
					bool flag7 = false;
					for (int i = cellRect.minX; i <= cellRect.maxX; i++)
					{
						for (int j = cellRect.minZ; j <= cellRect.maxZ; j++)
						{
							int num4 = j * x + i;
							int num5 = playerVisibilityChangeTick[num4];
							bool flag8 = !fogGrid[num4];
							if (flag8)
							{
								bool flag9 = array[num4] == 0;
								if (flag9)
								{
									bool flag10 = knownCells[num4];
									for (int k = 0; k < 9; k++)
									{
										this.vertsNotShown[k] = true;
										this.vertsSeen[k] = flag10;
									}
									bool flag11 = flag10;
									if (flag11)
									{
										int num6 = (j + 1) * x + i;
										int num7 = (j - 1) * x + i;
										int num8 = j * x + (i + 1);
										int num9 = j * x + (i - 1);
										int num10 = (j - 1) * x + (i - 1);
										int num11 = (j + 1) * x + (i - 1);
										int num12 = (j + 1) * x + (i + 1);
										int num13 = (j - 1) * x + (i + 1);
										bool flag12 = j < num2 && !knownCells[num6];
										if (flag12)
										{
											this.vertsSeen[2] = false;
											this.vertsSeen[3] = false;
											this.vertsSeen[4] = false;
										}
										bool flag13 = j > 0 && !knownCells[num7];
										if (flag13)
										{
											this.vertsSeen[6] = false;
											this.vertsSeen[7] = false;
											this.vertsSeen[0] = false;
										}
										bool flag14 = i < num3 && !knownCells[num8];
										if (flag14)
										{
											this.vertsSeen[4] = false;
											this.vertsSeen[5] = false;
											this.vertsSeen[6] = false;
										}
										bool flag15 = i > 0 && !knownCells[num9];
										if (flag15)
										{
											this.vertsSeen[0] = false;
											this.vertsSeen[1] = false;
											this.vertsSeen[2] = false;
										}
										bool flag16 = j > 0 && i > 0 && !knownCells[num10];
										if (flag16)
										{
											this.vertsSeen[0] = false;
										}
										bool flag17 = j < num2 && i > 0 && !knownCells[num11];
										if (flag17)
										{
											this.vertsSeen[2] = false;
										}
										bool flag18 = j < num2 && i < num3 && !knownCells[num12];
										if (flag18)
										{
											this.vertsSeen[4] = false;
										}
										bool flag19 = j > 0 && i < num3 && !knownCells[num13];
										if (flag19)
										{
											this.vertsSeen[6] = false;
										}
									}
								}
								else
								{
									for (int l = 0; l < 9; l++)
									{
										this.vertsNotShown[l] = false;
										this.vertsSeen[l] = false;
									}
									int num6 = (j + 1) * x + i;
									int num7 = (j - 1) * x + i;
									int num8 = j * x + (i + 1);
									int num9 = j * x + (i - 1);
									int num10 = (j - 1) * x + (i - 1);
									int num11 = (j + 1) * x + (i - 1);
									int num12 = (j + 1) * x + (i + 1);
									int num13 = (j - 1) * x + (i + 1);
									bool flag20 = j < num2 && array[num6] == 0;
									if (flag20)
									{
										bool flag21 = knownCells[num6];
										this.vertsNotShown[2] = true;
										this.vertsSeen[2] = flag21;
										this.vertsNotShown[3] = true;
										this.vertsSeen[3] = flag21;
										this.vertsNotShown[4] = true;
										this.vertsSeen[4] = flag21;
									}
									bool flag22 = j > 0 && array[num7] == 0;
									if (flag22)
									{
										bool flag21 = knownCells[num7];
										this.vertsNotShown[6] = true;
										this.vertsSeen[6] = flag21;
										this.vertsNotShown[7] = true;
										this.vertsSeen[7] = flag21;
										this.vertsNotShown[0] = true;
										this.vertsSeen[0] = flag21;
									}
									bool flag23 = i < num3 && array[num8] == 0;
									if (flag23)
									{
										bool flag21 = knownCells[num8];
										this.vertsNotShown[4] = true;
										this.vertsSeen[4] = flag21;
										this.vertsNotShown[5] = true;
										this.vertsSeen[5] = flag21;
										this.vertsNotShown[6] = true;
										this.vertsSeen[6] = flag21;
									}
									bool flag24 = i > 0 && array[num9] == 0;
									if (flag24)
									{
										bool flag21 = knownCells[num9];
										this.vertsNotShown[0] = true;
										this.vertsSeen[0] = flag21;
										this.vertsNotShown[1] = true;
										this.vertsSeen[1] = flag21;
										this.vertsNotShown[2] = true;
										this.vertsSeen[2] = flag21;
									}
									bool flag25 = j > 0 && i > 0 && array[num10] == 0;
									if (flag25)
									{
										bool flag21 = knownCells[num10];
										this.vertsNotShown[0] = true;
										this.vertsSeen[0] = flag21;
									}
									bool flag26 = j < num2 && i > 0 && array[num11] == 0;
									if (flag26)
									{
										bool flag21 = knownCells[num11];
										this.vertsNotShown[2] = true;
										this.vertsSeen[2] = flag21;
									}
									bool flag27 = j < num2 && i < num3 && array[num12] == 0;
									if (flag27)
									{
										bool flag21 = knownCells[num12];
										this.vertsNotShown[4] = true;
										this.vertsSeen[4] = flag21;
									}
									bool flag28 = j > 0 && i < num3 && array[num13] == 0;
									if (flag28)
									{
										bool flag21 = knownCells[num13];
										this.vertsNotShown[6] = true;
										this.vertsSeen[6] = flag21;
									}
								}
							}
							else
							{
								for (int m = 0; m < 9; m++)
								{
									this.vertsNotShown[m] = true;
									this.vertsSeen[m] = false;
								}
							}
							for (int n = 0; n < 9; n++)
							{
								bool flag29 = this.vertsNotShown[n];
								byte b;
								if (flag29)
								{
									bool flag30 = this.vertsSeen[n];
									if (flag30)
									{
										b = SectionLayer_FoVLayer.prefFogAlpha;
									}
									else
									{
										b = byte.MaxValue;
									}
									flag7 = true;
								}
								else
								{
									b = 0;
								}
								bool flag31 = !SectionLayer_FoVLayer.prefEnableFade || flag5;
								if (flag31)
								{
									bool flag32 = flag5 || this.meshColors[num].a != b;
									if (flag32)
									{
										this.meshColors[num] = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, b);
									}
									bool flag33 = SectionLayer_FoVLayer.prefEnableFade;
									if (flag33)
									{
										this.activeFogTransitions = true;
										this.targetAlphas[num] = b;
										this.alphaChangeTick[num] = num5;
									}
								}
								else
								{
									bool flag34 = this.targetAlphas[num] != b;
									if (flag34)
									{
										this.activeFogTransitions = true;
										this.targetAlphas[num] = b;
										this.alphaChangeTick[num] = num5;
									}
								}
								num++;
							}
						}
					}
					bool flag35 = !SectionLayer_FoVLayer.prefEnableFade || flag5;
					if (flag35)
					{
						bool flag36 = flag7;
						if (flag36)
						{
							subMesh.disabled = false;
							subMesh.mesh.colors32 = this.meshColors;
						}
						else
						{
							subMesh.disabled = true;
						}
					}
				}
			}
		}

		// Token: 0x0600002C RID: 44 RVA: 0x00005010 File Offset: 0x00003210
		public override void DrawLayer()
		{
			bool flag = SectionLayer_FoVLayer.prefEnableFade && this.Visible && this.activeFogTransitions;
			if (flag)
			{
				int ticksGame = Find.TickManager.TicksGame;
				int num = Math.Max((int)Find.TickManager.CurTimeSpeed, 1);
				bool flag2 = false;
				bool flag3 = false;
				Color32[] array = this.meshColors;
				for (int i = 0; i < this.targetAlphas.Length; i++)
				{
					byte b = this.targetAlphas[i];
					byte b2 = array[i].a;
					bool flag4 = b2 > b;
					if (flag4)
					{
						flag2 = true;
						bool flag5 = ticksGame != this.alphaChangeTick[i];
						if (flag5)
						{
							b2 = (byte)Math.Max((int)b, (int)b2 - SectionLayer_FoVLayer.prefFadeSpeedMult / num * (ticksGame - this.alphaChangeTick[i]));
							array[i] = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, b2);
							this.alphaChangeTick[i] = ticksGame;
						}
					}
					else
					{
						bool flag6 = b2 < b;
						if (flag6)
						{
							flag2 = true;
							bool flag7 = ticksGame != this.alphaChangeTick[i];
							if (flag7)
							{
								b2 = (byte)Math.Min((int)b, (int)b2 + SectionLayer_FoVLayer.prefFadeSpeedMult / num * (ticksGame - this.alphaChangeTick[i]));
								array[i] = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, b2);
								this.alphaChangeTick[i] = ticksGame;
							}
						}
					}
					bool flag8 = b2 > 0;
					if (flag8)
					{
						flag3 = true;
					}
				}
				bool flag9 = flag2;
				if (flag9)
				{
					LayerSubMesh subMesh = base.GetSubMesh(MatBases.FogOfWar);
					bool flag10 = flag3;
					if (flag10)
					{
						subMesh.disabled = false;
						subMesh.mesh.colors32 = array;
					}
					else
					{
						subMesh.disabled = true;
					}
				}
				else
				{
					this.activeFogTransitions = false;
				}
			}
			base.DrawLayer();
		}

		// Token: 0x0400001D RID: 29
		public static bool prefEnableFade = true;

		// Token: 0x0400001E RID: 30
		public static int prefFadeSpeedMult = 20;

		// Token: 0x0400001F RID: 31
		public static byte prefFogAlpha = 86;

		// Token: 0x04000020 RID: 32
		private MapComponentSeenFog pawnFog;

		// Token: 0x04000021 RID: 33
		private short[] factionShownGrid = null;

		// Token: 0x04000022 RID: 34
		public static MapMeshFlag mapMeshFlag = MapMeshFlag.None;

		// Token: 0x04000023 RID: 35
		private bool activeFogTransitions = false;

		// Token: 0x04000024 RID: 36
		private bool[] vertsNotShown = new bool[9];

		// Token: 0x04000025 RID: 37
		private bool[] vertsSeen = new bool[9];

		// Token: 0x04000026 RID: 38
		private byte[] targetAlphas = new byte[0];

		// Token: 0x04000027 RID: 39
		private int[] alphaChangeTick = new int[0];

		// Token: 0x04000028 RID: 40
		private Color32[] meshColors = new Color32[0];
	}
}
