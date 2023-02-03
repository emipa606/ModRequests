using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Vampire
{
	public class WorldComponent_Improved_VampireSettings : WorldComponent
	{
		public CaitiffMode caitiffmode = CaitiffMode.Default;
		//public bool settingsWindowSeen = false;
		public int CaitiffXP = 5;

		public WorldComponent_Improved_VampireSettings(World world) : base(world)
		{
		}

		public void ApplySettings()
		{
			switch (caitiffmode)
			{
				case CaitiffMode.Default:
					CaitiffXP = 5;
					break;

				case CaitiffMode.Custom:
					break;
			}
			Log.Message("Vampire settings applied.");
			Messages.Message("Vampire settings applied.", null, MessageTypeDefOf.TaskCompletion);

		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref caitiffmode, "caitiffmode");
			Scribe_Values.Look(ref CaitiffXP, "CaitiffXP", 5);
		}
	}
}
