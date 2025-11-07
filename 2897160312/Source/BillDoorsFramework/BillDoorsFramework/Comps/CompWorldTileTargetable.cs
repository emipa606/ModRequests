using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace BillDoorsFramework
{
    [StaticConstructorOnStartup]
    public class CompWorldTileTargetable : ThingComp
    {
        public int destinationTile;

        public TransportPodsArrivalAction arrivalAction;

        public bool readyForPickUp => destinationTile > 0 && arrivalAction != null;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref destinationTile, "destinationTile", 0);
            Scribe_Deep.Look(ref arrivalAction, "arrivalAction");
        }

        public override string CompInspectStringExtra()
        {
            return destinationTile.ToString() + (arrivalAction != null).ToString();
        }

        public CompProperties_Durability Props => props as CompProperties_Durability;
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo item in base.CompGetGizmosExtra())
            {
                yield return item;
            }
            Command_Action command_Action = new Command_Action();
            command_Action.defaultLabel = "CommandLaunchGroup".Translate();
            command_Action.defaultDesc = "CommandLaunchGroupDesc".Translate();
            command_Action.icon = ContentFinder<Texture2D>.Get("UI/Commands/LaunchShip");
            command_Action.action = delegate
            {
                StartChoosingDestination();
            };
            if (parent.Position.Roofed(parent.Map))
            {
                command_Action.Disable("CommandLaunchGroupFailUnderRoof".Translate());
            }
            yield return command_Action;
        }

        public void StartChoosingDestination()
        {
            CameraJumper.TryJump(CameraJumper.GetWorldTarget(parent));
            Find.WorldSelector.ClearSelection();
            int tile = parent.Map.Tile;
            Find.WorldTargeter.BeginTargeting(ChoseWorldTarget, canTargetTiles: true, null, closeWorldTabWhenFinished: true);
        }

        private bool ChoseWorldTarget(GlobalTargetInfo target)
        {
            if (!target.IsValid)
            {
                Messages.Message("MessageTransportPodsDestinationIsInvalid".Translate(), MessageTypeDefOf.RejectInput, historical: false);
                return false;
            }
            if (target.WorldObject is MapParent mapParent)
            {
                IEnumerable<IThingHolder> pods = new List<IThingHolder>();
                pods.Append(parent as IThingHolder);
                foreach (FloatMenuOption shuttleFloatMenuOption in target.WorldObject.GetShuttleFloatMenuOptions(pods, confirm))
                {
                    shuttleFloatMenuOption.action();
                }
                return true;
            }
            return false;
        }

        private void confirm(int destinationTile, TransportPodsArrivalAction arrivalAction)
        {
            this.destinationTile = destinationTile;
            this.arrivalAction = arrivalAction;
        }
    }

    public class CompProperties_WorldTileTargetable : CompProperties
    {
        public CompProperties_WorldTileTargetable()
        {
            compClass = typeof(CompWorldTileTargetable);
        }
        public int time;
        public float chance;
        public bool melee;
        public bool range;
    }
}
