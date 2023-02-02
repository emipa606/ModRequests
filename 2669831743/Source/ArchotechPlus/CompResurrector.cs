using RimWorld;
using UnityEngine;
using Verse;

namespace ArchotechPlus
{
    public class CompResurrector : ThingComp
    {
        private const int TicksPerHour = 2500;
        
        private int _ticksToResurrection;
        public Corpse Corpse;

        public CompProperties_Resurrector Props => (CompProperties_Resurrector) props;

        public bool CorpseIsInContainer => Corpse.StoringThing() != null;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            _ticksToResurrection = ArchotechPlusSettings.ResurrectionRange.RandomInRange * TicksPerHour;
             if (parent.GetType() == typeof(Corpse))
            {
                Corpse = (Corpse)parent;
            }
            base.PostSpawnSetup(respawningAfterLoad);
        }

        public override void CompTick()
        {
            _ticksToResurrection--;
            if (_ticksToResurrection > 0)
            {
                return;
            }

            if (ResurrectionConditionsMet() && CorpseRemovedFromContainers())
            {
                MessageWasResurrectionSuccessful(true);
                ResurrectionUtility.ResurrectWithSideEffects(Corpse.InnerPawn);
            }
            parent.Destroy();
        }

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref _ticksToResurrection, "ticksToResurrection", 600);
            Scribe_References.Look(ref Corpse, "targetPawn");
            base.PostExposeData();
        }

        private bool CorpseRemovedFromContainers()
        {
            if (CorpseIsInContainer && Corpse.StoringThing().GetType().IsSubclassOf(typeof(Building_Casket)))
            {
                var container = (Building_Casket) Corpse.StoringThing();
                container.EjectContents();
            }
            if (!CorpseIsInContainer)
            {
                return true;
            }

            Debug.Log("Error: Corpse is in miscellaneous that could not be processed container.");
            return false;
        }
        
        private bool ResurrectionConditionsMet()
        {
            if (!Corpse.DestroyedOrNull() && Corpse.InnerPawn.health.hediffSet.HasHead)
            {
                return true;
            }

            MessageWasResurrectionSuccessful(false);
            return false;
        }

        private void MessageWasResurrectionSuccessful(bool successful)
        {
            if (successful)
            {
                Messages.Message(
                    "ArchotechPlusSuccessfulResurrection".Translate((NamedArgument)parent.LabelCap,
                        (NamedArgument)Corpse.InnerPawn.LabelShort,
                        Corpse.InnerPawn.Named("PAWN")), Corpse, MessageTypeDefOf.PositiveEvent);
            }
            else
            {
                Messages.Message(
                    "ArchotechPlusFailedResurrection".Translate((NamedArgument)parent.LabelCap,
                        (NamedArgument)Corpse.InnerPawn.LabelShort,
                        Corpse.InnerPawn.Named("PAWN")), Corpse, MessageTypeDefOf.PositiveEvent);
            }
        }
    }
}