using RimWorld;
using Verse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ContainerMortarMod
{
    public class PlaceWorker_NextToContainerMortarAccepter : PlaceWorker
    {

        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot)
        {
            for (int i = 0; i < 4; i++)
            {
                IntVec3 c = loc + GenAdj.CardinalDirections[i];
                if (c.InBounds())
                {
                    List<Thing> thingList = c.GetThingList();
                    for (int j = 0; j < thingList.Count; j++)
                    {
                        Thing thing = thingList[j];
                        if (thing.def.defName == "Turret_MortarContainer") {
                            return true;
                        }
                    }
                }
            }
            return "MustPlaceNextToContainerMortarAccepter".Translate();
        }
    }
}