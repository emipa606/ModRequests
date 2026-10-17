using HarmonyLib;
using System;
using System.Reflection;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace SR.ModRimWorld.RaidExtension.Util
{
public class HideRaidStrategy
{
    private delegate bool Func();
    private static MethodInfo info = AccessTools.Method( "HideRaidStrategy.HideRaidStrategyMod:IsMaskRaidLikeEventsEnabled" );
    private static Func func = info != null ? AccessTools.MethodDelegate< Func >( info, null ) : null;

    private delegate void LetterFunc(IncidentWorker worker, IncidentParms parms, List<Pawn> pawns);
    private static MethodInfo letterInfo = AccessTools.Method( "HideRaidStrategy.HideRaidStrategyMod:SendLetter" );
    private static LetterFunc letterFunc = letterInfo != null ? AccessTools.MethodDelegate< LetterFunc >( letterInfo, null ) : null;

    private delegate void RegisterFunc(Type eventType);
    private static MethodInfo registerInfo = AccessTools.Method( "HideRaidStrategy.HideRaidStrategyMod:RegisterRaidLikeEvent" );
    private static RegisterFunc registerFunc = registerInfo != null ? AccessTools.MethodDelegate< RegisterFunc >( registerInfo, null ) : null;

    public static bool Enabled => func != null && func();

    public static void SendLetter(IncidentWorker worker, IncidentParms parms, List<Pawn> pawns)
    {
        if( letterFunc != null )
            letterFunc(worker, parms, pawns);
    }

    public static void RegisterRaidLikeEvent(Type type)
    {
        if( registerFunc != null )
            registerFunc( type );
    }
}

}
