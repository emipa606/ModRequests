using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace AnimalDiscovery
{
    public class Alert_FoundPredator : Alert
    {
		public Alert_FoundPredator()
		{
			this.defaultLabel = "AnimalDiscovery.AlertFoundPredatorLabel".Translate();
			this.defaultPriority = AlertPriority.High;
		}

		private List<Thing> FoundWildPredatorAnimals
		{
			get
			{
				this.cacheFoundWildPredatorAnimals.Clear();
				List<Map> maps = Find.Maps;
				for (int i = 0; i < maps.Count; i++)
				{
					Map map = maps[i];
					if (AnimalDiscoveryMod.Settings.predatorAlert && (map.IsPlayerHome || AnimalDiscoveryMod.Settings.notOnlyPlayerHome) && map.mapPawns.AnyColonistSpawned)
					{
						Func<Thing, bool> validator = delegate (Thing t)
						{
							Pawn pawn = t as Pawn;
							return pawn != null && !pawn.Fogged() && AnimalDiscoveryMod.Settings.IsPredatorAlertTarget(pawn);
						};
						this.cacheFoundWildPredatorAnimals.AddRange(from t in map.listerThings.ThingsInGroup(ThingRequestGroup.Pawn)
															where validator(t)
															select t);
					}
				}
				return this.cacheFoundWildPredatorAnimals;
			}
		}

		public override TaggedString GetExplanation()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (IGrouping<ThingDef, Thing> grouping in from t in this.FoundWildPredatorAnimals
															group t by t.def)
			{
				stringBuilder.AppendLine("AnimalDiscovery.AlertFoundItem".Translate(grouping.Key.LabelCap, grouping.Count<Thing>()));
			}
			return "AnimalDiscovery.AlertFoundPredatorDesc".Translate(stringBuilder.ToString());
		}

		public override AlertReport GetReport()
		{
			return AlertReport.CulpritsAre(this.FoundWildPredatorAnimals);
		}

		private readonly List<Thing> cacheFoundWildPredatorAnimals = new List<Thing>();
	}
}
