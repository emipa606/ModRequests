using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace BillDoorsFramework
{
    [Obsolete]
    internal class CompPawnEquipmentGizmo : ThingComp
    {
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            Pawn parentpawn = parent as Pawn;
            if (parentpawn == null && Find.Selector.SingleSelectedThing == parentpawn && parentpawn.Drafted)
            {
                ThingWithComps thingWithComps = parentpawn.equipment.Primary;
                if (thingWithComps == null || thingWithComps.AllComps.NullOrEmpty())
                {
                    yield break;
                }
                IEnumerable<CompEquippedGizmo> comps = thingWithComps.GetComps<CompEquippedGizmo>();
                foreach (CompEquippedGizmo allComp in comps)
                {
                    foreach (Gizmo item in allComp.CompGetGizmosEquipped())
                    {
                        yield return item;
                    }
                }
            }
        }
    }

    [Obsolete]
    internal class CompPawnTurretDeployGizmo : ThingComp
    {
        static List<IntVec3> AcceptedCell(Pawn pawn)
        {
            return new List<IntVec3>() { pawn.Position + IntVec3.South, pawn.Position + IntVec3.North, pawn.Position + IntVec3.East, pawn.Position + IntVec3.West };
        }

        public static TargetingParameters TargetParam(Pawn pawn)
        {
            return new TargetingParameters
            {
                canTargetLocations = true,
                canTargetSelf = false,
                canTargetPawns = false,
                canTargetFires = false,
                canTargetBuildings = false,
                canTargetItems = false,
                validator = (TargetInfo x) => AcceptedCell(pawn).Contains(x.Cell) && x.Cell.GetEdifice(pawn.Map) == null
            };
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            Pawn parentpawn = parent as Pawn;
            if (parentpawn == null && Find.Selector.SingleSelectedThing == parentpawn && parentpawn.Drafted)
            {
                foreach (Thing thing in parentpawn.inventory.innerContainer)
                {
                    if (thing is MinifiedThingDeployable deployable)
                    {
                        Command_Target command_Target = new Command_Target
                        {
                            defaultLabel = deployable.InnerThing.Label,
                            targetingParams = TargetParam(parentpawn),
                            icon = deployable.InnerThing.def.GetUIIconForStuff(null),
                            action = delegate (LocalTargetInfo target)
                            {
                                deployable.Deploy(target.Cell, parentpawn);
                            }
                        };
                        yield return command_Target;
                    }
                }
            }
        }
    }

    internal class MinifiedThingDeployable : MinifiedThing
    {
        public bool Deploy(IntVec3 cell, Pawn workerPawn)
        {
            workerPawn.rotationTracker.Face(cell.ToVector3Shifted());

            foreach (IntVec3 item in GenAdj.OccupiedRect(cell, workerPawn.Rotation, InnerThing.def.size))
            {
                if (item.GetEdifice(workerPawn.Map) != null)
                {
                    Messages.Message("BD_MinifiedDeployable_Selected_area_blocked".Translate(), workerPawn, MessageTypeDefOf.RejectInput, historical: false);
                    return false;
                }
            }

            Thing createdThing = InnerThing;
            Map map = workerPawn.Map;
            GenSpawn.WipeExistingThings(cell, workerPawn.Rotation, createdThing.def, map, DestroyMode.Deconstruct);
            if (!Destroyed)
            {
                Destroy();
            }
            if (createdThing.def.CanHaveFaction)
            {
                createdThing.SetFactionDirect(workerPawn.Faction);
            }
            Thing thing = GenSpawn.Spawn(createdThing, cell, map, workerPawn.Rotation, WipeMode.VanishOrMoveAside);
            Find.Selector.Deselect(workerPawn);
            Find.Selector.Select(thing, playSound: false, forceDesignatorDeselect: false);
            Job job = JobMaker.MakeJob(RimWorld.JobDefOf.ManTurret, thing);
            workerPawn.jobs.TryTakeOrderedJob(job, 0, true);
            return true;
        }
    }

    internal class CompMinifyToInventory : CompUseEffect
    {
        public override void DoEffect(Pawn usedBy)
        {
            Thing thing = parent;
            if (parent is Building building && building.def.Minifiable)
            {
                thing = building.MakeMinified();
            }
            if (thing.Spawned)
            {
                thing.DeSpawn();
            }
            usedBy.inventory.innerContainer.TryAddOrTransfer(thing);
        }
    }
}
