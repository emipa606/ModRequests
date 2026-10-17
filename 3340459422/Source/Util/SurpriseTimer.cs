// Modified by llunak, l.lunak@centrum.cz .

using Verse;
using Verse.AI.Group;
using System;

namespace SR.ModRimWorld.RaidExtension;

public class SurpriseTimer
{
    private int ticksToSurpriseAttack = -1; // <0 - no surprise

    private static bool debug = false;

    public void SetIsSurprise( bool isSurprise )
    {
        if( !isSurprise )
            ticksToSurpriseAttack = -1;
        else
            ticksToSurpriseAttack = 1; // Will be adjusted by InitTimer().
    }

    public bool IsSurprise => ticksToSurpriseAttack >= 0;

    public void InitTimer( int maxTicks = 60000 )
    {
        if( ticksToSurpriseAttack < 0 )
            return;
        ticksToSurpriseAttack = Math.Min( maxTicks, (int)(2500f * Rand.Range(1f, 4f)));
    }

    public bool ActivateOn( TriggerSignal signal )
    {
        if( signal.type != TriggerSignalType.Tick )
            return false;
        if( debug && ticksToSurpriseAttack >= 0 )
        {
            ticksToSurpriseAttack = 10;
            debug = false;
        }
        if( ticksToSurpriseAttack < 0 )
            return false;
        if( ticksToSurpriseAttack > 0 )
            --ticksToSurpriseAttack;
        return ticksToSurpriseAttack == 0;
    }

    public void Delay( int delayTicks )
    {
        ticksToSurpriseAttack += delayTicks;
    }

    public void ExposeData()
    {
        Scribe_Values.Look(ref ticksToSurpriseAttack, "ticksToSurpriseAttack");
    }
}
