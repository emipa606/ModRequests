using Verse;

namespace ArchotechWeaponry.Comps
{
    public class CompProperties_LimitedUsage : CompProperties
    {
        public int initialCharges = 6;
        

        public CompProperties_LimitedUsage()
        {
            this.compClass = typeof(CompLimitedUsage);
        }
    }
    
    public class CompLimitedUsage : ThingComp
    {
        public CompProperties_LimitedUsage Props => (CompProperties_LimitedUsage)this.props;
        public int currentCharges;
        
        public void ConsumeCharge()
        {
            this.currentCharges--;
        }

        public override string CompInspectStringExtra()
        {
            return $"Remaining charges : {this.currentCharges}";
        }

        public override void Initialize(CompProperties properties)
        {
            base.Initialize(properties);
            this.currentCharges = ((CompProperties_LimitedUsage)properties).initialCharges;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref currentCharges, "currentCharges", Props.initialCharges);
        }
    }
}