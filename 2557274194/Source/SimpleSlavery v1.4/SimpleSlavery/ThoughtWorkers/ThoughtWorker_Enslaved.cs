using RimWorld;
using Verse;

namespace SimpleSlaveryCollars
{
	public class ThoughtWorker_Enslaved : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn pawn)
		{
			if (SimpleSlaveryCollarsSetting.SlavestageEnable == false)
				return ThoughtState.Inactive;
			if (pawn.IsSlaveOfColony)
			{
				if (SlaveUtility.TimeAsSlave(pawn) < SlaveUtility.SlaveStage1)
					return ThoughtState.ActiveAtStage(0);
				else if (SlaveUtility.TimeAsSlave(pawn) < SlaveUtility.SlaveStage2)
					return ThoughtState.ActiveAtStage(1);
				else if (SlaveUtility.TimeAsSlave(pawn) < SlaveUtility.SlaveStage3)
					return ThoughtState.ActiveAtStage(2);
				else if (SlaveUtility.TimeAsSlave(pawn) < SlaveUtility.SlaveStage4 || (SlaveUtility.TimeAsSlave(pawn) >= SlaveUtility.SlaveStage3 && SlaveUtility.IsSteadfast(pawn)))
					return ThoughtState.ActiveAtStage(3);
				else if (SlaveUtility.TimeAsSlave(pawn) >= SlaveUtility.SlaveStage4)
				{
					if (pawn.guest.SlaveFaction != Faction.OfPlayer && SimpleSlaveryCollarsSetting.AssimilationSlaveEnable == true)
						pawn.guest.SetGuestStatus(Faction.OfPlayer, GuestStatus.Slave);
					return ThoughtState.ActiveAtStage(4);
				}
			}
			return ThoughtState.Inactive;
		}
	}
}

