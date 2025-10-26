using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;


namespace Clutter_Structure
{
    [StaticConstructorOnStartup]
    internal class Wall_Light : Building
    {
        public static Graphic PowerOffMaterial2 = null;
        public Graphic_LinkedCornerFiller Graphic2nd2 = null;
        public ClutterStructureThingDefs def2;
        private CompPowerTrader powerComp;
        private CompGlower glowerComp;
        private CompPowerTransmitter powerTrans;
        private bool Switcher = false;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
       
            powerComp = base.GetComp<CompPowerTrader>();
            powerTrans = base.GetComp<CompPowerTransmitter>();
            glowerComp = base.GetComp<CompGlower>();
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
                if (this.Graphic2nd2 == null || this.Graphic2nd2.MatSingle == null)
                {
                    //this.GetGraphics();
                    if (this.Graphic2nd2 == null || this.Graphic2nd2.MatSingle == null)
                    {
                        result = this.def.graphic;
                        return result;
                    }
                }
                if (!Switcher)
                {
                    result = this.Graphic2nd2;
                }
                else
                {
                    result = this.def.graphic;
                }
                return result;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            LongEventHandler.ExecuteWhenFinished(GetGraphics);
        }
        private void GetGraphics()
        {
            this.def2 = (this.def as ClutterStructureThingDefs);
            if (this.Graphic2nd2 == null || this.Graphic2nd2.MatSingle == null)
            {
                Graphic graphic_Single = GraphicDatabase.Get<Graphic_Single>(this.def2.PowerOffMaterialPath);
                this.Graphic2nd2 = new Graphic_LinkedCornerFiller(graphic_Single);
            }

        }

        public override void Tick()
        {
            base.Tick();

            if (this.powerComp.PowerOn && !Switcher)
            {
                Switcher = true;
                this.Map.mapDrawer.MapMeshDirty(this.Position, (ulong)MapMeshFlagDefOf.Things, false, false);

            }
            if (!this.powerComp.PowerOn && Switcher)
            {
                Switcher = false;
                this.Map.mapDrawer.MapMeshDirty(this.Position, (ulong)MapMeshFlagDefOf.Things, false, false);

            }

        }

    }
}
