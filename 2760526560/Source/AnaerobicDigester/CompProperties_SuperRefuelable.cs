using RimWorld;

namespace AnaerobicDigester
{
    public class CompProperties_SuperRefuelable : CompProperties_Refuelable
    {
        public bool isSuperRefuelable = true;
        
        public CompProperties_SuperRefuelable() => this.compClass = typeof(CompRefuelable);
    }
}