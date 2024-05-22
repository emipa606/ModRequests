using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Revolus.TextileStats {
    public class Settings : ModSettings {
        private List<string> stuffTableCategoryBuilding = null;
        private List<string> stuffTableCategoryApparell = null;
        
        private Dictionary<(StatKind, string), string> statFormat = null;

        private string unknownFormat = null;
        private string factorFormat = null;
        private string offsetFormat = null;

        public IEnumerable<string> StuffTableCategoryBuilding {
            get {
                var result = this.stuffTableCategoryBuilding;
                if (result is null || result.Count == 0) {
                    result = SettingsDefault.StuffTableCategoryBuilding;
                }
                return result;
            }
            set {
                this.stuffTableCategoryBuilding = value.ToList();
            }
        }

        public IEnumerable<string> CategoryApparell {
            get {
                var result = this.stuffTableCategoryApparell;
                if (result is null || result.Count == 0) {
                    result = SettingsDefault.StuffTableCategoryApparell;
                }
                return result;
            }
            set {
                this.stuffTableCategoryApparell = value.ToList();
            }
        }

        public string UnknownFormat {
            get {
                var result = this.unknownFormat;
                if (string.IsNullOrWhiteSpace(result)) {
                    result = SettingsDefault.UnknownFormat;
                }
                return result;
            }
            set {
                this.unknownFormat = value;
            }
        }

        public string FactorFormat {
            get {
                var result = this.factorFormat;
                if (string.IsNullOrWhiteSpace(result)) {
                    result = SettingsDefault.FactorFormat;
                }
                return result;
            }
            set {
                this.factorFormat = value;
            }
        }

        public string OffsetFormat {
            get {
                var result = this.offsetFormat;
                if (string.IsNullOrWhiteSpace(result)) {
                    result = SettingsDefault.OffsetFormat;
                }
                return result;
            }
            set {
                this.offsetFormat = value;
            }
        }

        public string StatFormatFor(KindAndDef kindAndDef) {
            var name = kindAndDef.kind != StatKind.Special ? kindAndDef.def.defName : SpecialDef.ByDef(kindAndDef.def);

            string result = null;
            bool lookup(Dictionary<(StatKind, string), string> dict) {
                return dict != null && dict.TryGetValue((kindAndDef.kind, name), out result) && !string.IsNullOrWhiteSpace(result);
            }

            if (name != null && (lookup(this.statFormat) || lookup(SettingsDefault.StatFormat))) {
                return result;
            } else if (kindAndDef.kind == StatKind.Factor) {
                return this.FactorFormat;
            } else if (kindAndDef.kind == StatKind.Offset) {
                return this.OffsetFormat;
            } else {
                return null;
            }
        }

        public override void ExposeData() {
            base.ExposeData();
        }

        internal void ShowAndChangeSettings(Listing_Standard listing) {
            // TODO
        }
    }
}
