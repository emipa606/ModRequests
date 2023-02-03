using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Diabetes
{
	public class Alert_LowInsulin : Alert
	{
		public Alert_LowInsulin()
		{
			this.defaultLabel = "LowInsulin".Translate();
		}

		public override TaggedString GetExplanation()
		{
			Map map = this.MapLowInsulin();
			if (map == null)
			{
				return "";
			}

			int totalInsulin = map.resourceCounter.GetCount(TypeGetter.ThingDef(EThingDef.Insulin));
			int totalDiabetics = GetTotalDiabetics(map);
			return "LowInsulinDesc".Translate(totalInsulin.ToStringCached(), totalDiabetics.ToStringCached());
		}

		public override AlertReport GetReport()
		{
			return MapLowInsulin() != null;
		}

		private Map MapLowInsulin()
		{
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				Map map = maps[i];
				ThingDef def = TypeGetter.ThingDef(EThingDef.Insulin);
				if (map.IsPlayerHome && map.mapPawns.AnyColonistSpawned)
				{
					int totalInsuline = map.spawnedThings.TotalStackCountOfDef(def);

					if (totalInsuline == 0)
					{
						return null;
					}

					totalInsuline -= GetTotalDiabetics(map) * InsulinThreshold;

					if (totalInsuline < 0)
					{
						return map;
					}
				}
			}
			return null;
		}

		private int GetTotalDiabetics(Map map)
		{
			int total = 0;
			foreach (Pawn pawn in map.mapPawns.FreeColonistsAndPrisoners)
			{
				if (pawn.health.hediffSet.HasHediff(TypeGetter.HediffDef(EHediffDef.Diabetes)))
				{
					total++;
				}
			}
			return total;
		}

		private const int InsulinThreshold = 4;

		private List<Pawn> diabetics = new List<Pawn>();
	}
}
