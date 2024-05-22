using System.Collections.Generic;
using Verse;

namespace Revolus.TextileStats {
    public class KindAndDef : IExposable {
        public StatKind kind;
        public Def def;

        public KindAndDef() {
            this.kind = StatKind.Unset;
            this.def = null;
        }

        public KindAndDef(StatKind kind, Def def) {
            if (def is null) {
                kind = StatKind.Unset;
            }
            this.kind = kind;
            this.def = def;
        }

        public void ExposeData() {
            Scribe_Values.Look(ref this.kind, "kind");
            
            switch (this.kind) {
                case StatKind.Unset: {
                    string name = null;
                    Scribe_Values.Look(ref name, "name");
                    this.def = null;
                    break;
                }
                case StatKind.Special: {
                    var name = this.def != null ? SpecialDef.ByDef(this.def) : null;
                    Scribe_Values.Look(ref name, "name");
                    this.def = !string.IsNullOrEmpty(name) ? SpecialDef.ByName(name) : null;
                    break;
                }
                default: {
                    Scribe_Defs.Look(ref this.def, "name");
                    break;
                }
            }
        }

        public override bool Equals(object obj) {
            return obj is KindAndDef other && this.kind == other.kind && ReferenceEquals(this.def, other.def);
        }

        public override int GetHashCode() {
            var hashCode = 1927805511;
            hashCode = hashCode * -1521134295 + this.kind.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<Def>.Default.GetHashCode(this.def);
            return hashCode;
        }
    }
}
