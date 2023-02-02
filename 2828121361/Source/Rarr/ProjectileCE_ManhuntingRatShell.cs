using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using CombatExtended;

namespace Rarr
{
	public class ProjectileCE_ManhuntingRatShell : ProjectileCE
	{
		private static readonly PawnKindDef rat = PawnKindDef.Named("Rat");

		public override void Impact(Thing hitThing)
		{
			Faction faction = FactionUtility.DefaultFactionFrom(rat.defaultFactionType);
			// Spawns 5 rats
			for (int i = 0; i < 5; i++)
			{
				Pawn pawn = GenSpawn.Spawn(PawnGenerator.GeneratePawn(rat, faction), base.Position, base.Map) as Pawn;
				pawn.health.AddHediff(HediffDefOf.Scaria);
				pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.ManhunterPermanent);
			}
			GenClamor.DoClamor(this, 2.1f, ClamorDefOf.Impact);
			Destroy();
		}
	}
}
