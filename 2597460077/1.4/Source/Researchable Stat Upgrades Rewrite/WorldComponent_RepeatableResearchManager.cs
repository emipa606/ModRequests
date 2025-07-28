using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace ResearchableStatUpgrades
{
	public class WorldComponent_RepeatableResearchManager : WorldComponent
	{
		public WorldComponent_RepeatableResearchManager(World world) : base(world)
		{
		}

		~WorldComponent_RepeatableResearchManager()
		{
			this.ResetResearchedFactor();
		}

		private void ResetResearchedFactor()
		{
			foreach (ResearchProjectDef researchProjectDef in DefDatabase<ResearchProjectDef>.AllDefs)
			{
				bool flag = researchProjectDef.IsRepeatableResearch();
				if (flag)
				{
					this.researchedFactor.Add(researchProjectDef, 0);
				}
			}
		}

        public override void FinalizeInit()
		{
			base.FinalizeInit();
			for (int i = 0; i < this.researchedFactor.Keys.Count; i++)
			{
				ResearchProjectDef def = this.researchedFactor.Keys.ElementAt(i);
				this.ModifyNewDef(def);
			}
		}

        public override void ExposeData()
		{
            Scribe_Collections.Look<ResearchProjectDef,int>(ref researchedFactor, "researchedTimes", LookMode.Def, LookMode.Value);
		}

		public void ModifyNewDef(ResearchProjectDef def)
		{
			int i;
			for (i = def.description.Length; i >= 0; i--)
			{
				int num;
				bool flag = !int.TryParse(def.description[i - 1].ToString(), out num);
				if (flag)
				{
					break;
				}
			}
			bool flag2 = i != def.description.Length;
			if (flag2)
			{
				def.description = def.description.Remove(i);
			}
			ResearchProjectDef researchProjectDef = def;
			researchProjectDef.description += this.researchedFactor[def].ToString();
			def.GetModExtension<ModExtension_ResearchScaleable>().ResolveCost(ref def, this.researchedFactor[def]);
		}

		public void AddToDictionary(ResearchProjectDef def, int factor = 1)
		{
			bool flag = this.researchedFactor.ContainsKey(def);
			if (flag)
			{
				Dictionary<ResearchProjectDef, int> dictionary = this.researchedFactor;
				dictionary[def] += factor;
			}
			else
			{
				this.researchedFactor.Add(def, factor);
			}
			this.ModifyNewDef(def);
		}

		public int GetFactorFor(ResearchProjectDef def)
		{
			bool flag = def == null || !this.researchedFactor.ContainsKey(def);
			int result;
			if (flag)
			{
				result = 0;
			}
			else
			{
				result = this.researchedFactor[def];
			}
			return result;
		}

		public Dictionary<ResearchProjectDef, int> researchedFactor = new Dictionary<ResearchProjectDef, int>();
	}
}