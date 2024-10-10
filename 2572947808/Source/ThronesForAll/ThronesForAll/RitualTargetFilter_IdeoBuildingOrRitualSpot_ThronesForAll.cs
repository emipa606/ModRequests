using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using Verse.AI;

namespace ThronesForAll
{

    class RitualTargetFilter_IdeoBuildingOrRitualSpot_ThronesForAll : RitualTargetFilter_IdeoBuildingOrRitualSpot
    {
		public RitualTargetFilter_IdeoBuildingOrRitualSpot_ThronesForAll()
		{
		}
		public RitualTargetFilter_IdeoBuildingOrRitualSpot_ThronesForAll(RitualTargetFilterDef def)
	: base(def)
		{
		}
		public override TargetInfo BestTarget(TargetInfo initiator, TargetInfo selectedTarget)
        {
			if(!ModsConfig.IdeologyActive)
            {
				return selectedTarget;
            }

			Pawn pawn = (Pawn)initiator.Thing;

			/*if(initiator != null)
            {
				//Log.Message("initiator is " + initiator.Label);
            }

			if(selectedTarget != null)
            {
				//Log.Message("selectedTarget is " + selectedTarget.Label);
            }*/

			if(pawn.ownership.AssignedThrone != null && RoomRoleWorker_ThroneRoom.Validate(pawn.ownership.AssignedThrone.GetRoom()) == null) // and make sure throne is usable
            {
				//Log.Message("returning throne");
				return pawn.ownership.AssignedThrone;
            }
			return base.BestTarget(initiator, selectedTarget);
		}

	}
}
