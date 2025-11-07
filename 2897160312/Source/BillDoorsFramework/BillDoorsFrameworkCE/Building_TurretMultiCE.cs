using CombatExtended;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace BillDoorsFramework
{
    public class Building_TurretMultiCE : Building_TurretGunCE
    {
        ModExtension_Building_TurretMulti Extension;
        CompRefuelable CompRefuelable;

        List<FuelPercentAndGraphicPair> Graphics = new List<FuelPercentAndGraphicPair>();

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            Extension = def.GetModExtension<ModExtension_Building_TurretMulti>();
            CompRefuelable = this.TryGetComp<CompRefuelable>();
            Graphics = Extension?.graphicDatas;
        }

        public override Material TurretTopMaterial
        {
            get
            {
                if (Extension != null)
                {
                    Material mat = null;
                    if (CompAmmo != null)
                    {
                        for (int i = 0; i < Graphics.Count; i++)
                        {
                            if (CompAmmo.CurMagCount <= Graphics[i].fuelPercent)
                            {
                                mat = MaterialPool.MatFrom(Graphics[i].graphicData.texPath);
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < Graphics.Count; i++)
                        {
                            if (CompRefuelable?.FuelPercentOfMax <= Graphics[i].fuelPercent)
                            {
                                mat = MaterialPool.MatFrom(Graphics[i].graphicData.texPath);
                            }
                        }
                    }
                    return mat ?? def.building.turretTopMat;
                }
                return this.def.building.turretTopMat;
            }
        }
    }

    public class Building_TurretNonSnapCE : Building_TurretMultiCE, IFireArc
    {
        public float curAngle;
        public float rotateSpeed => ext?.speed ?? 1f;

        public ModExt_NonSnapTurret ext => def.GetModExtension<ModExt_NonSnapTurret>();

        public CompFireArc CompFireArc => this.GetComp<CompFireArc>();

        public Vector3 turretOrientation => Vector3.forward.RotatedBy(curAngle);

        public float deltaAngle => currentTargetInt == null ? 0 : Vector3.SignedAngle(turretOrientation, (currentTargetInt.CenterVector3 - DrawPos).Yto0(), Vector3.up);

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref curAngle, "curAngle");
        }
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                curAngle = Rotation.AsAngle;
            }
        }

        public override void Tick()
        {
            if (Active && currentTargetInt != null)
            {
                if (burstWarmupTicksLeft == 1 && Mathf.Abs(deltaAngle) > rotateSpeed)
                {
                    burstWarmupTicksLeft++;
                }

                curAngle += (Mathf.Abs(deltaAngle) - rotateSpeed) > 0 ? Mathf.Sign(deltaAngle) * rotateSpeed : deltaAngle;
            }
            if (CurrentTarget != null && (!CompFireArc?.WithinFireArc(CurrentTarget) ?? false))
            {
                ResetForcedTarget();
                ResetCurrentTarget();
            }
            base.Tick();

            curAngle = Trim(curAngle);
        }

        protected float Trim(float angle)
        {
            if (angle > 360) angle -= 360;
            if (angle < 0) angle += 360;
            return angle;
        }

        public override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            top.CurRotation = curAngle;
            base.DrawAt(drawLoc, flip);
        }

        private IAttackTargetSearcher TargSearcher()
        {
            if (mannableComp != null && mannableComp.MannedNow)
            {
                return mannableComp.ManningPawn;
            }
            return this;
        }
        private bool IsValidTargetInner(Thing t)
        {
            Pawn pawn = t as Pawn;
            if (pawn != null)
            {
                if (this.mannableComp == null)
                {
                    return !GenAI.MachinesLike(base.Faction, pawn);
                }
                if (pawn.RaceProps.Animal && pawn.Faction == Faction.OfPlayer)
                {
                    return false;
                }
            }
            return true;
        }

        private bool IsValidTarget(Thing t)
        {
            return IsValidTargetInner(t) && (CompFireArc?.WithinFireArc(t) ?? true);
        }

        public override void OrderAttack(LocalTargetInfo targ)
        {
            if (CompFireArc?.WithinFireArc(targ) ?? true) base.OrderAttack(targ);
        }

        public override LocalTargetInfo TryFindNewTarget()
        {
            IAttackTargetSearcher attackTargetSearcher = TargSearcher();
            Faction faction = attackTargetSearcher.Thing.Faction;
            float range = AttackVerb.verbProps.range;
            if (Rand.Value < 0.5f && AttackVerb.ProjectileFliesOverhead() && faction.HostileTo(Faction.OfPlayer) && base.Map.listerBuildings.allBuildingsColonist.Where(delegate (Building x)
            {
                float num = AttackVerb.verbProps.EffectiveMinRange(x, this);
                float num2 = x.Position.DistanceToSquared(base.Position);
                return num2 > num * num && num2 < range * range;
            }).TryRandomElement(out var result))
            {
                return result;
            }
            TargetScanFlags targetScanFlags = TargetScanFlags.NeedThreat | TargetScanFlags.NeedAutoTargetable;
            if (!Projectile.projectile.flyOverhead)
            {
                targetScanFlags |= TargetScanFlags.NeedLOSToAll;
                targetScanFlags |= TargetScanFlags.LOSBlockableByGas;
            }
            else
            {
                targetScanFlags |= TargetScanFlags.NeedNotUnderThickRoof;
            }
            if (AttackVerb.IsIncendiary_Ranged())
            {
                targetScanFlags |= TargetScanFlags.NeedNonBurning;
            }
            return (Thing)AttackTargetFinderAngle.BestShootTargetFromCurrentPosition(attackTargetSearcher, targetScanFlags, turretOrientation, IsValidTarget);
        }

        public override void DrawExtraSelectionOverlays()
        {
            base.DrawExtraSelectionOverlays();
            if (this.AllComps != null)
            {
                for (int i = 0; i < this.AllComps.Count; i++)
                {
                    this.AllComps[i].PostDrawExtraSelectionOverlays();
                }
            }
        }
        public void PostAdjustFireArc()
        {
            currentTargetInt = LocalTargetInfo.Invalid;
            forcedTarget = LocalTargetInfo.Invalid;
            TryFindNewTarget();
        }
    }
}
