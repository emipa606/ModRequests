using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace ResearchableStatUpgrades
{
    public class StatModifierDef : Def
    {
        public override void ResolveReferences()
        {
            base.ResolveReferences();
            if (this.def.parts == null)
            {
                this.def.parts = new List<StatPart>();
            }
            StatPart_ResearchDependent statPart_ResearchDependent = null;
            IEnumerable<StatPart_ResearchDependent> enumerable = this.def.parts.OfType<StatPart_ResearchDependent>();
            if (enumerable != null)
            {
                GenCollection.TryRandomElement(enumerable, out statPart_ResearchDependent);
            }
            if (statPart_ResearchDependent == null)
            {
                statPart_ResearchDependent = new StatPart_ResearchDependent();
                for (int i=0;i<this.factors.Count;i++)
                {
                    statPart_ResearchDependent.AddFactor(this.factors[i]);
                }
                this.def.parts.Add(statPart_ResearchDependent);
            }
        }

        public StatDef def;
        public List<ResearchFactor> factors;
    }
}
