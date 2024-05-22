using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Revolus.TextileStats {
    public class SpecialDef {
        private class DefsComparator : IEqualityComparer<Def> {
            public bool Equals(Def x, Def y) => ReferenceEquals(x, y);

            public int GetHashCode(Def obj) => EqualityComparer<Def>.Default.GetHashCode(obj);
        }

        public static readonly IEqualityComparer<Def> CompareDefs = new DefsComparator();
        private static readonly Dictionary<string, Def> byName = new Dictionary<string, Def>();
        private static readonly Dictionary<Def, string> byDef = new Dictionary<Def, string>(CompareDefs);

        public static Def ByName(string defName) => byName.TryGetValue(defName, out var def) ? def : null;

        public static string ByDef(Def def) => byDef.TryGetValue(def, out var defName) ? defName : null;

        public static readonly StatDef Stat_Label = Register<StatDef>("Stat_Label", "name");
        public static readonly StatDef Stat_Count = Register<StatDef>("Stat_Count", "in stock", "How many items of this type you have in your stockpiles.");

        public static T Register<T>(string defName, string label, string description = null) where T : Def, new() {
            if (byName.TryGetValue(defName, out var cachedDef)) {
                return (T) cachedDef;
            }
            
            var def = new T();
            def.label = label;
            def.description = description;
            byDef.Add(def, defName);
            byName.Add(defName, def);
            return def;
        }
    }
}
