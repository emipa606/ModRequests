using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace BillDoorsFramework
{
    public class Skyfaller_MedEvac : Skyfaller
    {
        public Thing intendedThing;

        public Pawn intendedPawn => intendedThing as Pawn;

        public SkyfallerMedEvacExtension extension => def.GetModExtension<SkyfallerMedEvacExtension>();
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref intendedThing, "intendedPawn", true);
        }

        protected override void SpawnThings()
        {
            if (extension == null || extension.returnShuttleFallerDef == null || extension.leaveShuttleFallerDef == null || extension.worldObjectDef == null)
            {
                Log.Error("SkyfallerMedEvacExtension Error for " + this.def.defName);
                return;
            }
            FlyShipLeaving_MedEvac leavingShip = (FlyShipLeaving_MedEvac)SkyfallerMaker.MakeSkyfaller(extension.leaveShuttleFallerDef);
            leavingShip.groupID = Rand.Int;
            GenSpawn.Spawn(leavingShip, Position, this.Map);
            if (intendedThing != null && intendedThing.Spawned)
            {
                Skyfaller_Custom landingShip = (Skyfaller_Custom)SkyfallerMaker.MakeSkyfaller(extension.returnShuttleFallerDef);
                landingShip.ticksToImpact = 114514;

                intendedThing.DeSpawnOrDeselect();
                landingShip.innerContainer.TryAdd(intendedThing);
                if (intendedPawn != null)
                {
                    IEnumerable<Hediff> wounds = intendedPawn.health.hediffSet.GetHediffsTendable();
                    foreach (Hediff hediff in wounds)
                    {
                        hediff.Tended(1, 1);
                    }
                }

                leavingShip.innerContainer.TryAdd(landingShip);

                if (intendedThing.Faction == Faction.OfPlayer || intendedPawn.IsColonist || intendedPawn.IsPrisonerOfColony)
                {
                    List<Map> maps = Find.Maps.Where((Map x) => x.IsPlayerHome && x.mapPawns.AnyFreeColonistSpawned).ToList();
                    if (maps.Any())
                    {
                        maps.SortBy((Map m) => { return -m.PlayerWealthForStoryteller; });

                        Map map = maps[0];
                        MapParent mapParent = map.Parent;
                        leavingShip.destinationTile = mapParent.Tile;

                        leavingShip.arrivalAction = new TransportPodsArrivalAction_MedEvacLand(mapParent, findBestLandingSpot(map));
                        leavingShip.worldObjectDef = extension.worldObjectDef;
                    }
                }
                else
                {
                    leavingShip.createWorldObject = false;
                }
            }
        }

        IntVec3 findBestLandingSpot(Map map)
        {
            IntVec3 cell;
            if (ModLister.CheckRoyalty("Shuttle") && ThingDefOf.ShipLandingBeacon != null)
            {
                DropCellFinder.TryFindShipLandingArea(map, out cell, out _);
                if (cell.InBounds(map))
                {
                    return cell;
                }
            }
            if ((cell = DropCellFinder.TradeDropSpot(map)) != null)
            {
                return cell;
            }
            if ((cell = DropCellFinder.TryFindSafeLandingSpotCloseToColony(map, extension.returnShuttleFallerDef.size, borderWidth: 10)) != null)
            {
                return cell;
            }
            if (DropCellFinder.TryFindDropSpotNear(map.Center, map, out cell, false, false))
            {
                return cell;
            }
            return map.Center;
        }
    }

    public class SkyfallerMedEvacExtension : DefModExtension
    {
        public ThingDef returnShuttleFallerDef;

        public float radius = 5;

        public WorldObjectDef worldObjectDef;

        public ThingDef leaveShuttleFallerDef;
    }

    public class FlyShipLeaving_MedEvac : Skyfaller
    {
        public int groupID = -1;

        public int destinationTile = -1;

        public TransportPodsArrivalAction arrivalAction;

        public bool createWorldObject = true;

        public WorldObjectDef worldObjectDef;

        private bool alreadyLeft;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref groupID, "groupID", 0);
            Scribe_Values.Look(ref destinationTile, "destinationTile", 0);
            Scribe_Deep.Look(ref arrivalAction, "arrivalAction");
            Scribe_Values.Look(ref alreadyLeft, "alreadyLeft", defaultValue: false);
            Scribe_Values.Look(ref createWorldObject, "createWorldObject", defaultValue: true);
            Scribe_Defs.Look(ref worldObjectDef, "worldObjectDef");
        }

        protected override void LeaveMap()
        {
            {
                if (alreadyLeft || !createWorldObject)
                {
                    if (innerContainer != null && innerContainer[0].TryGetInnerInteractableThingOwner() != null)
                    {
                        foreach (Thing item in innerContainer[0].TryGetInnerInteractableThingOwner())
                        {
                            if (item is Pawn pawn)
                            {
                                pawn.ExitMap(allowedToJoinOrCreateCaravan: false, Rot4.Invalid);
                            }
                        }
                        innerContainer[0].TryGetInnerInteractableThingOwner().ClearAndDestroyContentsOrPassToWorld(DestroyMode.QuestLogic);
                    }
                    Destroy();
                    return;
                }
                if (destinationTile < 0)
                {
                    Log.Error("Drop pod left the map, but its destination tile is " + destinationTile);
                    Destroy();
                    return;
                }
                Lord lord = TransporterUtility.FindLord(groupID, base.Map);
                if (lord != null)
                {
                    base.Map.lordManager.RemoveLord(lord);
                }
                TravelingTransportPods travelingTransportPods = (TravelingTransportPods)WorldObjectMaker.MakeWorldObject(worldObjectDef ?? WorldObjectDefOf.TravelingTransportPods);
                travelingTransportPods.Tile = base.Map.Tile;
                travelingTransportPods.SetFaction(Faction.OfPlayer);
                travelingTransportPods.destinationTile = destinationTile;
                travelingTransportPods.arrivalAction = arrivalAction;
                Find.WorldObjects.Add(travelingTransportPods);

                alreadyLeft = true;

                ActiveDropPodInfo ActiveDropPodInfo = new ActiveDropPodInfo();
                ActiveDropPodInfo.innerContainer.TryAddRangeOrTransfer(innerContainer, canMergeWithExistingStacks: true, destroyLeftover: true);
                travelingTransportPods.AddPod(ActiveDropPodInfo, justLeftTheMap: true);
                ActiveDropPodInfo = null;
                Destroy();
            }
        }

    }

    public class TransportPodsArrivalAction_MedEvacLand : TransportPodsArrivalAction
    {
        private MapParent mapParent;

        private IntVec3 cell;

        public TransportPodsArrivalAction_MedEvacLand()
        {
        }

        public TransportPodsArrivalAction_MedEvacLand(MapParent mapParent, IntVec3 cell)
        {
            this.mapParent = mapParent;
            this.cell = cell;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref mapParent, "mapParent");
            Scribe_Values.Look(ref cell, "cell");
        }

        public override FloatMenuAcceptanceReport StillValid(IEnumerable<IThingHolder> pods, int destinationTile)
        {
            FloatMenuAcceptanceReport floatMenuAcceptanceReport = base.StillValid(pods, destinationTile);
            if (!floatMenuAcceptanceReport)
            {
                return floatMenuAcceptanceReport;
            }
            if (mapParent != null && mapParent.Tile != destinationTile)
            {
                return false;
            }
            return CanLandInSpecificCell(pods, mapParent);
        }

        public override void Arrived(List<ActiveDropPodInfo> pods, int tile)
        {
            TransportPodsArrivalActionUtility.RemovePawnsFromWorldPawns(pods);
            for (int i = 0; i < pods.Count; i++)
            {
                if (pods[i].innerContainer.First() is Skyfaller skyfaller)
                {
                    GenSpawn.Spawn(skyfaller, cell, mapParent.Map);
                }
                else
                {
                    TransportPodsArrivalActionUtility.DropTravelingTransportPods(pods, cell, mapParent.Map);
                }
            }
        }

        public static bool CanLandInSpecificCell(IEnumerable<IThingHolder> pods, MapParent mapParent)
        {
            if (mapParent == null || !mapParent.Spawned || !mapParent.HasMap)
            {
                return false;
            }
            if (mapParent.EnterCooldownBlocksEntering())
            {
                return FloatMenuAcceptanceReport.WithFailMessage("MessageEnterCooldownBlocksEntering".Translate(mapParent.EnterCooldownTicksLeft().ToStringTicksToPeriod()));
            }
            return true;
        }
    }
}
