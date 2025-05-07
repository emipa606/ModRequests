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
    public class AssignResearch : PatchOperation
    {

        protected override bool ApplyWorker(XmlDocument xml)
        {
            foreach (Assignment a in Assignments)
            {
                DTechprinting.GearAssigner.hardAssignment.SetOrAdd(a.thingDef, a.researchDefName);
            }
            return true;
        }

        public List<Assignment> Assignments;
    }

    public class Assignment
    {
        public string thingDef;
        public string researchDefName;

        public Assignment() { }
    }
}
