using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResearchableStatUpgrades
{
    public class ResearchMod_Repeatable : ResearchMod_SetResearchToZero
    {
        public override void Apply()
        {
            base.Apply();
            RSUUtil.RepeatableResearchManager.AddToDictionary(this.def, 1);
        }
    }
}
