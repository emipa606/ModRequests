using BillDoorsUnifiedHaulJob;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.Sound;

namespace BillDoorsLootsAndShelves
{
    public class CompDisplayItemRequester : ThingComp, IRefuelable
    {
        Building_Locker locker => parent as Building_Locker;

        bool CanLoadNow => locker != null && (locker.slotGroup.HeldThingsCount < locker.MaxItems && locker.tempStorage.Count < locker.MaxItems);

        #region refuelableInterface
        public ThingRequest CurrentRequest => ThingRequest.ForGroup(ThingRequestGroup.HaulableEver);

        public int RequestAmount => 1;

        public bool ShouldRefuelNow => CanLoadNow && requestedThing != null && requestedThing.Spawned;

        public Predicate<Thing> FuelValidator => t => t == requestedThing;

        public float SearchRadius => 999;

        public int RefuelWaitTick => 0;

        public Thing FoundFuel { get => requestedThing; set => requestedThing = value; }

        public Thing Parent => parent;
        #endregion

        public Thing requestedThing;

        public void RefuelEffect(Thing fuel, Pawn pawn, params object[] parms)
        {
            if (locker != null)
            {
                if (locker.tempStorage.Any()) locker.Flick();
                GenPlace.TryPlaceThing(fuel, locker.AllSlotCells().RandomElement(), locker.Map, ThingPlaceMode.Direct, delegate
                {
                    locker.Flick();
                });
            }
            IRefuelableUtil.Deregister(parent.Map, this);
            requestedThing = null;
        }


        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (parent.Faction.IsPlayer && parent.Spawned && CanLoadNow)
            {
                string defaultLabel = (requestedThing == null ? "BDshelves_Button_SelectThing" : "BDshelves_Button_Cancel").Translate();
                string defaultDesc = (requestedThing == null ? "BDshelves_SelectThingDesc" : "BDshelves_CancelRequestDesc").Translate();
                Command_Action command_Action = new Command_Action
                {
                    defaultLabel = defaultLabel,
                    defaultDesc = defaultDesc,
                    icon = requestedThing == null ? TexCommand.SelectCarriedThing : TexCommand.CannotShoot,
                    action = delegate
                    {
                        if (requestedThing == null)
                        {
                            TargetingParameters parm = new TargetingParameters()
                            {
                                canTargetLocations = true,
                                canTargetBuildings = false,
                                canTargetPawns = false,

                            };

                            SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
                            Find.Targeter.BeginTargeting(parm, action: delegate (LocalTargetInfo target) { FloatMenuForCell(target.Cell); });
                        }
                        else
                        {
                            requestedThing = null;
                        }
                    }
                };
                yield return command_Action;
            }
        }

        void FloatMenuForCell(IntVec3 cell)
        {
            var list = haulableOnCell(cell);
            if (list.Any())
            {
                List<FloatMenuOption> list4 = new List<FloatMenuOption>();
                foreach (var i in list)
                {
                    list4.Add(new FloatMenuOption(i.Label, delegate
                    {
                        requestedThing = i;
                        IRefuelableUtil.SpawnRegister(this);
                    }));
                }
                Find.WindowStack.Add(new FloatMenu(list4));
            }
        }

        IEnumerable<Thing> haulableOnCell(IntVec3 cell)
        {
            return cell.GetThingList(parent.Map).Where(t => t.def.EverHaulable && !t.IsForbidden(Faction.OfPlayer));
        }

        public override string CompInspectStringExtra()
        {
            if (requestedThing != null)
            {
                return "BDshelves_RequestingItem".Translate(requestedThing.Label);
            }
            return "";
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            if (requestedThing != null)
            {
                IRefuelableUtil.SpawnRegister(this);
            }
        }

        public override void PostDeSpawn(Map map)
        {
            IRefuelableUtil.Deregister(map, this);
        }

        public override void PostExposeData()
        {
            Scribe_References.Look(ref requestedThing, "requestedThing");
        }

        public string GetUniqueLoadID()
        {
            return "CompDisplayRequester_" + parent.ThingID;
        }
    }
}
