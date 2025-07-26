using System.Collections.Generic;
using Verse;
using RimWorld;

namespace ResistanceRestraintsMod
{
    public class HediffCompProperties_FoodDrain : HediffCompProperties
    {
        public float foodNeedIncreaseRate = 0.001f; // Default rate, configurable in XML

        public HediffCompProperties_FoodDrain()
        {
            this.compClass = typeof(HediffComp_FoodDrain);
        }
    }

    public class HediffComp_FoodDrain : HediffComp
    {
        public HediffCompProperties_FoodDrain Props => (HediffCompProperties_FoodDrain)this.props;

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            if (Pawn?.needs?.food != null)
            {
                // Reduce food level based on XML-defined rate
                Pawn.needs.food.CurLevel -= Props.foodNeedIncreaseRate / 60f;

                // Ensure it doesn't go below zero
                if (Pawn.needs.food.CurLevel < 0f)
                    Pawn.needs.food.CurLevel = 0f;
            }
        }
    }
}