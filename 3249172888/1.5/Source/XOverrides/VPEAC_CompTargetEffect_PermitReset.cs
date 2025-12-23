// Assembly-CSharp, Version=1.4.8706.7168, Culture=neutral, PublicKeyToken=null
// RimWorld.CompTargetEffect_PsychicShock
using RimWorld;
using System.Security.Cryptography;
using Verse;
using Verse.Noise;
using VPEArchocaster;

public class VPEAC_CompTargetEffect_PermitReset : CompTargetEffect
{
    public override void DoEffectOn(Pawn user, Thing target)
    {

        Pawn subject = (Pawn)target;
        Pawn_RoyaltyTracker royalty = subject.royalty;
        var permits = royalty.factionPermits;
        if (!subject.Dead)
        {
            for (int num = permits.Count - 1; num >= 0; num--)
            {
                if (permits[num].OnCooldown)
                {
                    permits[num].faction.TryAffectGoodwillWith(target.Faction,
                        -(int)(VPEArchocasterMod_Settings.honorToGoodWillRatio * permits[num].Permit.royalAid.favorCost), true);
                }
            }

            foreach (Faction faction in Find.FactionManager.AllFactions){
                if (faction == null)
                {
                    continue;
                }
                if (royalty.favor.ContainsKey(faction))
                {
                    royalty.RefundPermits(0, faction);
                }
                
            }
        }

    }
}
