using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace Shoo
{
    class Designator_Shoo: Designator
    {
        private List<Pawn> justDesignated = new List<Pawn>();

        public override int DraggableDimensions
        {
            get
            {
                return 2;
            }
        }

        public Designator_Shoo()
        {
            defaultLabel = "DesignatorShoo".Translate();
            defaultDesc = "DesignatorShooDesc".Translate();
            icon = ContentFinder<Texture2D>.Get("Shoo", true);
            soundDragSustain = SoundDefOf.Designate_DragStandard;
            soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
            useMouseIcon = true;
            soundSucceeded = SoundDefOf.Designate_Hunt;
            hotKey = KeyBindingDefOf.Misc1;
        }

        public override AcceptanceReport CanDesignateCell(IntVec3 c)
        {
            if (!c.InBounds(base.Map))
            {
                return false;
            }
            if (!HuntablesInCell(c).Any<Pawn>())
            {
                return "MessageMustDesignateShooable".Translate();
            }
            return true;
        }

        public override void DesignateSingleCell(IntVec3 loc)
        {
            foreach (Pawn current in HuntablesInCell(loc))
            {
                DesignateThing(current);
            }
        }

        public override AcceptanceReport CanDesignateThing(Thing t)
        {
            if (t is Pawn pawn && pawn.AnimalOrWildMan() && pawn.Faction == null && Map.designationManager.DesignationOn(pawn, ShooDefOf.Shoo) == null)
            {
                return true;
            }
            return false;
        }

        public override void DesignateThing(Thing t)
        {
            Map.designationManager.RemoveAllDesignationsOn(t, false);
            Map.designationManager.AddDesignation(new Designation(t, ShooDefOf.Shoo));
            justDesignated.Add((Pawn)t);
        }

        protected override void FinalizeDesignationSucceeded()
        {
            base.FinalizeDesignationSucceeded();
            foreach (PawnKindDef kind in (from p in justDesignated
                                          select p.kindDef).Distinct<PawnKindDef>())
            {
                float num = (kind != PawnKindDefOf.WildMan) ? kind.RaceProps.manhunterOnDamageChance : 0.5f;
                if (num > 0.2f)
                {
                    Messages.Message("MessageAnimalDangerousToShoo".Translate(new object[]
                    {
                        kind.GetLabelPlural(-1)
                    }), justDesignated.First((Pawn x) => x.kindDef == kind), MessageTypeDefOf.CautionInput);
                }
            }
            justDesignated.Clear();
        }

        private IEnumerable<Pawn> HuntablesInCell(IntVec3 c)
        {
            if (!c.Fogged(Map))
            {
                List<Thing> thingList = c.GetThingList(Map);
                for (int i = 0; i < thingList.Count; i++)
                {
                    if (CanDesignateThing(thingList[i]).Accepted)
                    {
                        yield return (Pawn)thingList[i];
                    }
                }
            }
        }
    }
}
