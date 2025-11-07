using RimWorld;
using Verse;

namespace BillDoorsFramework
{
    public class CompUseEffect_SendCompSignal : CompUseEffect
    {
        public CompProperties_Useable_SendCompSignal Props => (CompProperties_Useable_SendCompSignal)props;
        public override void DoEffect(Pawn usedBy)
        {
            base.DoEffect(usedBy);
            parent.BroadcastCompSignal(Props.signal);
        }
    }

    public class CompProperties_Useable_SendCompSignal : CompProperties_UseEffect
    {
        public CompProperties_Useable_SendCompSignal()
        {
            compClass = typeof(CompUseEffect_SendCompSignal);
        }

        public string signal;
    }
}
