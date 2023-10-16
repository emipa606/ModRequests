using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace DIYHumans
{
    public class CompProperties_CraftedPawn : CompProperties
    {
        public CompProperties_CraftedPawn()
        {
            this.compClass = typeof(CompCraftedPawn);
        }

        public PawnKindDef pawnKindDef;
        public Faction faction;
        public float mentalStateChance;
        public List<MentalStateDef> possibleMentalStates = new List<MentalStateDef>();
    }
}
