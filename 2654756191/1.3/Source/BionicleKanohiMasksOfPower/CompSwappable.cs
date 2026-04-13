using RimWorld;
using System.Collections.Generic;
using Verse;

namespace BionicleKanohiMasksOfPower
{

	public class CompProperties_Swappable : CompProperties//adds comp for items
    {
        public string swapGroupKey;
        public CompProperties_Swappable()
        {
            this.compClass = typeof(CompSwappable);
        }
    }

	public class CompSwappable : ThingComp
    {
        public CompProperties_Swappable Props => base.props as CompProperties_Swappable;
        public Apparel Apparel => this.parent as Apparel;
        public Pawn Wearer => Apparel.Wearer;
        public override IEnumerable<Gizmo> CompGetWornGizmosExtra()
        {
            foreach (var g in base.CompGetWornGizmosExtra())//adds gizmos
            {
                yield return g;
            }

            if (Wearer.IsColonistPlayerControlled)
            {
                foreach (var thing in Wearer.inventory.innerContainer)//checks inventory for swappable masks
                {
                    if (thing is Apparel apparel)
                    {
                        var compSwappable = apparel.GetComp<CompSwappable>();
                        if (compSwappable != null && compSwappable.Props.swapGroupKey == this.Props.swapGroupKey)
                        {
                            yield return new Command_Action
                            {
                                defaultLabel = apparel.LabelCap,
                                defaultDesc = apparel.def.description,
                                action = delegate
                                {
                                    if (!Find.Targeter.IsTargeting)
                                    {
                                        var pawn = Wearer;
                                        pawn.inventory.innerContainer.TryAddOrTransfer(this.parent);
                                        apparel.holdingOwner.Remove(apparel);
                                        pawn.apparel.Wear(apparel, false);
                                        pawn.health.hediffSet.DirtyCache();
                                        pawn.health.CheckForStateChange(null, null);
                                    }
                                },
                                icon = apparel.def.uiIcon,
                            };
                        }
                    }
                }
            }
        }
    }
}