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
    class HoloBase : Building
    {

        private CompPowerTrader powerComp;
        private static Texture2D UI_Pon;
        public List<ThingDef> HologramList = new List<ThingDef>();
        private int SelectionIndex = 0;
        private static int maxindex = 0;
        public Thing SpawnedHolo;
        private bool powered = false;
        private string HoloOn;
        private int counter = 0;
        private static Graphic TexMain;


        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            LongEventHandler.ExecuteWhenFinished(ss2);
        }

        public void ss2()
        {
            ReadFormXML();
            powerComp = GetComp<CompPowerTrader>();

            if (HologramList.Count > 0)
            {
                maxindex = HologramList.Count - 1;
            }
            if (!string.IsNullOrEmpty(HoloOn))
            {
                TexMain = GraphicDatabase.Get<Graphic_Single>(HoloOn, def.graphicData.Graphic.Shader);
                TexMain.drawSize = def.graphicData.drawSize;

            }
        }

        public void ResetHolo()
        {

        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref SelectionIndex, "SelectedThing");
            Scribe_Values.Look<bool>(ref powered, "Toggle");
            //LongEventHandler.ExecuteWhenFinished(ReadFormXML);
        }

        public override void Tick()
        {
            base.Tick();

            counter += 1;
            
            if ((!powerComp.PowerOn && SpawnedHolo != null) || (powerComp.PowerOn && !powered && SpawnedHolo != null))
            {
                HoloDespawner();
            }
            if (powerComp.PowerOn && powered && SpawnedHolo == null)
            {
                Spawner();
            }
        }

        private void Toggle()
        {
            powered = !powered;
            this.Map.mapDrawer.MapMeshDirty(Position, (ulong)MapMeshFlagDefOf.Things, false, false);
        }

        private bool HoloDespawner()
        {
            if (HologramList.Count > 0)
            {
                List<Thing> thingList = new List<Thing>();
                thingList = this.Map.thingGrid.ThingsListAt(Position);
                for (int i = 0; i < thingList.Count - 1; i++)
                {

                    for (int j = 0; j < HologramList.Count; j++)
                    {
                        if (thingList.ElementAt(i).def == HologramList.ElementAt(j))
                        {
                            thingList.ElementAt(i).Destroy();
                        }
                    }
                }

            }
            if (SpawnedHolo != null && !SpawnedHolo.Destroyed)
            {
                SpawnedHolo.Destroy();
                SpawnedHolo = null;
                return false;
            }
            else
            {
                SpawnedHolo = null;
                return true;
            }
        }

        private void HoloSelectionUp()
        {

            if (SelectionIndex < maxindex)
            {
                SelectionIndex = SelectionIndex + 1;
            }
            else
            {
                SelectionIndex = 0;
            }

        }


        private bool GeneralCheck()
        {
            //Log.Message("State1");
            if (UI_Pon != null)
            {
                //  Log.Message("State2");
                if (HologramList.Count > 0)
                {
                    // Log.Message("State3");
                    if (powerComp.PowerOn)
                    {
                        //  Log.Message("State4");
                        return true;
                    }
                    return false;
                }
            }
            return false;
        }

        private void Spawner()
        {

            if (HoloDespawner())
            {

                if (powerComp.PowerOn)
                {
                    Holo HoloToSpawn = (Holo)ThingMaker.MakeThing(HologramList.ElementAt(SelectionIndex));
                    HoloToSpawn.HoloBase = this;
                    SpawnedHolo = HoloToSpawn;
                    GenSpawn.Spawn(HoloToSpawn, Position, this.Map); 
                }
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            IList<Gizmo> list = new List<Gizmo>();
            if (GeneralCheck())
            {

                Command_Action ca = new Command_Action();
                ca.icon = ContentFinder<Texture2D>.Get(HologramList.ElementAt(SelectionIndex).graphicData.texPath);
                ca.defaultDesc = "ClutterButtonHoloToggle".Translate();
                ca.activateSound = SoundDef.Named("Click");
                ca.action = new Action(Toggle);
                ca.hotKey = KeyBindingDefOf.Misc8;
                ca.groupKey = 887760020;
                list.Add(ca);

                Command_Action ca1 = new Command_Action();
                ca1.icon = UI_Pon;
                ca1.defaultDesc = "ClutterButtonHoloSwitch".Translate();
                ca1.activateSound = SoundDef.Named("Click");
                ca1.action = new Action(HoloSelectionUp);
                ca1.hotKey = KeyBindingDefOf.Misc9;
                ca1.groupKey = 887760021;
                list.Add(ca1);

            }
            IEnumerable<Gizmo> commands = base.GetGizmos();
            IEnumerable<Gizmo> result;

            if (commands != null)
                result = list.AsEnumerable<Gizmo>().Concat(commands);
            else
                result = list.AsEnumerable<Gizmo>();
            return result;
        }

        public override Graphic Graphic
        {
            get
            {
                if (TexMain != null && SpawnedHolo != null)
                {
                    return TexMain;
                }
                else
                {
                    return base.Graphic;
                }
            }
        }


        private void ReadFormXML()
        {
            HoloBaseDefs newThingDefs = (HoloBaseDefs)def;
            if (newThingDefs != null)
            {
                HologramList = newThingDefs.HologramList;
                if (!string.IsNullOrEmpty(newThingDefs.ActiveTexturePath))
                {
                    HoloOn = newThingDefs.ActiveTexturePath;
                }
                if (!string.IsNullOrEmpty(newThingDefs.HoloButtonPath))
                {
                    UI_Pon = ContentFinder<Texture2D>.Get(newThingDefs.HoloButtonPath);

                }
            }

        }





    }

}