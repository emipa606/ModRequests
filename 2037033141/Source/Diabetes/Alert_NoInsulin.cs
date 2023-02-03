using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Diabetes
{
	public class Alert_NoInsulin : Alert_Critical
	{
		public Alert_NoInsulin()
		{
			this.defaultLabel = "NoInsulin".Translate();
			this.defaultExplanation = "NoInsulinDesc".Translate();
		}

		public override AlertReport GetReport()
		{
			return HasNoInsulin();
		}

		private bool HasNoInsulin()
		{
			List<Map> maps = Find.Maps;
			ThingDef def = DefDatabase<ThingDef>.GetNamed("Insulin");
			for (int i = 0; i < maps.Count; i++)
			{
				Map map = maps[i];

				if (map.IsPlayerHome && !map.mapPawns.FreeColonistsAndPrisonersSpawned.EnumerableNullOrEmpty())
				{
					int totalInsulin = map.spawnedThings.TotalStackCountOfDef(def);
					if (totalInsulin > 0)
					{
						continue;
					}
					if (HasDiabetic(map))
					{
						return true;
					}
				}
			}

			return false;
		}

		private bool HasDiabetic(Map map)
		{
			foreach (Pawn pawn in map.mapPawns.FreeColonistsAndPrisoners)
			{
				if (pawn.health.hediffSet.HasHediff(DefDatabase<HediffDef>.GetNamed("Diabetes")))
				{
					return true;
				}
			}
			return false;
		}
	}
}
