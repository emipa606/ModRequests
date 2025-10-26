using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using System.Reflection;
using System.Linq;
namespace Clutter_Structure
{
    [StaticConstructorOnStartup]
    internal class Wall_Multi : Building
    {
        public static Graphic SecondaryMaterial = null;
        public static Graphic ThirdMaterial = null;
        public Graphic_LinkedCornerFiller Graphic2nd = null;
        public Graphic_LinkedCornerFiller Graphic3rd = null;
        public ClutterStructureThingDefs def2;
        private int WallRandom = -1;
        private Map mapzie;
       
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
 	        base.SpawnSetup(map, respawningAfterLoad);
                
            mapzie = map;
            if (this.WallRandom == -1)
            {
                this.WallRandom = UnityEngine.Random.Range(1, 100);
            }
           // Log.Message(this.Stuff.stuffProps.categories.FirstOrDefault().ToString());
            
            LongEventHandler.ExecuteWhenFinished(SS2);
        }
        
        public void SS2()
        {
            this.GetGraphics();
        }

        public override Graphic Graphic
        {
            get
            {
                Graphic result;
                if (this.Graphic2nd == null || this.Graphic2nd.MatSingle == null)
                {
                    //this.GetGraphics();
                    if (this.Graphic2nd == null || this.Graphic2nd.MatSingle == null)
                    {
                        result = this.def.graphic;
                        return result;
                    }
                }
                if (this.WallRandom >= 56 && this.WallRandom <= 74)
                {
                    result = this.Graphic2nd;
                }
                else
                {
                    if (this.WallRandom > 75 && this.WallRandom <= 94)
                    {
                        result = this.Graphic3rd;
                    }
                    else
                    {
                        result = this.def.graphic;
                    }
                }
                return result;
            }
        }


        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.WallRandom, "WallSelectionNumber", -1, false);
            LongEventHandler.ExecuteWhenFinished(GetGraphics);
        }

        //public string StuffTex()
        //{
        //    if(this.Stuff!=null)
        //    {
        //        if(this.Stuff.cat)
        //    }
        //}
        private void GetGraphics()
        {
            this.def2 = (this.def as ClutterStructureThingDefs);
            if (this.Graphic2nd == null || this.Graphic2nd.MatSingle == null)
            {

                Graphic graphic_Single = GraphicDatabase.Get<Graphic_Single>(this.def2.SecondaryMaterialPath);
                this.Graphic2nd = new Graphic_LinkedCornerFiller(graphic_Single);
            }
            if (this.Graphic3rd == null || this.Graphic3rd.MatSingle == null)
            {

                Graphic graphic_Single2 = GraphicDatabase.Get<Graphic_Single>(this.def2.ThirdMaterialPath);
                this.Graphic3rd = new Graphic_LinkedCornerFiller(graphic_Single2);
            }
        }
       
        //public override IEnumerable<Gizmo> GetGizmos()
        //{

           
        //        IEnumerator<Gizmo> enumerator = base.GetGizmos().GetEnumerator();
        //        while (enumerator.MoveNext())
        //        {
        //            Gizmo current = enumerator.Current;

        //            yield return current;
        //        }

        //        if (this.Faction == Faction.OfPlayer)
        //        {

        //            yield return new Command_Action
        //            {
        //                action = delegate
        //                {
        //                    this.WallLightSpawn();
        //                },
        //                hotKey = KeyBindingDefOf.Misc1,
        //                icon = Wall_Multi.RebuildIco,
        //                defaultLabel = "ClutterButtonMountLights".Translate()
        //            };

        //        }
            
        //    yield break;

        //}
                
        //private void WallLightSpawn()
        //{
            
        //    Map thizMapz = this.Map;
        //    ThingDef thingie = ThingDef.Named("ClutterUWallLight");
        //    if (thingie != null && thizMapz!=null)
        //    {
        //        if (thingie.blueprintDef==null)
        //        {
        //            MethodInfo generateBlueprint = typeof(ThingDefGenerator_Buildings).GetMethod("NewBlueprintDef_Thing", BindingFlags.NonPublic | BindingFlags.Static);
        //            generateBlueprint.Invoke(this, new object[] { thingie, false, null });
        //            thingie.blueprintDef.PostLoad();
                                   
        //        }
        //        if(thingie.frameDef==null)
        //        {
        //            MethodInfo generateFrame = typeof(ThingDefGenerator_Buildings).GetMethod("NewFrameDef_Thing", BindingFlags.NonPublic | BindingFlags.Static);
        //            generateFrame.Invoke(this, new object[] { thingie });
        //            thingie.frameDef.PostLoad();
                   
        //        }
        //        this.Destroy(0);
        //        GenConstruct.PlaceBlueprintForBuild(GenConstruct.BuiltDefOf(ThingDef.Named("ClutterUWallLight")), base.Position, thizMapz, base.Rotation, Faction.OfPlayer, null);
                              
        //    }
        //    else
        //    {
        //        Log.Error("<<No thingdef called ClutterUWallLight found>>");
        //    }

            
           
            
        //}
      }
}
