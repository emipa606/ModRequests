using System;
using Verse;
using RimWorld;

namespace Mashed_BlackPlague
{
    class Thought_Situational_TuurngaitInColony : Thought_Situational
    {
        public override float MoodOffset()
        {
            return this.BaseMoodOffset * (float)Utility.TuurngaitInFaction(this.pawn.Faction);
        }
    }
}
