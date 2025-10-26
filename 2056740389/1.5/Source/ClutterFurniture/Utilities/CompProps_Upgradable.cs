using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;

namespace ClutterFurniture
{  

    public class CompProps_Upgradable : CompProperties
    {
        public string UpgradableDef;
        public List<string> ResearchNeeded=new List<string>();
        
        public CompProps_Upgradable()
        {
            this.compClass = typeof(ClutterFurniture.Comp_PopUpUpgrade);
        }

    }
}
