// Assembly-CSharp, Version=1.4.8706.7168, Culture=neutral, PublicKeyToken=null
// Verse.AI.Verb_CastTargetEffectLances
using RimWorld;
using Verse;
using Verse.AI;
using VFECore.Abilities;

public class VPEAC_Verb_CastTargetEffectLances_TargetValidator : Verb_CastTargetEffectLances
{

    public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
    {
        Pawn subject = target.Pawn;
        Pawn_RoyaltyTracker royalty = subject.royalty;
        var permits = royalty.factionPermits;

        for (int num = permits.Count - 1; num >= 0; num--)
        {
            if (permits[num].OnCooldown)
            {
                Messages.Message(TranslatorFormattedStringExtensions.Translate("VPEAC.TargetPermitsOnCooldown",
                                new NamedArgument(permits[num].faction, "angered")),
                                MessageTypeDefOf.CautionInput);
            }
        }

        if (royalty.factionPermits == null || royalty.factionPermits.Count == 0)
        {
            if (showMessages)
            {
                Messages.Message("VPEAC.TargetNoPermits".Translate(), MessageTypeDefOf.CautionInput);
            }
            return false;
        }

        return base.ValidateTarget(target, showMessages);
    }

}
