using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace ResearchableStatUpgrades
{
    public class ResearchFactor
    {
        public ResearchProjectDef def;
        public float factor = 1f;
        public bool applyToNonColonistFaction = false;
        public bool applyToNonHumanlike = false;
        public bool applyToSlave = true;
    }
}
