using CombatExtended;
using CombatExtended.Compatibility;
using CombatExtended.Utilities;
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.TestTools;
using Verse;
using static Verse.SpecificApparelRequirement;

namespace BillDoorsFramework
{
    public class ProjectileGuidedBulletCE : BulletCE
    {
        public float Distance
        {
            get
            {
                return distance;
            }
        }

        public Vector3 Origin
        {
            get { return launcherPos; }
        }
        private Vector3 Direction
        {
            get
            {
                directionInt.x = TargetPos.x - launcherPos.x;
                directionInt.z = TargetPos.z - launcherPos.z;
                return directionInt.normalized;
            }
        }

        public Vector3 TargetPos
        {
            get
            {
                if (intendedTargetThing != null)
                {
                    targetPosInt.x = intendedTargetThing.TrueCenter().x;
                    targetPosInt.z = intendedTargetThing.TrueCenter().z;
                    return targetPosInt;
                }
                return Destination;
            }
        }

        private Vector3 GuidedDestination
        {
            get
            {
                return launcherPos + (Direction * distance);
            }
        }

        public float AimHeight
        {
            get
            {
                if (aimHeight < 0)
                {
                    aimHeight = getAimHeight();
                }
                return aimHeight;
            }
        }

        private float distance;
        private Vector3 targetPosInt;
        private Vector3 directionInt;
        private Vector3 launcherPos;
        private float aimHeight = -1;
        private QualityAffectedGuidance qualityMultiplier = new QualityAffectedGuidance();
        public bool guidanceOn = true;
        public int guidanceDelay;
        public bool proxFuse = false;
        Thing newTarget;
        bool shouldRedirect = false;
        float unaimedPenalty = 1;
        public ProjectileGuidedBulletCE()
        {
            targetPosInt = new Vector3();
            directionInt = new Vector3();
            this.launcherPos = new Vector3();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref newTarget, "newTarget");
            Scribe_Deep.Look(ref qualityMultiplier, "qualityMultiplier");
            Scribe_Values.Look(ref guidanceOn, "guidanceOn");
            Scribe_Values.Look(ref shouldRedirect, "shouldRedirect");
            Scribe_Values.Look(ref guidanceDelay, "guidanceDelay");
            Scribe_Values.Look(ref proxFuse, "proxFuse");
        }

        public override void Launch(Thing launcher, Vector2 origin, Thing equipment = null)
        {
            base.Launch(launcher, origin, equipment);
            this.launcherPos.x = origin.x;
            this.launcherPos.z = origin.y;
            double x = this.Destination.x - origin.x;
            double y = this.Destination.y - origin.y;
            distance = (float)Math.Sqrt(x * x + y * y);
            AccuracyFactor = 0.01f;
            if (equipment.def.HasModExtension<DefModExtensionGuidedBulletCE>())
            {
                DefModExtensionGuidedBulletCE Extension = equipment.def.GetModExtension<DefModExtensionGuidedBulletCE>();

                if (Extension.guidanceDelay > 0)
                {
                    guidanceDelay = Extension.guidanceDelay;
                    guidanceOn = false;
                }

                proxFuse = Extension.proxFuse;

                QualityCategory qc;
                if (equipment.TryGetQuality(out qc))
                {
                    foreach (QualityAffectedGuidance multiplier in Extension.qualityMultipliers)
                    {
                        if (multiplier.quality == qc)
                        {
                            qualityMultiplier = multiplier;
                            break;
                        }
                    }
                }
                else
                {
                    qualityMultiplier = Extension.qualityMultipliers.FirstOrDefault();
                }

                unaimedPenalty = Extension.unaimedPenalty;

                if (intendedTargetThing != null && intendedTargetThing.Faction != null && qualityMultiplier.guidanceRedirectChance > 0 && Rand.Chance(qualityMultiplier.guidanceRedirectChance * unaimedPenalty))
                {
                    IEnumerable<Thing> possibleTargets = launcherPos.ToIntVec3().PawnsNearSegment(intendedTarget.Cell, this.Map, Extension.redirectionRadius).Where(isValidTarget);
                    newTarget = intendedTargetThing;
                    if (possibleTargets.Any())
                    {
                        newTarget = possibleTargets.RandomElement();
                    }

                    shouldRedirect = true;
                }
                AccuracyFactor = qualityMultiplier.accuracyFactor * unaimedPenalty;
            }
        }

