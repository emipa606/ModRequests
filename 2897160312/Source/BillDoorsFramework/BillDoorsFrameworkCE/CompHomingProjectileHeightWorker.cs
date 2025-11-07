using CombatExtended;
using CombatExtended.Compatibility;
using ProjectileImpactFX;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace BillDoorsFramework
{
    public class CompHomingProjectileHeightWorker : ThingComp
    {
        CompProperties_HomingProjectileHeightWorker Props => props as CompProperties_HomingProjectileHeightWorker;

        int activatedPhase = -1;

        public HomingProjectilePhase ActivatedPhase => activatedPhase > -1 ? Props.PhaseList[Math.Min(activatedPhase, Props.PhaseList.Count - 1)] : null;

        public float GetTargetHeight(ProjectileCE_Homing projectile, LocalTargetInfo targetInfo)
        {
            CheckNextPhase(projectile, targetInfo);
            return ActivatedPhase?.HeightDelta(projectile, targetInfo) ?? -1;
        }

        void CheckNextPhase(ProjectileCE_Homing projectile, LocalTargetInfo targetInfo)
        {
            if (activatedPhase == Props.PhaseList.Count - 1)
            {
                return;
            }
            if (Props.PhaseList[activatedPhase + 1].ShouldInvoke(projectile, targetInfo))
            {
                activatedPhase++;
            }
        }

        public void Launch()
        {
            if (Props.PhaseList.Any())
            {
                activatedPhase = 0;
            }
        }
    }

    public class CompProperties_HomingProjectileHeightWorker : CompProperties
    {
        public List<HomingProjectilePhase> PhaseList = new List<HomingProjectilePhase>();

        public CompProperties_HomingProjectileHeightWorker()
        {
            compClass = typeof(CompHomingProjectileHeightWorker);
        }
    }
}
