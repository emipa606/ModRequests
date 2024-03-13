namespace MinePlan
{
    using System;
    using System.Collections.Generic;
    using JetBrains.Annotations;
    using RimWorld;
    using UnityEngine;
    using Verse;

    [UsedImplicitly]
    public class Designator_MinePlan : Designator
    {

        public override int DraggableDimensions => 2;

        public Designator_MinePlan()
        {
            this.defaultLabel = "Mine to Plan";
            this.icon = ContentFinder<Texture2D>.Get(itemPath: "MTP/MinePlan");
            this.defaultDesc = "Quickly change mining to planning designations";
            this.soundDragSustain = SoundDefOf.Designate_DragStandard;
            this.soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
            this.useMouseIcon = true;
            this.soundSucceeded = SoundDefOf.Designate_Haul;
            DesignationCategoryDef named = DefDatabase<DesignationCategoryDef>.GetNamed(defName: "Orders");
            Type type = named.specialDesignatorClasses.Find(match: x => x == this.GetType());
            if (type == null)
            {
                named.specialDesignatorClasses.Add(item: this.GetType());
                named.ResolveReferences();
                DesignationCategoryDef named2 = DefDatabase<DesignationCategoryDef>.GetNamed(defName: "OrdersMinePlan");
                List<DesignationCategoryDef> allDefsListForReading = DefDatabase<DesignationCategoryDef>.AllDefsListForReading;
                allDefsListForReading.Remove(item: named2);
                DefDatabase<DesignationCategoryDef>.ResolveAllReferences();
            }
        }

        public override AcceptanceReport CanDesignateCell(IntVec3 c)
        {
            if (!c.InBounds(map: Find.CurrentMap)) return AcceptanceReport.WasRejected;
            return Find.CurrentMap.designationManager.DesignationAt(c: c, def: DesignationDefOf.Mine) != null ? AcceptanceReport.WasAccepted : AcceptanceReport.WasRejected;
        }

        public override AcceptanceReport CanDesignateThing(Thing t) => 
            Find.CurrentMap.designationManager.DesignationAt(c: t.Position, def: DesignationDefOf.Mine) != null ? AcceptanceReport.WasAccepted : AcceptanceReport.WasRejected;

        public override void DesignateSingleCell(IntVec3 loc)
        {
            Designation des = Find.CurrentMap.designationManager.DesignationAt(c: loc, def: DesignationDefOf.Mine);
            if (des == null) return;
            Find.CurrentMap.designationManager.RemoveDesignation(des: des);
            if (Find.CurrentMap.designationManager.DesignationAt(c: loc, def: DesignationDefOf.Plan) == null)
                Find.CurrentMap.designationManager.AddDesignation(newDes: new Designation(target: loc, def: DesignationDefOf.Plan));
        }

        public override void DesignateThing(Thing t) => this.DesignateSingleCell(loc: t.Position);

        public override void SelectedUpdate() => GenUI.RenderMouseoverBracket();
    }
}
