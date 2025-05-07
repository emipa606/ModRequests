using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using System.Xml;

namespace DTechprinting
{
    public class SetShardCost : PatchOperation
    {

        protected override bool ApplyWorker(XmlDocument xml)
        {
            foreach (Cost c in Costs)
            {
                ResearchProjectHelper.shardCostAssignment.SetOrAdd(c.researchDefName, c.shardCost);
            }
            return true;
        }

        public List<Cost> Costs;
    }

    public class Cost
    {
        public int shardCost;
        public string researchDefName;

        public Cost() { }
    }
}
