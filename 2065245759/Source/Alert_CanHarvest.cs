using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace YouCanHarvest
{
	public class Alert_CanHarvest : Alert {
		private List<Thing> cacheHarvestablePlants = new List<Thing>();

		public Alert_CanHarvest() {
			this.defaultLabel = "YouCanHarvest.AlertCanHarvestLabel".Translate();
			this.defaultPriority = AlertPriority.High;
		}

		private List<Thing> HarvestablePlants {
			get {
				this.cacheHarvestablePlants.Clear();
				List<Map> maps = Find.Maps;
				for (int i = 0; i < maps.Count; i++) {
					Map map = maps[i];
					if (map.IsPlayerHome && map.mapPawns.AnyColonistSpawned) {
						Func<Thing, bool> validator = delegate (Thing t) {
							Plant p = t as Plant;
							return p != null && p.HarvestableNow && p.LifeStage == PlantLifeStage.Mature && !p.Fogged() && YouCanHarvestMod.Settings.IsAlertTarget(p);
						};
						this.cacheHarvestablePlants.AddRange(map.listerThings.ThingsInGroup(ThingRequestGroup.Plant).Where(t => validator(t)));
					}
				}
				return this.cacheHarvestablePlants;
			}
		}

		public override TaggedString GetExplanation() {
			StringBuilder stringBuilder = new StringBuilder();
			foreach (var group in this.HarvestablePlants.GroupBy(t => t.def)) {
				;
				stringBuilder.AppendLine("YouCanHarvest.AlertCanHarvestItem".Translate(group.Key.LabelCap, group.Count()));
			}
			return "YouCanHarvest.AlertCanHarvestDesc".Translate(stringBuilder.ToString());
		}

		public override AlertReport GetReport() {
			return AlertReport.CulpritsAre(this.HarvestablePlants);
		}
	}
}
