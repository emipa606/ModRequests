using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace HLVRMonsters
{
    public class CompBigMomma : ThingComp
    {
        public override void CompTickRare()
        {
            base.CompTickRare();
            Pawn parentPawn = this.parent as Pawn;
            if (parentPawn.MentalStateDef == MentalStateDefOf.Manhunter | parentPawn.MentalStateDef == MentalStateDefOf.ManhunterPermanent)
            {
                Random rnd = new Random();
                int headcrabType = rnd.Next(2);
                int rollSpawn = rnd.Next(100);
                if (rollSpawn > 30) return;
                Pawn pawn = null;
                switch (headcrabType)
                {
                    case 1:
                        pawn = PawnGenerator.GeneratePawn(HLVRDefOf.Monster_HeadcrabFast, null);
                        break;
                    case 2:
                        pawn = PawnGenerator.GeneratePawn(HLVRDefOf.Monster_HeadcrabPoison, null);
                        break;
                    default:
                        pawn = PawnGenerator.GeneratePawn(HLVRDefOf.Monster_Headcrab, null);
                        break;
                }
                GenSpawn.Spawn(pawn, this.parent.Position, this.parent.Map, this.parent.Rotation, WipeMode.Vanish, false);
            }
        }

    }

    public class CompProperties_BigMomma : CompProperties
    {
        public CompProperties_BigMomma()
        {
            this.compClass = typeof(CompBigMomma);
        }
    }
}
