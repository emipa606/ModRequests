using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.Noise;

namespace RadWorld
{
	public class StatPart_MutationHediff : StatPart
	{
		public override void TransformValue(StatRequest req, ref float val)
		{
			if (req.Pawn != null && TryGetEnlargedOrWeakenedMuscles(req.Pawn, out Hediff hediff))
			{
				if (hediff.def == RW_DefOf.RW_EnlargedMuscles)
                {
					val *= 1.30f;
				}
				else if (hediff.def == RW_DefOf.RW_WeakenedMuscles)
                {
					val /= 1.30f;
				}
			}
		}

		public override string ExplanationPart(StatRequest req)
		{
			return null;
		}

		private bool TryGetEnlargedOrWeakenedMuscles(Pawn pawn, out Hediff hediff)
		{
			hediff = pawn.health.hediffSet.GetFirstHediffOfDef(RW_DefOf.RW_EnlargedMuscles);
			if (hediff is null)
            {
				hediff = pawn.health.hediffSet.GetFirstHediffOfDef(RW_DefOf.RW_WeakenedMuscles);
			}
			return hediff != null;
		}
	}
}
