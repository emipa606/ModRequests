using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AnimalDiscovery
{
    public class Alert_FoundDetailSettings : Alert
    {
        public Alert_FoundDetailSettings()
        {
            this.defaultLabel = "AnimalDiscovery.AlertFoundDetailSettingsLabel".Translate();
            this.defaultPriority = AlertPriority.High;
        }

		private List<Thing> FoundWildDetailSettingAnimals
		{
			get
			{
				this.cacheFoundWildDetailSettingAnimals.Clear();
				List<Map> maps = Find.Maps;
				for (int i = 0; i < maps.Count; i++)
				{
					Map map = maps[i];
					if (AnimalDiscoveryMod.Settings.detailAlert && (map.IsPlayerHome || AnimalDiscoveryMod.Settings.notOnlyPlayerHome) && map.mapPawns.AnyColonistSpawned)
					{
						Func<Thing, bool> validator = delegate (Thing t)
						{
							Pawn pawn = t as Pawn;
							return pawn != null && !pawn.Fogged() && AnimalDiscoveryMod.Settings.IsDetailSettingsAlertTarget(pawn);
						};
						this.cacheFoundWildDetailSettingAnimals.AddRange(from t in map.listerThings.ThingsInGroup(ThingRequestGroup.Pawn)
																	where validator(t)
																	select t);
					}
				}
				return this.cacheFoundWildDetailSettingAnimals;
			}
		}

		public override TaggedString GetExplanation()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (IGrouping<ThingDef, Thing> grouping in from t in this.FoundWildDetailSettingAnimals
															group t by t.def)
			{
				stringBuilder.AppendLine("AnimalDiscovery.AlertFoundItem".Translate(grouping.Key.LabelCap, grouping.Count<Thing>()));
			}
			return "AnimalDiscovery.AlertFoundDetailSettingsDesc".Translate(stringBuilder.ToString());
		}

		public override AlertReport GetReport()
		{
			return AlertReport.CulpritsAre(this.FoundWildDetailSettingAnimals);
		}

		private List<Thing> cacheFoundWildDetailSettingAnimals = new List<Thing>();
	}
}
