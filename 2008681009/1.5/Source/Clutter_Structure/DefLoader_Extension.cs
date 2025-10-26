using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;
using System.Reflection;

namespace Clutter_StructureWall
{
    [StaticConstructorOnStartup]
    public class CSW_DefLoader : Def
    {
        public override void ResolveReferences()
        {
            base.ResolveReferences();
           // CompLoader2(ThingDef.Named("ClutterUtilityWall"));

           //  BuildingDefs("ClutterUWallLight");
             WallHeadButting();
            if (C_StructureWallModUI.ModdedWallsActive && DefDatabase<ThingDef>.GetNamed("ClutterSilverWall", false)==null)
            {
                StructureDefGenerator.WallDefs();
            }
            
        }

        public void WallHeadButting()
        {
            if (C_StructureWallModUI.WallTextureActive)
            {                
                if (ThingDefOf.Wall != null)
                {
                    ThingDef tDef = ThingDefOf.Wall;
                   // tDef.thingClass = typeof(Clutter_Structure.Wall_Multi);
                    tDef.graphicData.texPath = "Clutter/ValWall";
                    tDef.uiIconPath = "Clutter/Ui/ValIco_Smooth";
                }
            }
        }
      
       

        //public void BuildingDefs(string ThingieName)
        //{
        //    ThingDef Thingie = DefDatabase<ThingDef>.GetNamed(ThingieName);
                   
        //    if (Thingie != null)
        //    {
        //        if (Thingie.blueprintDef == null)
        //        {
        //            MethodInfo generateBlueprint = typeof(ThingDefGenerator_Buildings).GetMethod("NewBlueprintDef_Thing", BindingFlags.NonPublic | BindingFlags.Static);
        //            generateBlueprint.Invoke(this, new object[] { Thingie, false, null });
        //            Thingie.blueprintDef.PostLoad();
        //            DefDatabase<ThingDef>.Add(Thingie.blueprintDef);

        //        }
        //        if (Thingie.frameDef == null)
        //        {

        //            MethodInfo generateFrame = typeof(ThingDefGenerator_Buildings).GetMethod("NewFrameDef_Thing", BindingFlags.NonPublic | BindingFlags.Static);
        //            generateFrame.Invoke(this, new object[] { Thingie });
        //            Thingie.frameDef.PostLoad();
        //            Thingie.frameDef.stuffCategories = Thingie.stuffCategories;
        //            DefDatabase<ThingDef>.Add(Thingie.frameDef);

        //        }
        //    }
        //}
               
        public void CompLoader2(ThingDef Wallzie)
        {
            if (Wallzie != null)
            {
                //Log.Message("Wallzie found");
                CompProperties compie = new CompProperties();
                compie.compClass = typeof(Clutter_Structure.Comp_BuildableWallLight);
                //Log.Message("Wallzie comp made");
                if (compie != null)
                {
                    if (!Wallzie.comps.Contains(compie))
                    {
                        Wallzie.comps.Add(compie);
                        //Log.Message("Wallzie comp added");
                    }
                }
            }
        }
               
    }
}
