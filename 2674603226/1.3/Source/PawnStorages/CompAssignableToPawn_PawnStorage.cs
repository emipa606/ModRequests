using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace PawnStorages
{
	public class CompAssignableToPawn_PawnStorage : CompAssignableToPawn
	{
		public static HashSet<CompAssignableToPawn_PawnStorage> compAssiblables = new HashSet<CompAssignableToPawn_PawnStorage>();

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
			compAssiblables.Add(this);
		}

        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);
			compAssiblables.Remove(this);
		}
		public override IEnumerable<Pawn> AssigningCandidates
		{
			get
			{
				if (!parent.Spawned)
				{
					return Enumerable.Empty<Pawn>();
				}
				return parent.Map.mapPawns.FreeColonists.OrderByDescending((Pawn p) => CanAssignTo(p).Accepted);
			}
		}

		public override string GetAssignmentGizmoDesc()
		{
			return "PS_CommandSetOwnerDesc".Translate();
		}

		public override bool AssignedAnything(Pawn pawn)
		{
			return compAssiblables.Any(x => x != this && x.AssignedPawns.Contains(pawn));
		}

		public override bool ShouldShowAssignmentGizmo()
		{
			return this.parent.Faction == Faction.OfPlayer;
		}
	}
}
