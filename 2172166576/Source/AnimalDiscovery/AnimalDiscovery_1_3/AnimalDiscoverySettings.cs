using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace AnimalDiscovery
{
	public class AnimalDiscoverySettings : ModSettings
	{
		public List<AnimalAlertSettingItem> SettingItems
		{
			get
			{
				this.ResolveItemDef();
				return this.settingItems;
			}
		}

		public bool IsAlertTarget(Pawn p)
		{
			return this.settingItems.Exists((AnimalAlertSettingItem item) => item.IsAlertTarget(p));
		}

		public bool IsPredatorAlertTarget(Pawn p)
		{
			return p.Spawned && !p.Dead && p.Faction == null && p.def.race.Animal && p.def.race.predator;
		}

		public bool IsDetailSettingsAlertTarget(Pawn p)
		{
			if (!p.Spawned || p.Dead || p.Faction != null || !p.def.race.Animal)
            {
				return false;
            }
			return p.Spawned && !p.Dead && p.Faction == null && p.def.race.Animal && p.def.race.predator;
		}

		public override void ExposeData()
		{
			Scribe_Collections.Look<AnimalAlertSettingItem>(ref this.settingItems, "settingItems", LookMode.Deep, Array.Empty<object>());
			Scribe_Values.Look<bool>(ref this.notOnlyPlayerHome, "notOnlyPlayerHome", true, true);
			Scribe_Values.Look<bool>(ref this.predatorAlert, "predatorAlert", false, true);
			Scribe_Values.Look<bool>(ref this.detailAlert, "detailAlert", false, true);
			Scribe_Values.Look<bool>(ref this.customAlert, "customAlert", true, true);

			//Scribe_Values.Look<float>(ref this.settingBodySize, "settingBodySize", 3.0f, false);
		}

		public void ResolveItemDef()
		{
			if (!this.canResolve)
			{
				return;
			}
			foreach (AnimalAlertSettingItem animalAlertSettingItem in this.settingItems)
			{
				animalAlertSettingItem.ResolveDef();
			}
			this.canResolve = this.settingItems.All((AnimalAlertSettingItem item) => !item.IsAvailable);
		}

		public AnimalDiscoverySettings.AlertTargetAnimal alertTargetAnimal;

		public List<AnimalAlertSettingItem> settingItems = new List<AnimalAlertSettingItem>();

		private bool canResolve = true;

		public bool notOnlyPlayerHome = true;

		public bool predatorAlert = false;

		public bool detailAlert = false;

		public bool customAlert = true;

		//public float settingBodySize = 3.0f;

		public enum AlertTargetAnimal
		{
			Custom
		}
	}
}