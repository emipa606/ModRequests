using AbilitiesExtended;
using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace VFECore.Abilities
{
    // AbilitiesExtended.Verb_ShootEquipment
    public class Verb_ShockwaveWithShrapnel : Verb_ShootEquipment
    {
        // Token: 0x170006AA RID: 1706
        // (get) Token: 0x0600219B RID: 8603 RVA: 0x000CC040 File Offset: 0x000CA240

        /*bool RapidFire => verbProperties.RapidFire;
        float RapidFireRange => verbProperties.range / 2;
        bool BodyBurstSize => verbProperties.TyranidBurstBodySize;*/

        int PropjectilesPerShot => verbProperties.ScattershotCount + 1;

        protected override int ShotsPerBurst
        {
            get
            {
                return this.verbProps.burstShotCount;
            }
        }

        // Token: 0x0600219C RID: 8604 RVA: 0x000CC050 File Offset: 0x000CA250
        public override void WarmupComplete()
        {
            base.WarmupComplete();
            if (this.currentTarget.Thing is Pawn pawn && !pawn.Downed && this.CasterIsPawn && this.CasterPawn.skills != null)
            {
                float num = pawn.HostileTo(this.caster) ? 170f : 20f;
                float num2 = this.verbProps.AdjustedFullCycleTime(this, this.CasterPawn);
                this.CasterPawn.skills.Learn(SkillDefOf.Shooting, num * num2, false);
            }
        }
        
        
        // Token: 0x0600219D RID: 8605 RVA: 0x000CC0DB File Offset: 0x000CA2DB
        protected override bool TryCastShot()
        {

            bool flag = false;
            for (int i = 0; i < PropjectilesPerShot; i++)
            {
                if (base.TryCastShot())
                {
                    flag = true;
                }
            }
            if (flag && this.CasterIsPawn)
            {
                this.CasterPawn.records.Increment(RecordDefOf.ShotsFired);
            }
			List<IntVec3> AoECells = GenRadial.RadialCellsAround(this.CasterPawn.Position, 10f, true).ToList();
			float angle = GetAngle(this.CasterPawn.Position, this.currentTarget.Cell);
			List<IntVec3> ConeCells = new List<IntVec3>();
			foreach (IntVec3 c in AoECells)
			{
				if (Math.Abs(GetAngle(this.CasterPawn.Position, c) - angle) < 15f)	//+-15° (in both directions because of Abs) = 30° cone
				{
					ConeCells.Add(c);
				}
			}
			foreach (IntVec3 c in ConeCells)
			{
				Projectile newProjectile = (Projectile)GenSpawn.Spawn(WP_DefOf.WP_ProjectileTectonicShockwave, this.CasterPawn.Position, this.CasterPawn.Map, WipeMode.Vanish);
				if (newProjectile != null && newProjectile is Projectile)
				{
					newProjectile.Launch(this.CasterPawn, c, c, ProjectileHitFlags.All, false, null);
				}
			}
			
			//shrapnel part below
			            
			for (int i = 0; i < 21; i++)
            {
				IntVec3 randomTarget = this.currentTarget.Cell;
				randomTarget.x += Mathf.RoundToInt(Rand.Range(-2f, 2f));
				randomTarget.z += Mathf.RoundToInt(Rand.Range(-2f, 2f));
			
				Projectile newProjectile = (Projectile)GenSpawn.Spawn(WP_DefOf.WP_ProjectileTectonicShrapnel, this.CasterPawn.Position, this.CasterPawn.Map, WipeMode.Vanish);
				//this.def.GetModExtension<AbilityExtension_Projectile>().projectile
				if (newProjectile != null && newProjectile is Projectile)
				{
					newProjectile.Launch(this.CasterPawn, randomTarget, randomTarget, ProjectileHitFlags.All, false, null);
				}
			
			}
            return flag;
        }
		
		public float GetAngle(IntVec3 cell1, IntVec3 cell2)
		{
			float x1 = (float) cell1.x;
			float x2 = (float) cell2.x;
			float z1 = (float) cell1.z;
			float z2 = (float) cell2.z;
			float result = (Convert.ToSingle(Math.Atan2(x1 - x2, z1 - z2)) * 180 / Convert.ToSingle(Math.PI)) % 360; //arctan of the coordinate difference is the angle in rad, convert it to degrees, modulus 360
			return result;
		}
        
    }
}