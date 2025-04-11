using System;
using RimWorld;
using Verse;

namespace LobotomyCorp.Weapon
{
    public class Verb_SolemnVow : Verb_Shoot
    {
		private VerbProperties_SolemnVow Props
			=> verbProps as VerbProperties_SolemnVow;

		protected override bool TryCastShot()
		{
			Comp_Solemn_Vow comp = this.EquipmentSource.TryGetComp<Comp_Solemn_Vow>();
			if (comp == null)
            {
				Log.Message("comp is null");
				return base.TryCastShot();
			}

			if (comp.whiteAttack)
            {
				this.Props.CurrentProjectile = Props.whiteBullet;
            }
            else
            {
				this.Props.CurrentProjectile = Props.blackBullet;
			}

			return base.TryCastShot();
		}
	}

	public class VerbProperties_SolemnVow : VerbProperties
	{
		public VerbProperties_SolemnVow()
		{
			verbClass = typeof(Verb_SolemnVow);
		}

		public ThingDef whiteBullet;
		public ThingDef blackBullet;

		public ThingDef CurrentProjectile
		{
			get
			{
				return defaultProjectile;
			}
			set
			{
				defaultProjectile = value;
			}
		}
	}
}
