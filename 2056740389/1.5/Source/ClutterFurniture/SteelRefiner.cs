using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace ClutterFurniture
{
    [StaticConstructorOnStartup]
    class SteelRefiner : Building_WorkTable
    {
        public static Graphic[] TexFrameList;
       public static int FrameCount = 28;
       public static string FramePath = "Clutter/Tables/SteelRefFrames/SteelBench";
       public static int FPS = 5;
       private int timer = 0;
       private Graphic MainTex;

       public override void SpawnSetup(Map map, bool respawningAfterLoad)
       {
           base.SpawnSetup(map, respawningAfterLoad);
           LongEventHandler.ExecuteWhenFinished(GraphicLoader);
       }

       public void GraphicLoader()
       {
           TexFrameList = new Graphic_Single[FrameCount];
                for (int i = 0; i < FrameCount; i++)
                {
                    TexFrameList[i] = GraphicDatabase.Get<Graphic_Single>(FramePath + (i), ShaderDatabase.Transparent);
                    TexFrameList[i].drawSize = def.graphicData.drawSize;
                }
       }

       public bool PawnDetector()
       {
           Pawn pawnDetected = this.Map.thingGrid.ThingAt<Pawn>(this.InteractionCell);
           if(pawnDetected!=null)
           {
               if(pawnDetected.CurJob.targetA != null && pawnDetected.CurJob.targetA==this)
               {
                   return true;
               }
               else if (pawnDetected.CurJob.targetB != null && pawnDetected.CurJob.targetB == this)
               {
                   return true;
               }
               else if (pawnDetected.CurJob.targetC != null && pawnDetected.CurJob.targetC == this)
               {
                   return true;
               }
           }
           return false;
       }

       private void handleAnimation()
       {
           if (timer < (TexFrameList.Count() * FPS))
           {
               int i = timer / FPS;

               MainTex = TexFrameList[i];
              
           }
           timer += 1;

       }
       public override void Tick()
       {
           base.Tick();
           if (PawnDetector() && MaterialFinder())
           {
               handleAnimation();
               if (timer >= (TexFrameList.Count() * FPS))
               {
                   timer = 0;
               }
           }
           else if (!PawnDetector())
           {
               MainTex = TexFrameList.FirstOrDefault();
           }
       }

        private bool MaterialFinder()
        {
            Thing thing = this.Map.thingGrid.ThingAt(this.Position, ThingDef.Named("ClutterUnfinishedThing"));
            if(thing!=null)
            {
                return true;
            }
            return false;

        }

       public override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
           base.DrawAt(drawLoc, flip);
                     
           if (MainTex != null)
           {
              Graphics.DrawMesh(MeshPool.plane10, this.DrawPos+new Vector3(0,1,0), Quaternion.AngleAxis(0, Vector3.up), MainTex.MatSingle, 0);
           }

       }
        
    }
}
