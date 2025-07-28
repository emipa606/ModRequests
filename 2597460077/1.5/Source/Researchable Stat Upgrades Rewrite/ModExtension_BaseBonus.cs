using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace ResearchableStatUpgrades
{
    public class ModExtension_BaseBonus : ModExtension_ResearchScaleable
    {
        public override void ResolveCost(ref ResearchProjectDef def, int factor)
        {
            if (this.originalBaseCost < 0f)
            {
                this.originalBaseCost = def.baseCost;
            }
            def.baseCost = this.originalBaseCost + (float)factor * this.bonus;
        }

        public float originalBaseCost;
        public float bonus = 0f;
    }
}
