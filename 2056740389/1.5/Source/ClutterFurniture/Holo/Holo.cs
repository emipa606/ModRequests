using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;
using RimWorld;

namespace Clutter_Furniture
{
    [StaticConstructorOnStartup]
    class Holo : ThingWithComps
    {

        private Graphic[] TexResFrames;
        private Graphic TexMain;
        private float grow = 0.1f;
        private int timer = 0;
        private string FramePath;
        private int FrameCount = 0;
        private bool UseFrames = false;
        private bool JustSpawned = false;
        private bool JustDestroyed = false;
        public HoloBase HoloBase;
        public bool DestroyFast = false;
        public CompGlower glowerComp;
        public int cc = 0;


        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            LongEventHandler.ExecuteWhenFinished(SS2);
            glowerComp = this.TryGetComp<CompGlower>();
            glowerComp.UpdateLit(this.Map);
            
        }
        public void SS2()
        {
            ReadFormXML();

            if (!string.IsNullOrEmpty(FramePath) || FrameCount <= 0)
            {
                UseFrames = true;
                TexResFrames = new Graphic_Single[FrameCount];
                for (int i = 0; i < FrameCount; i++)
                {
                    TexResFrames[i] = GraphicDatabase.Get<Graphic_Single>(FramePath + (i + 1), def.graphicData.Graphic.Shader);
                    TexResFrames[i].drawSize = def.graphicData.drawSize;
                }
            }
            JustSpawned = true;
        }

        public override void ExposeData()
        {
            base.ExposeData();

        }

        public override void Tick()
        {
            if(cc==1050)
            {


                glowerComp.UpdateLit(this.Map);

                cc = 0;
                //Log.Message("Glow ON="); //+ glowerComp.Lit);
                         
            }
            cc++;
            base.Tick();
            //if(glowerComp.Lit!=true)
            //{
            //    glowerComp.Lit = true;
            //    glowerComp.UpdateLitX();
            //}
            if (!DestroyFast)
            {
                BaseCheck();
            }
            if (JustSpawned)
            {
                SpawningSec();
            }
            if (JustDestroyed)
            {
                DespawningSec();
            }
            if (UseFrames)
            {
                timer += 1;
                if (timer >= (TexResFrames.Count() * 3))
                {
                    timer = 0;
                }
                handleAnimation();
            }


        }

        private void handleAnimation()
        {
            if (timer < (TexResFrames.Count() * 3))
            {
                int i = timer / 3;

                TexMain = TexResFrames[i];
                TexMain.color = base.Graphic.color;

            }

        }
        private void BaseCheck()
        {
            Thing holoBase = this.Map.thingGrid.ThingAt(Position, ThingDef.Named("HoloEmitter"));
            if (holoBase == null || HoloBase == null)
            {
                DestroyFast = true;
                Destroy();
            }

            if (HoloBase != null)
            {
                //HoloBase holo1 = holoBase as HoloBase;
                //if (HoloBase.SpawnedHolo != this || holo1.SpawnedHolo != this || HoloBase.Destroyed)
                //{
                //    this.DestroyFast = true;
                //    this.Destroy(DestroyMode.Vanish);
                //}


            }

        }

        public override Graphic Graphic
        {
            get
            {
                if (TexMain != null && UseFrames)
                {
                    return TexMain;
                }
                else
                {
                    return base.Graphic;
                }
            }
        }
        //DeSpawning sequence
        private void DespawningSec()
        {
            if (grow <= 0.1f)
            {
                Destroy();
            }
            else if (grow >= 0.1f)
            {
                grow -= 0.05f;
            }
        }

        //Spawning sequence
        private void SpawningSec()
        {
            if (grow >= def.graphicData.drawSize.x)
            {
                grow =def.graphicData.drawSize.x;
                JustSpawned = false;
            }
            else
            {
                grow += 0.05f;

            }
        }

        //Texture drawing 
        public override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            if (TexResFrames.Count() > 0)
            {
                float num = grow / 1.2f;
                Matrix4x4 matrix = default(Matrix4x4);
                Vector3 s = new Vector3(num, 1f, num);
                Vector3 x = new Vector3(0f, 0f, 0.25f);
                matrix.SetTRS(DrawPos + x + Altitudes.AltIncVect, Rotation.AsQuat, s);
                Graphics.DrawMesh(MeshPool.plane10, matrix, Graphic.MatAt(Rotation), 0);
            }
            else
            {
                ReadFormXML();
                if (!string.IsNullOrEmpty(FramePath) || FrameCount <= 0)
                {
                    UseFrames = true;
                    TexResFrames = new Graphic_Single[FrameCount];
                    for (int i = 0; i < FrameCount; i++)
                    {
                        TexResFrames[i] = GraphicDatabase.Get<Graphic_Single>(FramePath + (i + 1), def.graphicData.Graphic.Shader);
                        TexResFrames[i].drawSize = def.graphicData.drawSize;
                    }
                }
               if (TexResFrames.Count() <= 0)
               {
                   Destroy();
               }

            }
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            JustDestroyed = true;
            if (grow <= 0.1f || DestroyFast)
            {
                base.Destroy(mode = 0);
            }
        }
        private void ReadFormXML()
        {
            ClutterHoloDefs newThingDefs = (ClutterHoloDefs)def;
            if (newThingDefs != null)
            {
                FrameCount = newThingDefs.FrameCount;
                FramePath = newThingDefs.FramePath;
            }
        }
    }

}