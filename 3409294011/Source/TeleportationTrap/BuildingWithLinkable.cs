using System.Collections.Generic;
using Verse;
using Verse.AI;
using RimWorld;
using UnityEngine;
using System.Linq;

namespace TeleportationTrap
{



    public class BuildingWithLinkable : Building
    {
        public override void DrawExtraSelectionOverlays()
        {
            base.DrawExtraSelectionOverlays();

            // Access the Comp_Linkable and invoke DrawLinks
            var linkableComp = this.TryGetComp<Comp_Linkable>();
            linkableComp?.DrawLinks();
        }
    }






}
