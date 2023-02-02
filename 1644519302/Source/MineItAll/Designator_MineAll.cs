using RimWorld;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace MineItAll
{
    class Designator_MineAll : Designator_Mine
    {
        protected override DesignationDef Designation => DefDatabase<DesignationDef>.GetNamed("MineAll");

        public Designator_MineAll()
        {
            defaultLabel = "DesignatorMineAll".Translate();
            defaultDesc = "DesignatorMineAllDesc".Translate();
            icon = ContentFinder<Texture2D>.Get("UI/Designators/MineAll");
            useMouseIcon = true;
            soundDragSustain = SoundDefOf.Designate_DragStandard;
            soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
            soundSucceeded = SoundDefOf.Designate_Mine;
            tutorTag = "Mine";
        }

        public override AcceptanceReport CanDesignateCell(IntVec3 c)
        {
            if (c.Fogged(Map)) return false;
            return base.CanDesignateCell(c);
        }

    }
}
