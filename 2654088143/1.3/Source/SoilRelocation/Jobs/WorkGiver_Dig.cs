using System;
using Verse;
using Verse.AI;
using RimWorld;

namespace SR
{
	public class WorkGiver_Dig : WorkGiver_ConstructAffectFloor
	{
		protected override DesignationDef DesDef
		{
			get
			{
				//Log.Message("Getting DesDef from WorkGiver_Dig");
				return DesignationDefOf.SR_Dig;
			}
		}

		public override Job JobOnCell(Pawn pawn, IntVec3 c, bool forced = false)
		{
			//Log.Message("Running WorkGiver_Dig.JobOnCell");
			return JobMaker.MakeJob(JobDefs.SR_Dig, c); 
		}
	}
}
