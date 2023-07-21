using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Noise;
using Verse.Sound;

namespace yayoPlanet
{
    // Token: 0x02000C65 RID: 3173
    [StaticConstructorOnStartup]
    public class yyTornado : ThingWithComps
    {
        int maxHp = 30;
        int _hp = 999;
        int hp
        {
            get
            {
                return _hp;
            }
            set
            {
                if(value > maxHp)
                {
                    _hp = maxHp;
                }else if(value < 0)
                {
                    _hp = 0;
                }
                else
                {
                    _hp = value;
                }
                
            }
        }

        int max_destroyHp = 1000;
        int destroyHp = 99999;

        private float FadeInOutFactor
        {
            get
            {
                float a = Mathf.Clamp01((float)(Find.TickManager.TicksGame - this.spawnTick) / 120f);
                float b = (this.leftFadeOutTicks < 0) ? 1f : Mathf.Min((float)this.leftFadeOutTicks / 120f, 1f);
                return Mathf.Min(a, b);
            }
        }

        
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<Vector2>(ref this.realPosition, "realPosition", default(Vector2), false);
            Scribe_Values.Look<float>(ref this.direction, "direction", 0f, false);
            Scribe_Values.Look<int>(ref this.spawnTick, "spawnTick", 0, false);
            Scribe_Values.Look<int>(ref this.leftFadeOutTicks, "leftFadeOutTicks", 0, false);
            Scribe_Values.Look<int>(ref this.ticksLeftToDisappear, "ticksLeftToDisappear", 0, false);
            Scribe_Values.Look<int>(ref this._hp, "_hp", maxHp, false);
            Scribe_Values.Look<int>(ref this.destroyHp, "destroyHp", max_destroyHp, false);
        }

        
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            hp = maxHp;
            destroyHp = max_destroyHp;
            base.SpawnSetup(map, respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                Vector3 vector = base.Position.ToVector3Shifted();
                this.realPosition = new Vector2(vector.x, vector.z);
                this.direction = Rand.Range(0f, 360f);
                this.spawnTick = Find.TickManager.TicksGame;
                this.leftFadeOutTicks = -1;
                this.ticksLeftToDisappear = new IntRange(50000, 70000).RandomInRange;
            }
            this.CreateSustainer();
        }

        // Token: 0x06004BE4 RID: 19428 RVA: 0x00194AE4 File Offset: 0x00192CE4
        public override void Tick()
        {
            hp++;
            if (base.Spawned)
            {
                if (this.sustainer == null)
                {
                    this.CreateSustainer();
                }
                this.sustainer.Maintain();
                this.UpdateSustainerVolume();
                base.GetComp<CompWindSource>().wind = 5f * this.FadeInOutFactor;
                if (this.leftFadeOutTicks > 0)
                {
                    this.leftFadeOutTicks--;
                    if (this.leftFadeOutTicks == 0)
                    {
                        this.Destroy(DestroyMode.Vanish);
                        return;
                    }
                }
                else
                {
                    if (directionNoise == null)
                    {
                        directionNoise = new Perlin(0.0020000000949949026, 2.0, 0.5, 4, 1948573612, QualityMode.Medium);
                    }
                    this.direction += (float)directionNoise.GetValue((double)Find.TickManager.TicksAbs, (double)((float)(this.thingIDNumber % 500) * 1000f), 0.0) * 0.78f;
                    this.realPosition = this.realPosition.Moved(this.direction, 0.0283333343f); // 이동속도
                    //this.realPosition = this.realPosition.Moved(this.direction, 0.0383333343f); // 이동속도
                    IntVec3 intVec = new Vector3(this.realPosition.x, 0f, this.realPosition.y).ToIntVec3();
                    
                    if (!intVec.CloseToEdge(base.Map, 5))
                    {
                        // 현재 셀이 이동불가 셀일경우 방향전환
                        if (intVec.Impassable(base.Map))
                        {
                            changeDirection(15);
                        }
                        /*
                        else if(intVec.InNoBuildEdgeArea(base.Map))// 인접한 셀이 이동불가 셀일경우 방향전환
                        {
                            changeDirection(10);
                        }
                        */

                        base.Position = intVec;
                        if (this.IsHashIntervalTick(120))
                        {
                            this.DamageCloseThings();
                        }
                        if (Rand.MTBEventOccurs(15f, 1f, 0.25f))
                        {
                            this.DamageFarThings();
                        }
                        /*
                        if (this.IsHashIntervalTick(20))
                        {
                            this.DestroyRoofs();
                        }
                        */
                        if (this.ticksLeftToDisappear > 0)
                        {
                            this.ticksLeftToDisappear--;
                            if (this.ticksLeftToDisappear == 0)
                            {
                                this.leftFadeOutTicks = 120;
                                //Messages.Message("MessageTornadoDissipated".Translate(), new TargetInfo(base.Position, base.Map, false), RimWorld.MessageTypeDefOf.PositiveEvent, true);
                            }
                        }
                        if (this.IsHashIntervalTick(4) && !this.CellImmuneToDamage(base.Position))
                        {
                            /*
                            float num = Rand.Range(0.6f, 1f);
                            RimWorld.MoteMaker.ThrowTornadoDustPuff(new Vector3(this.realPosition.x, 0f, this.realPosition.y)
                            {
                                y = AltitudeLayer.MoteOverhead.AltitudeFor()
                            } + Vector3Utility.RandomHorizontalOffset(1.5f), base.Map, Rand.Range(1.5f, 3f), new Color(num, num, num));
                            */
                            return;
                        }
                    }
                    else
                    {
                        // 맵의 외각일경우 중앙으로 향하기
                        Vector3 b = base.Map.Center.ToVector3();
                        direction = Vector2Utility.AngleTo(realPosition, new Vector2(b.x, b.z));
                        
                    }
                }
            }
        }

        public override void Draw()
        {
            Rand.PushState();
            Rand.Seed = this.thingIDNumber;

            int makeNum = 30;
            for (int i = 0; i < makeNum; i++)
            {
                // 높이
                this.DrawTornadoPart(PartsDistanceFromCenter.RandomInRange, Rand.Range(0f, 360f), Rand.Range(0.9f, 1.1f), Rand.Range(0.52f, 0.88f));
            }
            Rand.PopState();
        }

        private void DrawTornadoPart(float distanceFromCenter, float initialAngle, float speedMultiplier, float colorMultiplier)
        {
            int ticksGame = Find.TickManager.TicksGame;
            float num = 1f / distanceFromCenter;
            float num2 = 25f * speedMultiplier * num;
            float num3 = (initialAngle + (float)ticksGame * num2) % 360f;
            Vector2 vector = this.realPosition.Moved(num3, this.AdjustedDistanceFromCenter(distanceFromCenter));
            vector.y += distanceFromCenter * 1.5f; // 높이
            vector.y += ZOffsetBias;
            Vector3 a = new Vector3(vector.x, AltitudeLayer.Weather.AltitudeFor() + 0.0454545468f * Rand.Range(0f, 1f), vector.y);
            float num4 = distanceFromCenter * 1f + 18f; // 크기
            float num5 = 1f;
            if (num3 > 270f)
            {
                num5 = GenMath.LerpDouble(270f, 360f, 0f, 1f, num3);
            }
            else if (num3 > 180f)
            {
                num5 = GenMath.LerpDouble(180f, 270f, 1f, 0f, num3);
            }
            float num6 = Mathf.Min(distanceFromCenter / (PartsDistanceFromCenter.max + 2f), 1f);
            float d = Mathf.InverseLerp(0.18f, 0.4f, num6);
            Vector3 a2 = new Vector3(Mathf.Sin((float)ticksGame / 1000f + (float)(this.thingIDNumber * 10)) * 2f, 0f, 0f);
            Vector3 pos = a + a2 * d;
            float a3 = Mathf.Max(1f - num6, 0f) * num5 * this.FadeInOutFactor * 1f; // 투명도
            Color value = new Color(0.27f, 0.1f, 0.05f, a3); // 색깔
            matPropertyBlock.SetColor(ShaderPropertyIDs.Color, value);
            Matrix4x4 matrix = Matrix4x4.TRS(pos, Quaternion.Euler(0f, num3, 0f), new Vector3(num4, 1f, num4));
            Graphics.DrawMesh(MeshPool.plane10, matrix, TornadoMaterial, 0, null, 0, matPropertyBlock); // 메쉬
        }

        // Token: 0x06004BE7 RID: 19431 RVA: 0x00194FF0 File Offset: 0x001931F0
        private float AdjustedDistanceFromCenter(float distanceFromCenter)
        {
            float num = Mathf.Min(distanceFromCenter / 8f, 1f);
            num *= num;
            return distanceFromCenter * num;
        }

        private void UpdateSustainerVolume()
        {
            this.sustainer.info.volumeFactor = this.FadeInOutFactor * 0.4f; // 토네이도 볼륨
        }

        private void CreateSustainer()
        {
            LongEventHandler.ExecuteWhenFinished(delegate
            {
                SoundDef tornado = RimWorld.SoundDefOf.Tornado;
                this.sustainer = tornado.TrySpawnSustainer(SoundInfo.InMap(this, MaintenanceType.PerTick));
                this.UpdateSustainerVolume();
            });
        }

        private void DamageCloseThings()
        {
            float maxDist = 9f;
            int num = GenRadial.NumCellsInRadius(maxDist); // 공격범위
            for (int i = 0; i < num; i++)
            {
                IntVec3 intVec = base.Position + GenRadial.RadialPattern[i];
                
                if (intVec.InBounds(base.Map) && !this.CellImmuneToDamage(intVec))
                {
                    Pawn firstPawn = intVec.GetFirstPawn(base.Map);
                    if (firstPawn == null || !firstPawn.Downed || !Rand.Bool)
                    {
                        IntVec3 checkCell = intVec.RandomAdjacentCell8Way();
                        
                        Room room = checkCell.GetRoomOrAdjacent(base.Map, RegionType.Set_All);
                        if (checkCell.Walkable(base.Map) && (room == null || room.PsychologicallyOutdoors))
                        {
                            float dist = intVec.DistanceTo(base.Position);
                            float damageFactor = GenMath.LerpDouble(0f, 8.4f, maxDist, 0.4f, dist);

                            this.DoDamage(intVec, damageFactor);
                        }
                        
                    }
                }
            }
        }

        // Token: 0x06004BEB RID: 19435 RVA: 0x001950EC File Offset: 0x001932EC
        private void DamageFarThings()
        {
            IntVec3 c = (from x in GenRadial.RadialCellsAround(base.Position, 10f, true)
                         where x.InBounds(base.Map)
                         select x).RandomElement<IntVec3>();
            if (this.CellImmuneToDamage(c))
            {
                return;
            }
            this.DoDamage(c, 0.5f);
        }

        // Token: 0x06004BEC RID: 19436 RVA: 0x00195138 File Offset: 0x00193338
        private void DestroyRoofs()
        {
            this.removedRoofsTmp.Clear();
            foreach (IntVec3 intVec in from x in GenRadial.RadialCellsAround(base.Position, 4.2f, true)
                                       where x.InBounds(base.Map)
                                       select x)
            {
                if (!this.CellImmuneToDamage(intVec) && intVec.Roofed(base.Map))
                {
                    RoofDef roof = intVec.GetRoof(base.Map);
                    if (!roof.isThickRoof && !roof.isNatural)
                    {
                        RoofCollapserImmediate.DropRoofInCells(intVec, base.Map, null);
                        this.removedRoofsTmp.Add(intVec);
                    }
                }
            }
            if (this.removedRoofsTmp.Count > 0)
            {
                RoofCollapseCellsFinder.CheckCollapseFlyingRoofs(this.removedRoofsTmp, base.Map, true, false);
            }
        }

        // Token: 0x06004BED RID: 19437 RVA: 0x00195214 File Offset: 0x00193414
        private bool CellImmuneToDamage(IntVec3 c)
        {
            if (c.Roofed(base.Map) && c.GetRoof(base.Map).isThickRoof)
            {
                return true;
            }
            Building edifice = c.GetEdifice(base.Map);
            return edifice != null && edifice.def.category == ThingCategory.Building && (edifice.def.building.isNaturalRock || (edifice.def == RimWorld.ThingDefOf.Wall && edifice.Faction == null));
        }

        // Token: 0x06004BEE RID: 19438 RVA: 0x0019528C File Offset: 0x0019348C
        private void DoDamage(IntVec3 c, float damageFactor)
        {
            // 데미지 조절
            damageFactor *= 0.16f * modBase.val_yyTornado;

            tmpThings.Clear();
            
            tmpThings.AddRange(c.GetThingList(base.Map));
            Vector3 vector = c.ToVector3Shifted();
            Vector2 b = new Vector2(vector.x, vector.z);
            float angle = -this.realPosition.AngleTo(b) + 180f;
            bool playerThingIsNear = false;
            for (int i = 0; i < tmpThings.Count; i++)
            {
                

                Thing thing = tmpThings[i];

                

                BattleLogEntry_DamageTaken battleLogEntry_DamageTaken = null;
                switch (thing.def.category)
                {
                    case ThingCategory.Pawn:
                        {
                            Pawn pawn = (Pawn)tmpThings[i];
                            battleLogEntry_DamageTaken = new BattleLogEntry_DamageTaken(pawn, RimWorld.RulePackDefOf.DamageEvent_Tornado, null);
                            Find.BattleLog.Add(battleLogEntry_DamageTaken);
                            if (pawn.RaceProps.baseHealthScale < 1f)
                            {
                                damageFactor *= pawn.RaceProps.baseHealthScale * 4f;
                            }
                            if (pawn.RaceProps.Animal)
                            {
                                damageFactor *= 3f;
                            }
                            if (pawn.Downed)
                            {
                                damageFactor *= 0.8f;
                            }
                            break;
                        }
                    case ThingCategory.Item:
                        damageFactor *= 0.68f;
                        break;
                    case ThingCategory.Building:
                        damageFactor *= 0.1f;
                        destroyHp--;
                        if (destroyHp <= 0)
                        {
                            this.leftFadeOutTicks = 120;
                        }
                        break;
                    case ThingCategory.Plant:
                        damageFactor *= 0.025f;
                        break;
                }
                //if (thing.def.category == ThingCategory.Building) Log.Message($"--------------");
                int num = Mathf.Max(GenMath.RoundRandom(30f * damageFactor), 0);
                //if (thing.def.category == ThingCategory.Building) Log.Message($"a : {num}");
                if(thing.def.category == ThingCategory.Building)
                {
                    num = GenMath.RoundRandom(Mathf.Max(num, (float)thing.def.BaseMaxHitPoints * 0.02f * modBase.val_yyTornado)); // 체력에 따른 퍼뎀
                    //Log.Message($"b : {num}");
                }
                if (num > 0)
                {
                    if (Rand.Bool)
                    {
                        tmpThings[i].TakeDamage(new DamageInfo(RimWorld.DamageDefOf.Blunt, (float)num, 0f, angle, this, null, null, DamageInfo.SourceCategory.ThingOrUnknown, null)).AssociateWithLog(battleLogEntry_DamageTaken);
                    }
                    else
                    {
                        tmpThings[i].TakeDamage(new DamageInfo(RimWorld.DamageDefOf.Scratch, (float)num, 0f, angle, this, null, null, DamageInfo.SourceCategory.ThingOrUnknown, null)).AssociateWithLog(battleLogEntry_DamageTaken);
                    }
                }
                
                
                


            }
            tmpThings.Clear();
        }

        void changeDirection(int dmg)
        {
            // 체력감소에 따라 방향전환
            hp -= dmg;
            if (hp <= 0)
            {
                hp = maxHp;
                direction = Rand.Range(0f, 360f);
            }

            

        }

        // Token: 0x04002A6C RID: 10860
        private Vector2 realPosition;

        // Token: 0x04002A6D RID: 10861
        private float direction;

        // Token: 0x04002A6E RID: 10862
        private int spawnTick;

        // Token: 0x04002A6F RID: 10863
        private int leftFadeOutTicks = -1;

        // Token: 0x04002A70 RID: 10864
        private int ticksLeftToDisappear = -1;

        // Token: 0x04002A71 RID: 10865
        private Sustainer sustainer;

        // Token: 0x04002A72 RID: 10866
        private static MaterialPropertyBlock matPropertyBlock = new MaterialPropertyBlock();

        // Token: 0x04002A73 RID: 10867
        private static ModuleBase directionNoise;

        // Token: 0x04002A74 RID: 10868
        private const float Wind = 5f;

        // Token: 0x04002A75 RID: 10869
        private const int CloseDamageIntervalTicks = 15;

        // Token: 0x04002A76 RID: 10870
        private const int RoofDestructionIntervalTicks = 20;

        // Token: 0x04002A77 RID: 10871
        private const float FarDamageMTBTicks = 15f;

        // Token: 0x04002A78 RID: 10872
        private const float CloseDamageRadius = 4.2f;

        // Token: 0x04002A79 RID: 10873
        private const float FarDamageRadius = 10f;

        // Token: 0x04002A7A RID: 10874
        private const float BaseDamage = 30f;

        // Token: 0x04002A7B RID: 10875
        private const int SpawnMoteEveryTicks = 4;

        // Token: 0x04002A7D RID: 10877
        private const float DownedPawnDamageFactor = 0.2f;

        // Token: 0x04002A7E RID: 10878
        private const float AnimalPawnDamageFactor = 0.75f;

        // Token: 0x04002A7F RID: 10879
        private const float BuildingDamageFactor = 0.8f;

        // Token: 0x04002A80 RID: 10880
        private const float PlantDamageFactor = 1.7f;

        // Token: 0x04002A81 RID: 10881
        private const float ItemDamageFactor = 0.68f;

        // Token: 0x04002A82 RID: 10882
        private const float CellsPerSecond = 1.7f;

        // Token: 0x04002A83 RID: 10883
        private const float DirectionChangeSpeed = 0.78f;

        // Token: 0x04002A84 RID: 10884
        private const float DirectionNoiseFrequency = 0.002f;

        // Token: 0x04002A85 RID: 10885
        private const float TornadoAnimationSpeed = 25f;

        // Token: 0x04002A86 RID: 10886
        private const float ThreeDimensionalEffectStrength = 4f;

        // Token: 0x04002A87 RID: 10887
        private const int FadeInTicks = 120;

        // Token: 0x04002A88 RID: 10888
        private const int FadeOutTicks = 120;

        // Token: 0x04002A89 RID: 10889
        private const float MaxMidOffset = 2f;

        // Token: 0x04002A8A RID: 10890
        private static readonly Material TornadoMaterial = MaterialPool.MatFrom("Things/Ethereal/Tornado", ShaderDatabase.Transparent, MapMaterialRenderQueues.Tornado);

        // Token: 0x04002A8B RID: 10891
        private static readonly FloatRange PartsDistanceFromCenter = new FloatRange(1f, 10f);

        // Token: 0x04002A8C RID: 10892
        private static readonly float ZOffsetBias = -4f * PartsDistanceFromCenter.min;

        // Token: 0x04002A8D RID: 10893
        private List<IntVec3> removedRoofsTmp = new List<IntVec3>();

        // Token: 0x04002A8E RID: 10894
        private static List<Thing> tmpThings = new List<Thing>();
    }
}
