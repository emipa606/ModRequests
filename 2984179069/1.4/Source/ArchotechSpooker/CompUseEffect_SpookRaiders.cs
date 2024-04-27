using System.Linq;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace ArchotechSpooker
{
	public class CompUseEffect_SpookRaiders : CompUseEffect
	{
		public override void DoEffect(Pawn usedBy)
		{
			foreach (Lord lord in Find.CurrentMap.lordManager.lords)
			{
				if (lord.faction != null && lord.faction.HostileTo(Faction.OfPlayer) && lord.faction.def.autoFlee)
				{
					LordToil lordToil = Enumerable.FirstOrDefault(lord.Graph.lordToils, (LordToil st) => st is LordToil_PanicFlee);
					if (lordToil != null)
					{
						lord.GotoToil(lordToil);
					}
				}
			}
		}
	}
}
