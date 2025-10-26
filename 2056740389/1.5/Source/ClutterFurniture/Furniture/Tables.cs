using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace Clutter_Furniture
{
    [StaticConstructorOnStartup]
    class ClutterTables : Building
    {
        private List<string> TexPaths = new List<string>();
        private static Texture2D UiButton = null;
        private int GraphIndex = 0;
        private Graphic ChoosenTex;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            LongEventHandler.ExecuteWhenFinished(ReadXML);



        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref GraphIndex, "GraphIndex");
        }


        private void ReadXML()
        {
            ClutterThingDefsFurniture clutterThingDefs = (ClutterThingDefsFurniture)def;
            if (!string.IsNullOrEmpty(clutterThingDefs.UiButtonTex))
            {

                UiButton = ContentFinder<Texture2D>.Get(clutterThingDefs.UiButtonTex);
            }
            if (clutterThingDefs.SwitchTexPath.Count > 0)
            {
                TexPaths = clutterThingDefs.SwitchTexPath;
            }
            if (TexPaths.Count > 0)
            {
                GetGraphics(TexPaths.ElementAt(GraphIndex));
                //Find.MapDrawer.MapMeshDirty(this.Position, MapMeshFlag.Things, false, false);
            }
        }


        private void GetGraphics(string path)
        {
            if (def.graphicData.graphicClass == typeof(Graphic_Multi))
            {
                ChoosenTex = GraphicDatabase.Get<Graphic_Multi>(path, def.graphic.Shader, def.graphicData.drawSize, def.graphicData.color);
            }
            else
            {
                ChoosenTex = GraphicDatabase.Get<Graphic_Single>(path, def.graphic.Shader, def.graphicData.drawSize, def.graphicData.color);
            }
        }

        public override Graphic Graphic
        {
            get
            {
                if (ChoosenTex != null || GraphIndex != 0)
                {
                    return ChoosenTex;
                }
                else
                {
                    return base.Graphic;
                }

            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            IList<Gizmo> list = new List<Gizmo>();
            if (UiButton != null && TexPaths.Count > 0)
            {
                list.Add(new Command_Action
                {
                    defaultDesc = "ClutterButtonTable".Translate(),
                    icon = UiButton,
                    hotKey = KeyBindingDefOf.Misc1,
                    activateSound = SoundDef.Named("Click"),
                    action = new Action(SwitchTextureState),
                    groupKey = 887765221
                });
            }

            IEnumerable<Gizmo> gizmos = base.GetGizmos();
            IEnumerable<Gizmo> result;
            if (gizmos != null)
            {
                result = list.AsEnumerable<Gizmo>().Concat(gizmos);
            }
            else
            {
                result = list.AsEnumerable<Gizmo>();
            }
            return result;
        }

        private void SwitchTextureState()
        {
            if (GraphIndex < TexPaths.Count - 1)
            {
                GraphIndex += 1;
            }
            else
            {
                GraphIndex = 0;
            }
            GetGraphics(TexPaths.ElementAt(GraphIndex));
            this.Map.mapDrawer.MapMeshDirty(Position, (ulong)MapMeshFlagDefOf.Things, false, false);
        }


    }
}
