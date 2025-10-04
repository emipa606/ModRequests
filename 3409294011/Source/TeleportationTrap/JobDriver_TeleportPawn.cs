using System.Collections.Generic;
using Verse;
using Verse.AI;
using RimWorld;
using UnityEngine;
using System.Linq;

namespace TeleportationTrap
{


    [DefOf]
    public static class JobDefOf
    {
        public static JobDef TeleportPawn;
    }



    public class JobDriver_TeleportPawn : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            var initialTarget = job.GetTarget(TargetIndex.A).Thing;
            if (initialTarget == null)
            {
                Messages.Message("Teleportation failed: Invalid target.", MessageTypeDefOf.RejectInput, false);
                yield break;
            }

            // Go to the target location
            yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.Touch);

            // Perform teleportation to the other linked object
            Toil teleport = new Toil
            {
                initAction = () =>
                {
                    if (initialTarget.TryGetComp<Comp_Linkable>() is Comp_Linkable linkableComp &&
                        linkableComp.LinkedThings.Count > 0)
                    {
                        Thing linkedThing = linkableComp.LinkedThings.FirstOrDefault(t => t != initialTarget);
                        if (linkedThing != null)
                        {
                            pawn.Position = linkedThing.Position;
                            pawn.Notify_Teleported();
                            Messages.Message($"{pawn.LabelShort} has been teleported to {linkedThing.LabelShort}.", MessageTypeDefOf.PositiveEvent, false);
                        }
                        else
                        {
                            Messages.Message("Teleportation failed: No valid linked object found.", MessageTypeDefOf.RejectInput, false);
                        }
                    }
                    else
                    {
                        Messages.Message("Teleportation failed: Linkable component not found.", MessageTypeDefOf.RejectInput, false);
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };

            yield return teleport;
        }
    }









}
