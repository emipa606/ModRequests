using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using Verse.Sound;
using RimWorld;

namespace HLVRMonsters
{
	[StaticConstructorOnStartup]
	public class AntlionTunnelHiveSpawner : ThingWithComps
	{
		// Token: 0x0600515C RID: 20828 RVA: 0x001B6DD4 File Offset: 0x001B4FD4
		public static void ResetStaticData()
		{
			AntlionTunnelHiveSpawner.filthTypes.Clear();
			AntlionTunnelHiveSpawner.filthTypes.Add(ThingDefOf.Filth_Dirt);
			AntlionTunnelHiveSpawner.filthTypes.Add(ThingDefOf.Filth_Dirt);
			AntlionTunnelHiveSpawner.filthTypes.Add(ThingDefOf.Filth_Dirt);
			AntlionTunnelHiveSpawner.filthTypes.Add(ThingDefOf.Filth_RubbleRock);
		}

		// Token: 0x0600515D RID: 20829 RVA: 0x001B6E28 File Offset: 0x001B5028
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<int>(ref this.secondarySpawnTick, "secondarySpawnTick", 0, false);
			Scribe_Values.Look<bool>(ref this.spawnHive, "spawnHive", true, false);
			Scribe_Values.Look<float>(ref this.insectsPoints, "insectsPoints", 0f, false);
			Scribe_Values.Look<bool>(ref this.spawnedByInfestationThingComp, "spawnedByInfestationThingComp", false, false);
		}

