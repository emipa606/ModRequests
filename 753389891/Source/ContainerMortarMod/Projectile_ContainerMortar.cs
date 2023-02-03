using RimWorld;
using Verse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ContainerMortarMod
{
    public class Projectile_ContainerMortar : Projectile , IThingContainerOwner
    {
        private ThingContainer container = null;

        public override void Tick()
        {
            if (this.ticksToImpact == this.StartingTicksToImpact) {
                // first tick
                if (this.launcher != null && this.launcher.def != null && this.launcher.def.defName == "Turret_MortarContainer") {
                    LoadItem(this.launcher);
                }
            }
            base.Tick();
        }

        public override void SpawnSetup()
        {
            base.SpawnSetup();
            if (container == null)
            {
                container = new ThingContainer(this);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.LookDeep<ThingContainer>(ref container, "contains", new Object[] { this });
        }

        private void LoadItem(Thing launcher)
        {
            if (launcher.def.defName != "Turret_MortarContainer") return;

            List<IntVec3> AdjCells = (from c in GenAdj.CellsAdjacentCardinal(launcher)
                                      where c.InBounds()
                                      select c).ToList<IntVec3>();
            
            for (int i = 0; i < AdjCells.Count; i++)
            {
                Thing thing = null;
                Thing thing2 = null;
                List<Thing> thingList = AdjCells[i].GetThingList();
                for (int j = 0; j < thingList.Count; j++)
                {
                    Thing thing3 = thingList[j];
                    if (thing3.IsInValidStorage())
                    {
                        thing = thing3;
                    }
                    if (thing3.def == ThingDef.Named("ContainerMortarLoadingStation"))
                    {
                        thing2 = thing3;
                    }
                }
                if (thing != null && thing2 != null)
                {
                    container.TryAdd(thing);
                }
            }
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            base.Destroy(mode);
            container.TryDropAll(this.Position, ThingPlaceMode.Near);
        }

        public ThingContainer GetContainer()
        {
            return container;
        }

        public IntVec3 GetPosition()
        {
            return this.Position;
        }
    }
}
