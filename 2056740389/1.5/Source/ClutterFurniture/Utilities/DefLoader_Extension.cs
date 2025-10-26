using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;
using System.Reflection;

namespace ClutterFurniture
{
    public class DefLoader_Extension : Def
    {

        public override void ResolveReferences()
        {
            base.ResolveReferences();
            string[] rs = { "ComfyTechTierII" };
            CompLoaderUpgradable(ThingDefOf.Bed, "ClutterSlabBed",rs);
            string[] rs2 = { "ComfyTechTierII", "HospitalBed" };
            CompLoaderUpgradable(ThingDef.Named("HospitalBed"), "ClutterEkieBed", rs2);
                       
        }
                

        public void CompLoaderUpgradable(ThingDef thingie, string MarkTwo,string[] ResearchNeeded)
        {
            if (thingie != null)
            {

                CompProps_Upgradable compie = new CompProps_Upgradable();
                compie.compClass = typeof(ClutterFurniture.Comp_PopUpUpgrade);
                compie.UpgradableDef = MarkTwo;
                foreach (string t in ResearchNeeded)
                {
                    compie.ResearchNeeded.Add(t);
                }
                
                if (compie != null)
                {
                    if (!thingie.comps.Contains(compie))
                    {
                        thingie.comps.Add(compie);
                        
                    }
                }
            }
        }

       
    }
}