        bool isValidTarget(Thing thing)
        {
            if (thing.Faction == intendedTargetThing.Faction && !(thing is Pawn pawn && pawn.Downed))
            {
                return true;
            }
            return false;
        }

        public float getAimHeight()
        {
            CompFireModes CompFireModes = equipment.TryGetComp<CompFireModes>();
            if (intendedTargetThing == null)
            {
                return shotHeight;
            }
            var victimVert = new CollisionVertical(intendedTarget.Thing);
            var targetRange = victimVert.HeightRange;   //Get lower and upper heights of the target
            if (intendedTarget.Thing is Pawn Victim)
            {
                targetRange.min = victimVert.BottomHeight;
                targetRange.max = victimVert.MiddleHeight;

                if (CompFireModes?.targetMode == TargettingMode.head)
                {
                    // Aim upper thoraxic to head
                    targetRange.min = victimVert.MiddleHeight;
                    targetRange.max = victimVert.Max;
                }
                else if (CompFireModes?.targetMode == TargettingMode.legs)
                {
                    // Aim legs to lower abdomen
                    targetRange.min = victimVert.Min;
                    targetRange.max = victimVert.BottomHeight;
                }
                else
                {
                    // Aim center mass
                    targetRange.min = victimVert.BottomHeight;
                    targetRange.max = victimVert.MiddleHeight;
                }
            }

            return targetRange.Average;
        }


        public override void Tick()
        {
            if (proxFuse && intendedTarget != null)
            {
                Vector2 ExactPositionV2 = new Vector2(ExactPosition.x, ExactPosition.z);
                Vector2 IntendedTargetV2 = new Vector2(intendedTarget.Cell.x, intendedTarget.Cell.z);
                if (Vector2.Distance(ExactPositionV2, IntendedTargetV2) < Vector2.Distance(Vector2.Lerp(origin, Destination, (startingTicksToImpact - ticksToImpact + 1) / startingTicksToImpact), IntendedTargetV2))
                {
                    Impact(null);
                    return;
                }
            }
            base.Tick();
            guidanceDelay--;
            if (guidanceDelay < 0)
            {
                guidanceOn = true;
                guidanceDelay = 114514;
            }
            if (guidanceOn && intendedTargetThing != null)
            {
                this.Destination.x = GuidedDestination.x;
                this.Destination.y = GuidedDestination.z;

                var victimVert = new CollisionVertical(intendedTargetThing);
                if (Height > victimVert.Max && shotAngle > 0)
                {
                    shotAngle = 0;
                }

                if (qualityMultiplier.guidanceFailChance > 0 && Rand.Chance(qualityMultiplier.guidanceFailChance * unaimedPenalty))
                {
                    guidanceOn = false;
                    shotRotation += Rand.Value * 5 + 5;
                }

                if (shouldRedirect)
                {
                    intendedTarget = new LocalTargetInfo(newTarget);
                    shouldRedirect = false;
                }
            }
        }

    }

    public class DefModExtensionGuidedBulletCE : DefModExtension
    {
        public int guidanceDelay = 0;

        public float redirectionRadius = 5;

        public float unaimedPenalty = 2;

        public bool proxFuse = false;

        public List<QualityAffectedGuidance> qualityMultipliers;
    }

    public class QualityAffectedGuidance : IExposable
    {
        public QualityCategory quality;

        public float guidanceFailChance = 0;

        public float guidanceRedirectChance = 0;

        public float accuracyFactor = 0.01f;



        public QualityAffectedGuidance()
        {

        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref quality, "quality");
            Scribe_Values.Look(ref guidanceFailChance, "guidanceFailChance");
            Scribe_Values.Look(ref guidanceRedirectChance, "guidanceRedirectChance");
            Scribe_Values.Look(ref accuracyFactor, "accuracyFactor");
        }
    }
}
