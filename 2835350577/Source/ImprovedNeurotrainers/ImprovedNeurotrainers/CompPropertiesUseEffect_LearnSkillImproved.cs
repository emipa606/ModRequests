using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace ImprovedNeurotrainers
{
    class CompPropertiesUseEffect_LearnSkillImproved : CompProperties_UseEffect
	{
		public CompPropertiesUseEffect_LearnSkillImproved()
		{
			this.compClass = typeof(CompUseEffect_LearnSkillImproved);
		}

		public string skillDefName;
		public float xpAmount;
	}
}