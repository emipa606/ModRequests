using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace ClutterFurniture
{
    class ADoor : ThingComp
    {
        public List<Graphic> TexArray = new List<Graphic>();
        public string TexPath;
        public int Texnum;
        private int ticksCounter = 0;
        private int currentFrame=1;
       

        public override void PostSpawnSetup()
        {
            base.PostSpawnSetup();
          
          for(int i=0; i<Texnum;i++)
          {
              TexArray.Add(GraphicDatabase.Get<Graphic_Single>(TexPath + 1));
          }
        }

        public void OpenAnimation(int ticksPerFrame)
        {
            Graphics.DrawMesh(MeshPool.plane10, this.parent.DrawPos, Quaternion.AngleAxis(this.parent.Rotation.AsAngle, Vector3.up), TexArray.ElementAt(currentFrame).MatSingle, 0);

            if (ticksCounter >= ticksPerFrame)
            {
                if (currentFrame >= TexArray.Count - 1)
                    currentFrame = 0;
                else
                    currentFrame++;

                ticksCounter = 0;
            }
            else
            {
                ticksCounter++;
            }
        }

        public void CloseAnimation(int ticksPerFrame)
        {
            Graphics.DrawMesh(MeshPool.plane10, this.parent.DrawPos, Quaternion.AngleAxis(this.parent.Rotation.AsAngle, Vector3.up), TexArray.ElementAt(currentFrame).MatSingle, 0);

            if (ticksCounter >= ticksPerFrame)
            {
                if (currentFrame >= TexArray.Count - 1)
                    currentFrame = Texnum;
                else
                    currentFrame--;

                ticksCounter = 0;
            }
            else
            {
                ticksCounter--;
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            Building_Door doors = this.parent as Building_Door;
            if(doors !=null)
            {
                if(doors.Open)
                {
                    if (currentFrame <= Texnum)
                    {
                        OpenAnimation(3);
                    }
                }
            }
        }


    }
}
