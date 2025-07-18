using System.Linq;
using RimWorld;
using Verse;

namespace Kraltech_Industries;

public class HediffComp_CureAll : HediffComp
{
    private int ticksToHeal;

    public HediffCompProperties_CureAll Props => (HediffCompProperties_CureAll)props;

    public override void CompPostMake()
    {
        base.CompPostMake();
        ResetTicksToHeal();
    }

    private void ResetTicksToHeal()
    {
        ticksToHeal = Rand.Range(1, 2) * 2500;
    }

    public override void CompPostTick(ref float severityAdjustment)
    {
        ticksToHeal--;
        if (ticksToHeal <= 0)
        {
            TryHealRandomPermanentWound(Pawn, parent.LabelCap);
            ResetTicksToHeal();
        }
    }

    public static void TryHealRandomPermanentWound(Pawn pawn, string cause)
    {
        Hediff hediff;
        if ((from hd in pawn.health.hediffSet.hediffs
                where hd.IsPermanent() || hd.def.chronic || hd.def.displayWound
                select hd).TryRandomElement(out hediff))
        {
            HealthUtility.Cure(hediff);
            if (PawnUtility.ShouldSendNotificationAbout(pawn)) Messages.Message("MessagePermanentWoundHealed".Translate(cause, pawn.LabelShort, hediff.Label, pawn.Named("PAWN")), pawn, MessageTypeDefOf.PositiveEvent);
        }
    }

    public override void CompExposeData()
    {
        Scribe_Values.Look(ref ticksToHeal, "ticksToHeal");
    }

    public override string CompDebugString()
    {
        return "ticksToHeal: " + ticksToHeal;
    }
}