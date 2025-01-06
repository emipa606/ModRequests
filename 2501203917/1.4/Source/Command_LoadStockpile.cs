using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;

namespace LoadFromStockpileZone
{
    public class Command_LoadStockpile : Command
    {
        public CompTransporter transporter;
        public List<CompTransporter> group = new List<CompTransporter>();
		protected static HashSet<Building> tmpFuelingPortGivers = new HashSet<Building>();

		public override void ProcessInput(Event ev)
        {
			base.ProcessInput(ev);
			if (group == null)
			{
				group = new List<CompTransporter>();
			}
			if (!group.Contains(transporter))
			{
				group.Add(transporter);
			}
			CompLaunchable launchable = transporter.Launchable;
			if (launchable != null)
			{
				Building fuelingPortSource = launchable.FuelingPortSource;
				if (fuelingPortSource != null)
				{
					Map map = transporter.Map;
					tmpFuelingPortGivers.Clear();
					map.floodFiller.FloodFill(fuelingPortSource.Position, (IntVec3 x) => FuelingPortUtility.AnyFuelingPortGiverAt(x, map), delegate (IntVec3 x)
					{
						tmpFuelingPortGivers.Add(FuelingPortUtility.FuelingPortGiverAt(x, map));
					}, int.MaxValue, false, null);
					for (int i = 0; i < group.Count; i++)
					{
						Building fuelingPortSource2 = group[i].Launchable.FuelingPortSource;
						if (fuelingPortSource2 != null && !tmpFuelingPortGivers.Contains(fuelingPortSource2))
						{
							Messages.Message("MessageTransportersNotAdjacent".Translate(), fuelingPortSource2, MessageTypeDefOf.RejectInput, false);
							return;
						}
					}
				}
			}
			for (int j = 0; j < group.Count; j++)
			{
				if (group[j] != transporter && !transporter.Map.reachability.CanReach(transporter.parent.Position, group[j].parent, PathEndMode.Touch, TraverseParms.For(TraverseMode.PassDoors, Danger.Deadly, false)))
				{
					Messages.Message("MessageTransporterUnreachable".Translate(), group[j].parent, MessageTypeDefOf.RejectInput, false);
					return;
				}
			}
			Dialog_LoadFromStockpile dialog_LoadTransporters = new Dialog_LoadFromStockpile(transporter.Map, group);
			dialog_LoadTransporters.autoLoot = (transporter.Shuttle != null && transporter.Shuttle.CanAutoLoot);
			Find.WindowStack.Add(dialog_LoadTransporters);
		}


        public override bool InheritInteractionsFrom(Gizmo other)
        {
        	Command_LoadStockpile command_other = (Command_LoadStockpile)other;
        	if (command_other.transporter.parent.def != transporter.parent.def)
        	{
        		return false;
        	}
        	else
        	{
        		if (group == null)
        		{
        			group = new List<CompTransporter>();
        		}
        		if (!group.Contains(transporter))
        		{
        			group.Add(command_other.transporter);
        		}
        		return false;
        	}
        }


    }
}
