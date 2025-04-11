using System;
using RimWorld;
using LobotomyCorp;

namespace Verse
{
	class DamageWorker_LCTwilightCut : DamageWorker
	{

		public override DamageWorker.DamageResult Apply(DamageInfo dinfo, Thing victim)
		{
			Pawn pawn = victim as Pawn;
			if (pawn == null || pawn.Dead)
			{
				Log.Message("Pawn Null or Dead");
				return new DamageWorker.DamageResult();
			}


			DamageInfo redDamage = new DamageInfo();
			redDamage.Def = DamageDefOf.Cut;
			redDamage.SetAmount(dinfo.Amount);

			DamageInfo whiteDamage = new DamageInfo();
			whiteDamage.Def = DefDatabase<DamageDef>.AllDefsListForReading.Find(d => d.defName == "WhiteDmg");
			whiteDamage.SetAmount(dinfo.Amount);

			DamageInfo blackDamage = new DamageInfo();
			blackDamage.Def = DefDatabase<DamageDef>.AllDefsListForReading.Find(d => d.defName == "CutBlack");
			blackDamage.SetAmount(dinfo.Amount);

			DamageInfo paleDamage = new DamageInfo();
			paleDamage.Def = DefDatabase<DamageDef>.AllDefsListForReading.Find(d => d.defName == "PaleDmg");
			paleDamage.SetAmount(dinfo.Amount);

			pawn.TakeDamage(redDamage);
			pawn.TakeDamage(whiteDamage);
			pawn.TakeDamage(blackDamage);
			pawn.TakeDamage(paleDamage);

			return new DamageWorker.DamageResult();

		}
	}
}
