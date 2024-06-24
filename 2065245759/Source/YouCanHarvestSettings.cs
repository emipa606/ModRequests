using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace YouCanHarvest {
    public class YouCanHarvestSettings : ModSettings {
        public enum AlertTargetPlant {
            All,
            InGrowingArea,
            Custom
        }

        public AlertTargetPlant alertTargetPlant = AlertTargetPlant.All;
        public List<PlantAlertSettingItem> settingItems = new List<PlantAlertSettingItem>();

        private bool canResolve = true;

        public List<PlantAlertSettingItem> SettingItems {
            get {
                ResolveItemDef();
                return settingItems;
            }
        }

        public bool IsAlertTarget(Thing t) {
            if (alertTargetPlant == AlertTargetPlant.All) {
                return true;
            }else if (alertTargetPlant == AlertTargetPlant.InGrowingArea) {
                Zone zone = t.Map.zoneManager.ZoneAt(t.Position);
                return zone != null && zone is Zone_Growing;
            }
            return this.settingItems.Exists(item => item.IsAlertTarget(t));
        }

        public override void ExposeData() {
            Scribe_Values.Look(ref alertTargetPlant, "alertTargetPlant", AlertTargetPlant.All);
            Scribe_Collections.Look(ref settingItems, "settingItems", LookMode.Deep);
        }

        public void ResolveItemDef() {
            if (!this.canResolve) {
                return;
            }
            foreach (PlantAlertSettingItem item in settingItems) {
                item.ResolveDef();
            }

            this.canResolve = this.settingItems.All(item => !item.IsAvailable);
        }
    }
}
