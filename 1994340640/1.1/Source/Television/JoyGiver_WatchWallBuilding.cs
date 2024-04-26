using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace WallStuff
{
    class JoyGiver_WatchWallBuilding : JoyGiver_WatchBuilding
    {
        private static List<Thing> tmpCandidates = new List<Thing>();

        public override Job TryGiveJob(Pawn pawn)
        {
            //jcLog.Warning(pawn.Name + " Try Give job");
            Thing thing = this.FindBestGame(pawn, false, IntVec3.Invalid);
            if (thing != null)
            {
                //jcLog.Warning(pawn.Name + " Found Thing");
                return this.TryGivePlayJob(pawn, thing);
            }
            return null;
        }

        public override Job TryGiveJobWhileInBed(Pawn pawn)
        {
            //jcLog.Warning(pawn.Name + " Try Give job in bed");
            Thing thing = this.FindBestGame(pawn, true, IntVec3.Invalid);
            if (thing != null)
            {
                //jcLog.Warning(pawn.Name + " Found Thing");
                return this.TryGivePlayJobWhileInBed(pawn, thing);
            }
            return null;
        }


        private Thing FindBestGame(Pawn pawn, bool inBed, IntVec3 partySpot)
        {
            //jcLog.Warning(pawn.Name + " Find Best");
            tmpCandidates.Clear();
            this.GetSearchSet(pawn, tmpCandidates);
            if (tmpCandidates.Count == 0)
            {
                return null;
            }
            Predicate<Thing> predicate = (Thing t) => this.CanInteractWith(pawn, t, inBed);
            if (partySpot.IsValid)
            {
                Predicate<Thing> oldValidator = predicate;
                predicate = ((Thing x) => GatheringsUtility.InGatheringArea(x.Position, partySpot, pawn.Map) && oldValidator(x));
            }
            IntVec3 position = pawn.Position;
            Map map = pawn.Map;
            List<Thing> searchSet = tmpCandidates;
            Predicate<Thing> validator = predicate;
            Thing thing = searchSet[0];
            Thing result = GenClosest.ClosestThing_Global(position, searchSet, 9999f, validator, null);
            tmpCandidates.Clear();
            return result;
        }

        protected override void GetSearchSet(Pawn pawn, List<Thing> outCandidates)
        {
            outCandidates.Clear();
            if (this.def.thingDefs == null)
            {
                return;
            }
            if (this.def.thingDefs.Count == 1)
            {
                outCandidates.AddRange(pawn.Map.listerThings.ThingsOfDef(this.def.thingDefs[0]));
            }
            else
            {
                for (int i = 0; i < this.def.thingDefs.Count; i++)
                {
                    ThingDef thingDef = this.def.thingDefs[i];
                    List<Thing> thingsFound = pawn.Map.listerThings.ThingsOfDef(thingDef);
                    outCandidates.AddRange(thingsFound);
                }
            }
        }

        protected override bool CanInteractWith(Pawn pawn, Thing t, bool inBed)
        {
            //jcLog.Warning(pawn.Name + " Can Pawn interact with thing ??");
            if (!CanInteractCheck(pawn, t, inBed))
            {
                //jcLog.Warning(pawn.Name + " No :(");
                return false;
            }
            if (inBed)
            {
                //jcLog.Warning(pawn.Name + " In Bed");
                Building_Bed bed = pawn.CurrentBed();
                if (t.def.building.isEdifice)
                {
                    //jcLog.Warning(pawn.Name + " is ediface");
                    return WatchBuildingUtility.CanWatchFromBed(pawn, bed, t);
                } else
                {
                    //jcLog.Warning(pawn.Name + " is not ediface");
                    return WatchWallBuildingUtility.CanWatchFromBed(pawn, bed, t);
                }
            }
            return true;
        }

        private bool CanInteractCheck(Pawn pawn, Thing t, bool inBed)
        {
            //jcLog.Warning(pawn.Name + " is building ediface ?");
            if (t.def.building.isEdifice)
            {
                //jcLog.Warning(pawn.Name + " yes");
                return base.CanInteractWith(pawn, t, inBed);
            }
            else
            {
                //jcLog.Warning(pawn.Name + " no");
                return CanInteractWithCheck(pawn, t, inBed);
            }
        }

        private bool CanInteractWithCheck(Pawn pawn, Thing t, bool inBed)
        {
            //jcLog.Warning(pawn.Name + " Can Pawn reserve thing ?");
            if (!pawn.CanReserve(t, this.def.jobDef.joyMaxParticipants, -1, null, false))
            {
                return false;
            }
            //jcLog.Warning(pawn.Name + " Is thing forbidden ?");
            if (t.IsForbidden(pawn))
            {
                return false;
            }
            //jcLog.Warning(pawn.Name + " Is it socially proper ?");
            if (!t.IsSociallyProper(pawn))
            {
                return false;
            }
            //jcLog.Warning(pawn.Name + " Politically ?");
            if (!t.IsPoliticallyProper(pawn))
            {
                return false;
            }
            CompPowerTrader compPowerTrader = t.TryGetComp<CompPowerTrader>();
            //jcLog.Warning("1" + (compPowerTrader == null));
            //jcLog.Warning("2" + (compPowerTrader.PowerOn));
            //jcLog.Warning("3" + (!this.def.unroofedOnly));
            //jcLog.Warning("4" + (!WatchWallBuildingUtility.GetAdjustedCenter(t).Roofed(t.Map)));
            //jcLog.Warning("5" + ((compPowerTrader == null || compPowerTrader.PowerOn)));
            //jcLog.Warning("6" + ((!this.def.unroofedOnly || !WatchWallBuildingUtility.GetAdjustedCenter(t).Roofed(t.Map))));
            return (compPowerTrader == null || compPowerTrader.PowerOn) && (!this.def.unroofedOnly || !WatchWallBuildingUtility.GetAdjustedCenter(t).Roofed(t.Map));
        }

        protected override Job TryGivePlayJob(Pawn pawn, Thing t)
        {
            //jcLog.Warning(pawn.Name + " Try Give play job");
            IntVec3 c;
            Building t2;
            if (t.def.building.isEdifice)
            {
                //jcLog.Warning(pawn.Name + " It's An Ediface");
                if (!WatchWallBuildingUtility.TryFindBestWatchCell(t, pawn, this.def.desireSit, out c, out t2))
                {
                    return null;
                }
            } else
            {
                //jcLog.Warning(pawn.Name + " It's NOT An Ediface");
                if (!WatchBuildingUtility.TryFindBestWatchCell(t, pawn, this.def.desireSit, out c, out t2))
                {
                    return null;
                }
            }
            //jcLog.Warning(pawn.Name + " Return a job !");
            return new Job(this.def.jobDef, t, c, t2);
        }
    }
}
