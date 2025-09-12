using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace Necro
{
	
	public class Verb_PlagueSpewer : Verb
	{
		
		protected override bool TryCastShot()
		{
			bool result;
			if (this.currentTarget.HasThing && this.currentTarget.Thing.Map != this.caster.Map)
			{
				result = false;
			}
			else
			{
				if (base.EquipmentSource != null)
				{
					CompChangeableProjectile comp = base.EquipmentSource.GetComp<CompChangeableProjectile>();
					if (comp != null)
					{
						comp.Notify_ProjectileLaunched();
					}
					CompApparelReloadable comp2 = base.EquipmentSource.GetComp<CompApparelReloadable>();
					if (comp2 != null)
					{
						comp2.UsedOnce();
					}
				}
				IntVec3 position = this.caster.Position;
				float num = Mathf.Atan2(-(float)(this.currentTarget.Cell.z - position.z), (float)(this.currentTarget.Cell.x - position.x)) * 57.29578f;
				FloatRange value = new FloatRange(num - 13f, num + 13f);
				GenExplosion.DoExplosion(position, this.caster.MapHeld, this.verbProps.range, NecroDefOf.PlagueBile, null, -1, -1f, null, null, null, null, ThingDefOf.Filth_SpentAcid, 1f, 1, null, false, null, 0f, 1, 1f, false, null, null, new FloatRange?(value), false, 0.6f, 0f, false, null, 1f, null, null);
				base.AddEffecterToMaintain(NecroDefOf.Plague_SpewShort.Spawn(this.caster.Position, this.currentTarget.Cell, this.caster.Map, 1f), this.caster.Position, this.currentTarget.Cell, 20, this.caster.Map);
				this.lastShotTick = Find.TickManager.TicksGame;
				result = true;
			}
			return result;
		}

		
		public override bool Available()
		{
			bool result;
			if (!base.Available())
			{
				result = false;
			}
			else
			{
				if (this.CasterIsPawn)
				{
					Pawn casterPawn = this.CasterPawn;
					if (casterPawn.Faction != Faction.OfPlayer && casterPawn.mindState.MeleeThreatStillThreat && casterPawn.mindState.meleeThreat.Position.AdjacentTo8WayOrInside(casterPawn.Position))
					{
						return false;
					}
				}
				result = true;
			}
			return result;
		}

		
		public Verb_PlagueSpewer()
		{
		}
	}
}
