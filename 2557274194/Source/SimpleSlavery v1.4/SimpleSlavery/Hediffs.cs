using RimWorld;
using Verse;

namespace SimpleSlaveryCollars
{
    public class Hediff_Enslaved : HediffWithComps
    {
        public bool shackledGoal = true;
        public bool shackled = true;

        // Save and load all our data
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref shackledGoal, "shackledGoal", false);
            Scribe_Values.Look<bool>(ref shackled, "shackled", false);
        }
        public override bool Visible
        {
            get { return false; }
        }
    }
    public class Hediff_CryptoStasis : HediffWithComps
    {
        public MentalStateDef revertMentalStateDef;

        public void SaveMemory()
        {
            if (pawn.mindState.mentalStateHandler.CurStateDef == SS_MentalStateDefOf.CryptoStasis)
                revertMentalStateDef = MentalStateDefOf.Berserk;

            else
                revertMentalStateDef = pawn.mindState.mentalStateHandler.CurStateDef;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look<MentalStateDef>(ref revertMentalStateDef, "revertMentalStateDef");
        }
    }
}

