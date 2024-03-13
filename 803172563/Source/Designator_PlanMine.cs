namespace MinePlan
{
    using System;
    using System.Collections.Generic;
    using JetBrains.Annotations;
    using RimWorld;
    using UnityEngine;
    using Verse;

    [UsedImplicitly]
    public class Designator_PlanMine : Designator
    {

        public override int DraggableDimensions => 2;

        public Designator_PlanMine()
        {
            this.defaultLabel = "Plan to Mine";
            this.icon = ContentFinder<Texture2D>.Get(itemPath: "MTP/PlanMine");
            this.defaultDesc = "Quickly change planning to mining designations";
            this.soundDragSustain = SoundDefOf.Designate_DragStandard;
            this.soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
            this.useMouseIcon = true;
            this.soundSucceeded = SoundDefOf.Designate_Haul;
            DesignationCategoryDef named = DefDatabase<DesignationCategoryDef>.GetNamed(defName: "Orders");
            Type type = named.specialDesignatorClasses.Find(match: x => x == this.GetType());
            if (type != null) return;
            named.specialDesignatorClasses.Add(item: this.GetType());
            named.ResolveReferences();
            DesignationCategoryDef       named2                = DefDatabase<DesignationCategoryDef>.GetNamed(defName: "OrdersPlanMine");
            List<DesignationCategoryDef> allDefsListForReading = DefDatabase<DesignationCategoryDef>.AllDefsListForReading;
            allDefsListForReading.Remove(item: named2);
            DefDatabase<DesignationCategoryDef>.ResolveAllReferences();
        }

        public override AcceptanceReport CanDesignateCell(IntVec3 c)
        {
            if (!c.InBounds(map: Find.CurrentMap))
            {
                return AcceptanceReport.WasRejected;
            }
            if (Find.CurrentMap.designationManager.DesignationAt(c: c, def: DesignationDefOf.Plan) == null)
            {
                return AcceptanceReport.WasRejected;
            }
            if (c.Fogged(map: Find.CurrentMap))
            {
                return AcceptanceReport.WasAccepted;
            }
            Thing thing = c.GetFirstMineable(map: Find.CurrentMap);
            if (thing == null)
            {
                return "MessageMustDesignateMineable".Translate();
            }
            AcceptanceReport result = this.CanDesignateThing(t: thing);
            if (!result.Accepted)
            {
                return result;
            }
            return AcceptanceReport.WasAccepted;
        }

        public override AcceptanceReport CanDesignateThing(Thing t)
        {
            if (Find.CurrentMap.designationManager.DesignationAt(c: t.Position, def: DesignationDefOf.Plan) != null && t.def.mineable)
            {
                return AcceptanceReport.WasAccepted;
            }
            return AcceptanceReport.WasRejected;
        }

        public override void DesignateSingleCell(IntVec3 loc)
        {
            Designation des = Find.CurrentMap.designationManager.DesignationAt(c: loc, def: DesignationDefOf.Plan);
            if (des != null)
            {
                Find.CurrentMap.designationManager.RemoveDesignation(des: des);
                if (Find.CurrentMap.designationManager.DesignationAt(c: loc, def: DesignationDefOf.Mine) == null)
                    Find.CurrentMap.designationManager.AddDesignation(newDes: new Designation(target: loc, def: DesignationDefOf.Mine));
            }
        }

        public override void DesignateThing(Thing t) => this.DesignateSingleCell(loc: t.Position);

        public override void SelectedUpdate() => GenUI.RenderMouseoverBracket();
    }
}
