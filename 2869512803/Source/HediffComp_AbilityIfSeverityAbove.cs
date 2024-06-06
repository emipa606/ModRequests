namespace VFECore.Abilities
{
    using System.Collections.Generic;
    using System.Linq;
    using Verse;

    public class HediffComp_AbilityIfSeverityAbove : HediffComp
    {
        //public override bool ShouldRemove => false;
		//this comp can also be used to give an ability with any severity, by just having minSeverity be 0
		//abilitydefs can allegedly have required hediffs with a minimum level
		public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            if (this.parent.Severity > Props.minSeverity)
			{
				CompAbilities comp = this.parent.pawn.GetComp<CompAbilities>();
				comp.GiveAbility(Props.ability);
			}
        }

        public virtual IEnumerable<Gizmo> DrawGizmos()
        {
            yield break;
        }
		
		public HediffCompProperties_AbilityIfSeverityAbove Props
        {
            get
            {
                return (HediffCompProperties_AbilityIfSeverityAbove)this.props;
            }
        }
    }
	public class HediffCompProperties_AbilityIfSeverityAbove : HediffCompProperties
	{
		public AbilityDef ability;
		public float minSeverity;
		public HediffCompProperties_AbilityIfSeverityAbove()
        {
            compClass = typeof(HediffComp_AbilityIfSeverityAbove);
        }
	}
}