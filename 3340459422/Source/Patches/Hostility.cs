using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace SR.ModRimWorld.RaidExtension;

// Non-aggressive hostile travellers/caravans should not be considered an active threat.
[HarmonyPatch(typeof(GenHostility))]
public static class GenHostility_Patch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(IsActiveThreatTo))]
    public static bool IsActiveThreatTo(bool result, IAttackTarget target, Faction faction)
    {
        if(!result)
            return false;
        if(target.Thing is Pawn pawn && Hostility.IsNonAggressiveHostileTraveller( pawn ))
            return false;
        return true;
    }
}

// Friendly "raids", if they are present on the map for some reason, will search for hostile pawns,
// without checking IsActiveThreatTo(), so also disable them seeing non-aggressive hostile travellers/caravans
// as a threat for that case.
[HarmonyPatch(typeof(Pawn))]
public static class Pawn_Patch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(ThreatDisabled))]
    public static bool ThreatDisabled(bool result, Pawn __instance, IAttackTargetSearcher disabledFor)
    {
        if( result )
            return true;
        Pawn searcher = disabledFor?.Thing as Pawn;
        if( searcher != null && Hostility.IsFriendlyRaider( searcher ) && Hostility.IsNonAggressiveHostileTraveller( __instance ))
            return true;
        return false;
    }
}

public static class Hostility
{
    public static bool IsNonAggressiveHostileTraveller(Pawn pawn)
    {
        Lord lord = pawn.GetLord();
        if (lord == null )
            return false;
        if(( lord.LordJob is LordJobHostileTraderCaravanTravelAndExit
            || lord.LordJob is LordJobHostileTravelAndExit )
            && (pawn.mindState.duty == null || pawn.mindState.duty.def != RimWorld.DutyDefOf.AssaultColony))
        {
            return true;
        }
        return false;
    }

    public static bool IsFriendlyRaider(Pawn pawn)
    {
        Lord lord = pawn.GetLord();
        if (lord == null )
            return false;
        if( lord.LordJob is LordJob_AssistColony )
            return true;
        return false;
    }
}
