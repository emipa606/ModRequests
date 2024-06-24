using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace YouCanHarvest {
    public class PlantAlertSettingItem : IExposable {
        public string defName;
        public ThingDef def;
        public bool onlyGrowingZone = false;

        public string Label {
            get {
                return def != null ? def.LabelCap.ToString() : defName;
            }
        }

        public bool IsAvailable {
            get {
                return def != null;
            }
        }

        public PlantAlertSettingItem() {

        }

        public PlantAlertSettingItem(ThingDef def, bool onlyGrowingZone) {
            this.def = def;
            this.defName = def.defName;
            this.onlyGrowingZone = onlyGrowingZone;
        }

        public bool IsAlertTarget(Thing t) {
            if (this.def == t.def) {
                if (!onlyGrowingZone) {
                    return true;
                } else {
                    Zone zone = t.Map.zoneManager.ZoneAt(t.Position);
                    return zone != null && zone is Zone_Growing;
                }
            }
            return false;
        }

        public void ExposeData() {
            if (Scribe.mode == LoadSaveMode.Saving && def != null) {
                this.defName = def.defName;
            }
            Scribe_Values.Look(ref defName, "defName");
            Scribe_Values.Look(ref onlyGrowingZone, "onlyGrowingZone");
        }

        public bool ResolveDef() {
            this.def = DefDatabase<ThingDef>.GetNamed(defName, false);
            return this.IsAvailable;
        }
    }
}
