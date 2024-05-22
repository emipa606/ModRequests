using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Revolus.TextileStats {
    public struct RowData {
        public readonly ThingDef def;
        public readonly Texture2D colorTexture;
        public readonly Dictionary<KindAndDef, float> cellValues;

        public RowData(ThingDef def, Dictionary<KindAndDef, float> cellValues, Texture2D colorTexture) {
            this.def = def;
            this.cellValues = cellValues;
            this.colorTexture = colorTexture;
        }

        public RowData(ThingDef def, Dictionary<KindAndDef, float> values) : this(def, values, null) { }

        public void UpdateCount() {
            this.cellValues[new KindAndDef(StatKind.Special, SpecialDef.Stat_Count)] = Find.CurrentMap.resourceCounter.GetCount(this.def);
        }
    }
}
