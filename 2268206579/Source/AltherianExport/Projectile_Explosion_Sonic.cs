using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace AltherianExport
{
    public class Projectile_Explosion_Sonic : Projectile_Explosive
    {
		protected override void Explode()
		{
			MoteMaker.MakeStaticMote(base.Position, base.Map, ThingDefOf_AE.Mote_ImpactSound, 1f);

			base.Explode();
		}

	}
}
