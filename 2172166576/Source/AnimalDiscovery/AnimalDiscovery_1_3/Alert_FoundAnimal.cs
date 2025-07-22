using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace AnimalDiscovery
{
    public class Alert_FoundAnimal : Alert
	{
		public Alert_FoundAnimal()
		{
			this.defaultLabel = "AnimalDiscovery.AlertFoundAnimalLabel".Translate();
			this.defaultPriority = AlertPriority.High;
		}

		private List<Thing> FoundWildAnimals
		{
			get
			{
				this.cacheFoundWildAnimals.Clear();
				List<Map> maps = Find.Maps;
				for (int i = 0; i < maps.Count; i++)
				{
					Map map = maps[i];
					if (AnimalDiscoveryMod.Settings.customAlert && (map.IsPlayerHome || AnimalDiscoveryMod.Settings.notOnlyPlayerHome) && map.mapPawns.AnyColonistSpawned)
					{
                        bool validator(Thing t)
                        {
                            return t is Pawn pawn && !pawn.Fogged() && AnimalDiscoveryMod.Settings.IsAlertTarget(pawn);
                        }
                        this.cacheFoundWildAnimals.AddRange(map.listerThings.ThingsInGroup(ThingRequestGroup.Pawn).Where(x => validator(x)));
					}
				}
				return this.cacheFoundWildAnimals;
			}
		}

		public override TaggedString GetExplanation()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (IGrouping<ThingDef, Thing> grouping in from t in this.FoundWildAnimals
															group t by t.def)
			{
				stringBuilder.AppendLine("AnimalDiscovery.AlertFoundItem".Translate(grouping.Key.LabelCap, grouping.Count<Thing>()));
			}
			return "AnimalDiscovery.AlertFoundAnimalDesc".Translate(stringBuilder.ToString());
		}

		public override AlertReport GetReport()
		{
			return AlertReport.CulpritsAre(this.FoundWildAnimals);
		}

		private readonly List<Thing> cacheFoundWildAnimals = new List<Thing>();
	}
}