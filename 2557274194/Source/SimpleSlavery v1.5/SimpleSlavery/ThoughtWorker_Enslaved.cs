using System;
using RimWorld;
using Verse;

namespace SimpleSlavery
{
	public class ThoughtWorker_Enslaved : ThoughtWorker
	{
		private static bool IsSteadfast (Pawn pawn){
			if(pawn.story.traits.HasTrait(TraitDef.Named("Wimp")))
				return false;
			if(pawn.story.traits.HasTrait(TraitDef.Named("Nerves"))){
				if (pawn.story.traits.GetTrait (TraitDef.Named ("Nerves")).Degree > 0)
					return true;
				else
					return false;
			}
			return false;
		}

		protected override ThoughtState CurrentStateInternal (Pawn p)
		{
			if (p.health.hediffSet.HasHediff (HediffDef.Named ("Enslaved"))) {
				Hediff_Enslaved enslaved_def = SlaveUtility.GetEnslavedHediff (p);
				if (enslaved_def.ageTicks < 2500 * 3.5f && enslaved_def.SlaveWillpower > 0) // Gets some flavour text just after being enslaved
					return ThoughtState.ActiveAtStage (0);
				if (enslaved_def.SlaveWillpower > 75)
					return ThoughtState.ActiveAtStage (1);
				else if (enslaved_def.SlaveWillpower > 50)
					return ThoughtState.ActiveAtStage (2);
				else if (enslaved_def.SlaveWillpower > 25 || (enslaved_def.SlaveWillpower <= 50 && IsSteadfast(p)))
					return ThoughtState.ActiveAtStage (3);
				else if (Math.Round(enslaved_def.SlaveWillpower) <= 1)
					return ThoughtState.ActiveAtStage (4);
			}
			return ThoughtState.Inactive;
		}
	}
}

