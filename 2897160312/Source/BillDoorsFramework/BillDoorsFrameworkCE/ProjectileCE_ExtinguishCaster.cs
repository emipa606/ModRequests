using CombatExtended;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace BillDoorsFramework
{
    public class ProjectileCE_ExtinguishCaster : BulletCE
    {
        public override void Launch(Thing launcher, Vector2 origin, float shotAngle, float shotRotation, float shotHeight = 0, float shotSpeed = -1, Thing equipment = null, float distance = -1)
        {
            if (launcher.IsBurning())
            {
                Log.Message("fire");
                Thing fire = launcher.GetAttachment(ThingDefOf.Fire);
                fire?.Destroy();
            }
            if (launcher is Pawn pawn)
            {
                Hediff hediff = pawn.health.hediffSet.hediffs.Find(h => h.TryGetComp<HediffComp_Prometheum>() != null);
                if (hediff != null)
                {
                    Log.Message("prom");
                    pawn.health.hediffSet.hediffs.Remove(hediff);
                }
            }
            base.Launch(launcher, origin, shotAngle, shotRotation, shotHeight, shotSpeed, equipment, distance);
            Impact(null);
        }
    }
}
