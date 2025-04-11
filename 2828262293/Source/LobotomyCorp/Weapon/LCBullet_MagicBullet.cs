using RimWorld;
using System.Collections.Generic;
using Verse;

namespace LobotomyCorp.Weapon
{
    public class LCBullet_MagicBullet : Bullet
    {
        private HashSet<Thing> stored;
        private IntVec3 basePosition;

        public LCBullet_MagicBullet()
        {
            basePosition = Position;
            stored = new HashSet<Thing>();
        }

        public override void Tick()
        {

            foreach(Thing t in Map.thingGrid.ThingsListAt(Position).FindAll(
                    (Thing thing) => !stored.Contains(thing)
                    ))
            {
				if (t == launcher 
					|| !(t is Pawn) ) 
					continue;
				
				TakeDamage(t);
				stored.Add(t);
            }

            base.Tick();
        }

        private void TakeDamage(Thing t)
        {
			BattleLogEntry_RangedImpact battleLogEntry_RangedImpact =
				new BattleLogEntry_RangedImpact(launcher, t, intendedTarget.Thing, equipmentDef, def, targetCoverDef);


			DamageInfo dinfo = new DamageInfo(def.projectile.damageDef,
				base.DamageAmount, base.ArmorPenetration,
				ExactRotation.eulerAngles.y, launcher,
				null, equipmentDef, DamageInfo.SourceCategory.ThingOrUnknown,
				intendedTarget.Thing);
			t.TakeDamage(dinfo).AssociateWithLog(battleLogEntry_RangedImpact);

			Pawn pawn = t as Pawn;
			if (pawn != null && pawn.stances != null && pawn.BodySize <= this.def.projectile.StoppingPower + 0.001f)
				pawn.stances.stagger.StaggerFor(95);

			if (this.def.projectile.extraDamages == null) return;

			using (List<ExtraDamage>.Enumerator enumerator = this.def.projectile.extraDamages.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					ExtraDamage current = enumerator.Current;
					if (Rand.Chance(current.chance))
					{
						DamageInfo dinfo2 = new DamageInfo(current.def, current.amount, current.AdjustedArmorPenetration(), this.ExactRotation.eulerAngles.y, this.launcher, null, this.equipmentDef, DamageInfo.SourceCategory.ThingOrUnknown, this.intendedTarget.Thing, true, true);
						t.TakeDamage(dinfo2).AssociateWithLog(battleLogEntry_RangedImpact);
					}
				}
				return;
			}
		}


    }
}
