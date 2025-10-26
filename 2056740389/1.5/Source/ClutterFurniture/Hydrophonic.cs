using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;

namespace Clutter_Furniture
{
    [StaticConstructorOnStartup]
    class Hydrophonic : Building_PlantGrower
    {
        public bool ButtonUsed = false;
        public ThingDef plantSet = null;
        private static Texture2D Hydroico;
        public Thing parent = null;
        private List<IntVec3> cachedAdjCellsCardinal;
        private List<IntVec3> AdjCellsCardinal
        {
            get
            {
                if (cachedAdjCellsCardinal == null)
                {
                    cachedAdjCellsCardinal = GenAdj.CellsAdjacentCardinal(this).ToList<IntVec3>();
                }
                return cachedAdjCellsCardinal;
            }
        }
        public void CheckForHydro()
        {
            for (int i = 0; i < AdjCellsCardinal.Count; i++)
            {


                List<Thing> thingList = AdjCellsCardinal[i].GetThingList(this.Map);
                for (int j = 0; j < thingList.Count; j++)
                {
                    Thing thing = thingList.ElementAt(j);
                    if (thing.def.defName == def.defName)
                    {
                        Building_PlantGrower thing2 = thing as Building_PlantGrower;
                        if (thing2.GetPlantDefToGrow() != plantSet)
                        {
                            thing2.SetPlantDefToGrow(plantSet);
                           MoteMaker.ThrowText(thing2.TrueCenter(),this.Map, "^_^", Color.green);

                            Hydrophonic thing3 = thing2 as Hydrophonic;
                            if (!thing3.ButtonUsed)
                            {
                                thing3.plantSet = plantSet;
                                thing3.CheckForHydro();
                            }
                        }
                    }
                }
            }
            ButtonUsed = true;
        }
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            LongEventHandler.ExecuteWhenFinished(ReadXML);

        }

        public override void TickRare()
        {
            base.TickRare();
            if (ButtonUsed)
            {
                ButtonUsed = false;
            }
        }

        private void ReadXML()
        {
            ClutterThingDefsFurniture clutterThingDefs = (ClutterThingDefsFurniture)def;
            if (!string.IsNullOrEmpty(clutterThingDefs.Ui_ButtonHydro))
            {
                Hydroico = ContentFinder<Texture2D>.Get(clutterThingDefs.Ui_ButtonHydro);
            }
            else
            {
                Log.Error("Hydroponic Button Graphic Not Loaded");
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            IList<Gizmo> list = new List<Gizmo>();

            if (Hydroico != null)
            {
                if (!ButtonUsed)
                {
                    list.Add(new Command_Action
                    {
                        icon = Hydroico,
                        defaultDesc = "ClutterButtonSameSeeds".Translate(),
                        hotKey = KeyBindingDefOf.Misc2,
                        activateSound = SoundDef.Named("Click"),
                        action = new Action(MainGrower),
                        groupKey = 887764444

                    });
                }
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

        private void MainGrower()
        {
            plantSet = GetPlantDefToGrow();
            CheckForHydro();
        }
    }
}
