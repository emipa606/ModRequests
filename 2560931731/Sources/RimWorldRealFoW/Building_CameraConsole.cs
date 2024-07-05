using System;
using RimWorld;
using UnityEngine;
using Verse;
using System.Text;
namespace RimWorldRealFoW
{
    [StaticConstructorOnStartup]
    public class Building_CameraConsole : Building
    {
        public override void ExposeData()
        {
            base.ExposeData();
        }


        public bool Manned
        {
            get
            {
                return Find.TickManager.TicksGame < this.lastTick + 100;
            }
        }

        public Building_CameraConsole()
        {

        }
        
        public override string GetInspectString()
        {
            StringBuilder inspect = new StringBuilder();
            inspect.Append(base.GetInspectString());
            if(mapComp!=null)
            inspect.AppendInNewLine("CameraCount".Translate() + ": " + mapComp.SurveillanceCameraCount());

            return inspect.ToString();
        }
        public bool WorkingNow
        {
            get
            {
                return FlickUtility.WantsToBeOn(this) && (this.powerComp == null || this.powerComp.PowerOn) && (this.breakdownableComp == null || !this.breakdownableComp.BrokenDown);
            }
        }
        public bool NeedWatcher()
        {
            //return mapComp.SurveillanceCameraCount() >= 1;
            //Turret need the console to work so just keep it like this
            return true;
        }

        public void DrawOverLay()
        {
            if (this.Manned)
            {

                int cameraCount = Mathf.Min(mapComp.SurveillanceCameraCount(), 12);
                if (workingGraphics[cameraCount] == null)
                {
                    workingGraphics[cameraCount] = GraphicDatabase.Get(
                    this.def.graphicData.graphicClass,
                    this.def.graphicData.texPath + "_FX" + (cameraCount).ToString(),
                    ShaderDatabase.MoteGlow,
                    this.def.graphicData.drawSize,
                    this.DrawColor,
                    this.DrawColorTwo

                    );
                }
                workingGraphics[cameraCount].Draw(this.DrawPos + new Vector3(0f, 1f, 0f), base.Rotation, this, 0f);
                if(RFOWSettings.censorMode) {
                    if(censorGraphic == null) {
                    censorGraphic = GraphicDatabase.Get(
                    this.def.graphicData.graphicClass,
                    this.def.graphicData.texPath + "_FX_Censor" ,
                    ShaderDatabase.MoteGlow,
                    this.def.graphicData.drawSize,
                    this.DrawColor,
                    this.DrawColorTwo

                    );                        
                    }
                    censorGraphic.Draw(this.DrawPos + new Vector3(0f, 1f, 0f), base.Rotation, this, 0f);
                }
            
            }
        }
        public override void Draw()
        {
            //((ThingWithComps)this).Draw();
            base.Draw();
            DrawOverLay();

        }

        public void Used()
        {
            this.lastTick = Find.TickManager.TicksGame;
        }


        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.powerComp = base.GetComp<CompPowerTrader>();
            this.breakdownableComp = base.GetComp<CompBreakdownable>();
            this.mapComp = MapUtils.getMapComponentSeenFog(map);
            this.mapComp.RegisterCameraConsole(this);
            //12 Possible graphic so
            /*
			for (int i = 0; i <= 12; i++)
			{
				workingGraphics.Add(GraphicDatabase.Get(
					this.def.graphicData.graphicClass,
					this.def.graphicData.texPath + "_FX" + (i).ToString(), ShaderDatabase.MoteGlow,
					this.def.graphicData.drawSize,
					this.DrawColor,
					this.DrawColorTwo
					));
			}*/
        }

        // Token: 0x06000021 RID: 33 RVA: 0x00002DAC File Offset: 0x00000FAC
        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            base.DeSpawn(mode);
            this.mapComp.DeregisterCameraConsole(this);
        }

        public CompPowerTrader powerComp;

        public CompBreakdownable breakdownableComp;

        public MapComponentSeenFog mapComp;

        public int lastTick;

        private Graphic[] workingGraphics = new Graphic[13];

        private Graphic censorGraphic = null;

    }
}
