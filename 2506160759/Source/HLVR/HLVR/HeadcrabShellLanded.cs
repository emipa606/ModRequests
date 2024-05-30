using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using Verse.Sound;

namespace HLVRMonsters
{
    public class HeadcrabShellLanded : Building
    {
        int ticksToDeploy = 320;
        public ThingDef_HeadcrabShellLanded Def
        {
            get
            {
                return this.def as ThingDef_HeadcrabShellLanded;
            }
        }

        public override void Tick()
        {
            if (ticksToDeploy <= 0)
            {
                Random rnd = new Random();
                int headcrabAmount = rnd.Next(3, 6);
                int headcrabType = rnd.Next(2);
                IntVec3 strikeLocation = this.Position;
                Pawn pawn = null;
                for (int i = 0; i <= headcrabAmount; i++)
                {
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
                    IntVec3 loc = CellFinder.RandomClosewalkCellNear(this.Position, this.Map, 2, null);
                    GenSpawn.Spawn(pawn, loc, this.Map, this.Rotation, WipeMode.Vanish, false);
                }
                for (int i = 0; i < 1; i++)
                {
                    GenPlace.TryPlaceThing(ThingMaker.MakeThing(ThingDefOf.ChunkSlagSteel), this.Position, this.Map, ThingPlaceMode.Near);
                }
                SoundDefOf.DropPod_Open.PlayOneShot(new TargetInfo(this.Position, this.Map));
                this.Destroy();
            }
            ticksToDeploy--;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.ticksToDeploy, "ticksToDeploy", 0, false);
        }

    }

    public class ThingDef_HeadcrabShellLanded : ThingDef
    {
        public string HeadcrabType;
        public int HeadcrabCount;
    }



}
