using System.Collections.Generic;
using UnityEngine;
using RimWorld;
using Verse;
using Verse.Sound;

namespace BetterInfestations
{
    [StaticConstructorOnStartup]
    public class TunnelHiveSpawner : ThingWithComps
    {
        private int secondarySpawnTick;
        public bool spawnHive = true;
        public float insectsPoints;
        public bool spawnedByInfestationThingComp;
        private Sustainer sustainer;
        private static MaterialPropertyBlock matPropertyBlock = new MaterialPropertyBlock();
        private readonly FloatRange ResultSpawnDelay = new FloatRange(12f, 16f);
        [TweakValue("Gameplay", 0f, 1f)]
        private static float DustMoteSpawnMTB = 0.2f;
        [TweakValue("Gameplay", 0f, 1f)]
        private static float FilthSpawnMTB = 0.3f;
        [TweakValue("Gameplay", 0f, 10f)]
        private static float FilthSpawnRadius = 3f;
        private static readonly Material TunnelMaterial = MaterialPool.MatFrom("Things/Filth/Grainy/GrainyA", ShaderDatabase.Transparent);
        private static List<ThingDef> filthTypes = new List<ThingDef> { RimWorld.ThingDefOf.Filth_Dirt, RimWorld.ThingDefOf.Filth_Dirt, RimWorld.ThingDefOf.Filth_Dirt, RimWorld.ThingDefOf.Filth_RubbleRock };

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref secondarySpawnTick, "secondarySpawnTick", 0);
            Scribe_Values.Look(ref spawnHive, "spawnHive", defaultValue: true);
            Scribe_Values.Look(ref insectsPoints, "insectsPoints", 0f);
            Scribe_Values.Look(ref spawnedByInfestationThingComp, "spawnedByInfestationThingComp", defaultValue: false);
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                secondarySpawnTick = Find.TickManager.TicksGame + ResultSpawnDelay.RandomInRange.SecondsToTicks();
            }
            CreateSustainer();
        }

        public override void Tick()
        {
            if (!Spawned)
            {
                return;
            }
            sustainer.Maintain();
            Vector3 vector = Position.ToVector3Shifted();
            if (Rand.MTBEventOccurs(FilthSpawnMTB, 1f, 1.TicksToSeconds()) && CellFinder.TryFindRandomReachableCellNear(Position, Map, FilthSpawnRadius, TraverseParms.For(TraverseMode.NoPassClosedDoors), null, null, out IntVec3 result))
            {
                FilthMaker.TryMakeFilth(result, Map, filthTypes.RandomElement());
            }
            if (Rand.MTBEventOccurs(DustMoteSpawnMTB, 1f, 1.TicksToSeconds()))
            {
                Vector3 loc = new Vector3(vector.x, 0f, vector.z);
                loc.y = AltitudeLayer.MoteOverhead.AltitudeFor();
                FleckMaker.ThrowDustPuffThick(loc, Map, Rand.Range(1.5f, 3f), new Color(1f, 1f, 1f, 2.5f));
            }
            if (secondarySpawnTick > Find.TickManager.TicksGame)
            {
                return;
            }
            sustainer.End();
            Map map = Map;
            IntVec3 position = Position;
            Destroy();
            if (spawnHive)
            {
                Hive hive = (Hive)GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.BI_Hive), position, map);
                hive.SetFaction(Faction.OfInsects);
                hive.questTags = questTags;
                foreach (CompSpawner comp in hive.GetComps<CompSpawner>())
                {
                    if (comp.PropsSpawner.thingToSpawn == RimWorld.ThingDefOf.InsectJelly)
                    {
                        comp.TryDoSpawn();
                        break;
                    }
                }
            }
        }

        public override void Draw()
        {
            Rand.PushState();
            Rand.Seed = thingIDNumber;
            for (int i = 0; i < 6; i++)
            {
                DrawDustPart(Rand.Range(0f, 360f), Rand.Range(0.9f, 1.1f) * (float)Rand.Sign * 4f, Rand.Range(1f, 1.5f));
            }
            Rand.PopState();
        }

        private void DrawDustPart(float initialAngle, float speedMultiplier, float scale)
        {
            float num = (Find.TickManager.TicksGame - secondarySpawnTick).TicksToSeconds();
            Vector3 pos = Position.ToVector3ShiftedWithAltitude(AltitudeLayer.Filth);
            pos.y += 0.0454545468f * Rand.Range(0f, 1f);
            Color value = new Color(0.47f, 0.38f, 0.32f, 0.7f);
            matPropertyBlock.SetColor(ShaderPropertyIDs.Color, value);
            Matrix4x4 matrix = Matrix4x4.TRS(pos, Quaternion.Euler(0f, initialAngle + speedMultiplier * num, 0f), Vector3.one * scale);
            Graphics.DrawMesh(MeshPool.plane10, matrix, TunnelMaterial, 0, null, 0, matPropertyBlock);
        }

        private void CreateSustainer()
        {
            LongEventHandler.ExecuteWhenFinished(delegate
            {
                SoundDef tunnel = SoundDefOf.Tunnel;
                sustainer = tunnel.TrySpawnSustainer(SoundInfo.InMap(this, MaintenanceType.PerTick));
            });
        }
    }
}