		// Token: 0x0600515E RID: 20830 RVA: 0x001B6E88 File Offset: 0x001B5088
		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			if (!respawningAfterLoad)
			{
				this.secondarySpawnTick = Find.TickManager.TicksGame + this.ResultSpawnDelay.RandomInRange.SecondsToTicks();
			}
			this.CreateSustainer();
		}

		// Token: 0x0600515F RID: 20831 RVA: 0x001B6ECC File Offset: 0x001B50CC
		public override void Tick()
		{
			if (!base.Spawned)
			{
				return;
			}
			sustainer.Maintain();
			Vector3 vector = base.Position.ToVector3Shifted();
			//if (Rand.MTBEventOccurs(FilthSpawnMTB, 1f, 1.TicksToSeconds()) && CellFinder.TryFindRandomReachableCellNear(base.Position, base.Map, FilthSpawnRadius, TraverseParms.For(TraverseMode.NoPassClosedDoors), null, null, out IntVec3 result))
			//{
			//	FilthMaker.TryMakeFilth(result, base.Map, filthTypes.RandomElement());
			//}
			if (Rand.MTBEventOccurs(DustMoteSpawnMTB, 1f, 1.TicksToSeconds()))
			{
				Vector3 loc = new Vector3(vector.x, 0f, vector.z);
				loc.y = AltitudeLayer.MoteOverhead.AltitudeFor();
				FleckMaker.ThrowDustPuffThick(loc, base.Map, Rand.Range(1.5f, 3f), new Color(1f, 1f, 1f, 2.5f));
			}
			if (secondarySpawnTick > Find.TickManager.TicksGame)
			{
				return;
			}
			sustainer.End();
			Map map = base.Map;
			IntVec3 position = base.Position;
			Destroy();
			if (spawnHive)
			{
				Hive obj = (Hive)GenSpawn.Spawn(ThingMaker.MakeThing(HLVRDefOf.AntlionHive), position, map);
				obj.SetFaction(Faction.OfInsects);
				obj.questTags = questTags;
				foreach (CompSpawner comp in obj.GetComps<CompSpawner>())
				{
					if (comp.PropsSpawner.thingToSpawn == ThingDefOf.InsectJelly)
					{
						comp.TryDoSpawn();
						break;
					}
				}
			}
			if (!(insectsPoints > 0f))
			{
				return;
			}
			insectsPoints = Mathf.Max(insectsPoints, Hive.spawnablePawnKinds.Min((PawnKindDef x) => x.combatPower));
			float pointsLeft = insectsPoints;
			List<Pawn> list = new List<Pawn>();
			int num = 0;
			PawnKindDef result2;
			for (; pointsLeft > 0f; pointsLeft -= result2.combatPower)
			{
				num++;
				if (num > 1000)
				{
					Log.Error("Too many iterations.");
					break;
				}
				if (!Hive.spawnablePawnKinds.Where((PawnKindDef x) => x.combatPower <= pointsLeft).TryRandomElement(out result2))
				{
					break;
				}
				Pawn pawn = PawnGenerator.GeneratePawn(result2, Faction.OfInsects);
				GenSpawn.Spawn(pawn, CellFinder.RandomClosewalkCellNear(position, map, 2), map);
				pawn.mindState.spawnedByInfestationThingComp = spawnedByInfestationThingComp;
				list.Add(pawn);
			}
			if (list.Any())
			{
				LordMaker.MakeNewLord(Faction.OfInsects, new LordJob_AssaultColony(Faction.OfInsects, canKidnap: true, canTimeoutOrFlee: false), map, list);
			}
		}

		// Token: 0x06005160 RID: 20832 RVA: 0x001B71E4 File Offset: 0x001B53E4
		public override void Draw()
		{
			Rand.PushState();
			Rand.Seed = this.thingIDNumber;
			for (int i = 0; i < 6; i++)
			{
				this.DrawDustPart(Rand.Range(0f, 360f), Rand.Range(0.9f, 1.1f) * (float)Rand.Sign * 4f, Rand.Range(1f, 1.5f));
			}
			Rand.PopState();
		}

		// Token: 0x06005161 RID: 20833 RVA: 0x001B7254 File Offset: 0x001B5454
		private void DrawDustPart(float initialAngle, float speedMultiplier, float scale)
		{
			float num = (Find.TickManager.TicksGame - this.secondarySpawnTick).TicksToSeconds();
			Vector3 pos = base.Position.ToVector3ShiftedWithAltitude(AltitudeLayer.Filth);
			pos.y += 0.042857144f * Rand.Range(0f, 1f);
			Color value = new Color(0.470588237f, 0.384313732f, 0.3254902f, 0.7f);
			AntlionTunnelHiveSpawner.matPropertyBlock.SetColor(ShaderPropertyIDs.Color, value);
			Matrix4x4 matrix = Matrix4x4.TRS(pos, Quaternion.Euler(0f, initialAngle + speedMultiplier * num, 0f), Vector3.one * scale);
			Graphics.DrawMesh(MeshPool.plane10, matrix, AntlionTunnelHiveSpawner.TunnelMaterial, 0, null, 0, AntlionTunnelHiveSpawner.matPropertyBlock);
		}

		// Token: 0x06005162 RID: 20834 RVA: 0x001B7312 File Offset: 0x001B5512
		private void CreateSustainer()
		{
			LongEventHandler.ExecuteWhenFinished(delegate
			{
				SoundDef tunnel = SoundDefOf.Tunnel;
				this.sustainer = tunnel.TrySpawnSustainer(SoundInfo.InMap(this, MaintenanceType.PerTick));
			});
		}

		// Token: 0x04002DEB RID: 11755
		private int secondarySpawnTick;

		// Token: 0x04002DEC RID: 11756
		public bool spawnHive = true;

		// Token: 0x04002DED RID: 11757
		public float insectsPoints;

		// Token: 0x04002DEE RID: 11758
		public bool spawnedByInfestationThingComp;

		// Token: 0x04002DEF RID: 11759
		private Sustainer sustainer;

		// Token: 0x04002DF0 RID: 11760
		private static MaterialPropertyBlock matPropertyBlock = new MaterialPropertyBlock();

		// Token: 0x04002DF1 RID: 11761
		private readonly FloatRange ResultSpawnDelay = new FloatRange(26f, 30f);

		// Token: 0x04002DF2 RID: 11762
		[TweakValue("Gameplay", 0f, 1f)]
		private static float DustMoteSpawnMTB = 0.2f;

		// Token: 0x04002DF3 RID: 11763
		[TweakValue("Gameplay", 0f, 1f)]
		private static float FilthSpawnMTB = 0.3f;

		// Token: 0x04002DF4 RID: 11764
		[TweakValue("Gameplay", 0f, 10f)]
		private static float FilthSpawnRadius = 3f;

		// Token: 0x04002DF5 RID: 11765
		private static readonly Material TunnelMaterial = MaterialPool.MatFrom("Things/Filth/Grainy/GrainyA", ShaderDatabase.Transparent);

		// Token: 0x04002DF6 RID: 11766
		private static List<ThingDef> filthTypes = new List<ThingDef>();
	}
}
