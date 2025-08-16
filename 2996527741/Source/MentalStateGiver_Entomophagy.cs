using RimWorld;
using Verse;

namespace Entomophagy
{
    class MentalStateGiver_Entomophagy : TraitMentalStateGiver
    {
        public override bool CheckGive(Pawn pawn, int checkInterval)
        {
            int hungerCat = (int)pawn.needs.food.CurCategory;
            float mtb = traitDegreeData.randomMentalStateMtbDaysMoodCurve.Evaluate(hungerCat);
            bool forageTime = Rand.MTBEventOccurs(mtb, 60000f, (float)checkInterval);
            //if (forageTime) Log.Message(pawn.Name + " is foraging with a score of " + mtb.ToString());
            return forageTime && this.traitDegreeData.randomMentalState.Worker.StateCanOccur(pawn) && pawn.mindState.mentalStateHandler.TryStartMentalState(this.traitDegreeData.randomMentalState, "MentalStateReason_Trait".Translate(this.traitDegreeData.GetLabelFor(pawn)), false, false, null, false);
        }
    }
}
