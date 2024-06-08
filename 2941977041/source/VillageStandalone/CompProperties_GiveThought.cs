using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VillageStandalone
{
    public class CompProperties_GiveThoughtVillage : CompProperties
    {
        public CompProperties_GiveThoughtVillage()
        {
            this.compClass = typeof(CompGiveThoughtVillage);
        }

        public ThoughtDef thoughtDef;
        public int radius = 0;
        public bool enableInInventory = false;
    }
}
