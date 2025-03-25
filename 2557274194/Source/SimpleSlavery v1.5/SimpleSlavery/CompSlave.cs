using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace SimpleSlaveryCollars
{
    class CompSlave : ThingComp
    {
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (parent == null)
            {
                yield break;
            }
            var pawn = parent as Pawn;

            if (pawn.IsSlaveOfColony && pawn.health.hediffSet.HasHediff(SS_HediffDefOf.Enslaved))
            {
                var hediff = SlaveUtility.GetEnslavedHediff(pawn);

                var shackleSlave = new Command_Toggle();
                shackleSlave.isActive = () => hediff.shackledGoal;
                shackleSlave.defaultLabel = "LabelWordShackle".Translate();
                shackleSlave.defaultDesc = "CommandDescriptionShackle".Translate(pawn.Name.ToStringShort);
                shackleSlave.toggleAction = () => hediff.shackledGoal = !hediff.shackledGoal;
                shackleSlave.alsoClickIfOtherInGroupClicked = true;
                shackleSlave.activateSound = SoundDefOf.Click;
                shackleSlave.icon = ContentFinder<Texture2D>.Get("UI/Commands/Shackle", true);
                yield return shackleSlave;
            }
        }
    }
}
