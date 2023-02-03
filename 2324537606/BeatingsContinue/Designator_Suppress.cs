using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace BeatingsContinue
{
    public class Designator_Suppress : Designator
    {
        public override int DraggableDimensions => 2;

        //protected override DesignationDef Designation => DesignationDefOf.Strip;
        protected override DesignationDef Designation => BeatingsDefsOf.designationDef;

        public Designator_Suppress()
        {
            defaultLabel = "Suppress";
            defaultDesc = "Beat prisoners as much as possible, without permanent damage.";
            icon = ContentFinder<Texture2D>.Get("UI/Designators/Tame");
            soundDragSustain = SoundDefOf.Designate_DragStandard;
            soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
            useMouseIcon = true;
            soundSucceeded = SoundDefOf.Designate_Claim;
            //hotKey = KeyBindingDefOf.Misc11;
        }

        public override AcceptanceReport CanDesignateCell(IntVec3 c)
        {
            if (!c.InBounds(base.Map))
            {
                return false;
            }
            if (!SuppressablesInCell(c).Any())
            {
                return "Must Designate Prisoners";
            }
            return true;
        }

        public override void DesignateSingleCell(IntVec3 c)
        {
            foreach (Thing item in SuppressablesInCell(c))
            {
                DesignateThing(item);
            }
        }

        public override AcceptanceReport CanDesignateThing(Thing t)
        {
            if (base.Map.designationManager.DesignationOn(t, Designation) != null)
            {
                return false;
            }
            if (t is Pawn && (t as Pawn).IsPrisoner)
            {
                return true;
            }
            return false;
        }

        public override void DesignateThing(Thing t)
        {
            base.Map.designationManager.AddDesignation(new Designation(t, Designation));
            //StrippableUtility.CheckSendStrippingImpactsGoodwillMessage(t);
        }

        private IEnumerable<Thing> SuppressablesInCell(IntVec3 c)
        {
            if (c.Fogged(base.Map))
            {
                yield break;
            }
            List<Thing> thingList = c.GetThingList(base.Map);
            for (int i = 0; i < thingList.Count; i++)
            {
                if (CanDesignateThing(thingList[i]).Accepted)
                {
                    yield return thingList[i];
                }
            }
        }
    }
}
