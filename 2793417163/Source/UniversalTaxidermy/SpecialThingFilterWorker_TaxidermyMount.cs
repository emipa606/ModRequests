using RimWorld;
using Verse;

namespace UniversalTaxidermy
{
	public class SpecialThingFilterWorker_TaxidermyMount : SpecialThingFilterWorker
	{
		public PawnKindDef PawnKind = PawnKindDefOf.Beggar;
		public override bool Matches(Thing t)
		{
			if( !(t is Thing_TaxidermyMount mount) ) return false;
			if( mount.HeldPawn == null ) return false;
			if( mount.HeldPawn.kindDef == PawnKind ) return true;
			if( mount.HeldPawn.kindDef.race.race.Humanlike && !mount.HeldPawn.IsColonist && ( PawnKind != PawnKindDefOf.Colonist ) ) return true;
			return false;
		}

		public override bool CanEverMatch(ThingDef def)
		{
			return def == DefOfs.TaxidermyMount;
		}
	}
}
