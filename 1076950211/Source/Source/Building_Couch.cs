using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Therapy
{
    public class Building_Couch : Building
    {
        public Pawn CurOccupant => GetCurOccupant();
        public CompAssignableToPawn CompAssignableToPawn => GetComp<CompAssignableToPawn>();

        public override Color DrawColor => def.MadeFromStuff ? base.DrawColor : DrawColorTwo;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (respawningAfterLoad) return;

            foreach (var pawn in Map.mapPawns.FreeColonists)
            {
                CompAssignableToPawn.TryAssignPawn(pawn);
            }
        }

        public override string GetInspectString()
        {
            if (HasChair) return base.GetInspectString();
            return base.GetInspectString() + "\n" + "NeedsChair".Translate();
        }

        public bool HasChair => this.TryFindChairNearCouch(out _);

        public Pawn GetCurOccupant()
        {
            if (!Spawned)
            {
                return null;
            }
            var sleepingSlotPos = GetRestingSlotPos();
            var list = Map.thingGrid.ThingsListAt(sleepingSlotPos);
            return list.OfType<Pawn>().Where(pawn => pawn.CurJob != null).FirstOrDefault(pawn => pawn.jobs.posture == PawnPosture.LayingOnGroundFaceUp);
        }

        public bool MayUseThis(Pawn pawn)
        {
            if (CompAssignableToPawn.AssignedPawnsForReading.Count == 0) return true;
            return ExplicitlyAssignedTo(pawn);
        }

        public bool ExplicitlyAssignedTo(Pawn pawn)
        {
            return CompAssignableToPawn.AssignedAnything(pawn);
        }

        public IntVec3 GetRestingSlotPos()
        {
            var index = 0;
            CellRect cellRect = this.OccupiedRect();
            if (Rotation == Rot4.North)
            {
                return new IntVec3(cellRect.minX + index, Position.y, cellRect.minZ);
            }
            if (Rotation == Rot4.East)
            {
                return new IntVec3(cellRect.minX, Position.y, cellRect.maxZ - index);
            }
            if (Rotation == Rot4.South)
            {
                return new IntVec3(cellRect.minX + index, Position.y, cellRect.maxZ);
            }
            return new IntVec3(cellRect.maxX, Position.y, cellRect.maxZ - index);
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var gizmo in base.GetGizmos())
            {
                // Skip "Assign owner"
                if (gizmo is Command_Action action && action.hotKey == KeyBindingDefOf.Misc4) continue;
                yield return gizmo;
            }

            if (def.building.bed_humanlike && Faction == Faction.OfPlayer)
            {
                var commandAction = new Command_Action
                {
                    defaultLabel = "CommandThingSetPatientsLabel".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/Commands/AssignOwner"),
                    defaultDesc = "CommandBedSetPatientsDesc".Translate(),
                    action = delegate { Find.WindowStack.Add(new Dialog_AssignBuildingOwner(CompAssignableToPawn)); },
                    hotKey = KeyBindingDefOf.Misc4
                };
                yield return commandAction;
            }
        }
    }
}
