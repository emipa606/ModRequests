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
    public class OverrideResearch : PatchOperation
    {
        protected override bool ApplyWorker(XmlDocument xml)
        {
            foreach (Override o in Overrides)
            {
                DTechprinting.GearAssigner.overrideAssignment.SetOrAdd(o.thingDef, o.researchDefName);
            }
            return true;
        }

        public List<Override> Overrides;
    }

    public class Override
    {
        public string thingDef;
        public string researchDefName;

        public Override() { }
    }
}