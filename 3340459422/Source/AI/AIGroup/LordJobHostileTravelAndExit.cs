// Modified by llunak, l.lunak@centrum.cz .

using RimWorld;
using Verse;
using Verse.AI.Group;

namespace SR.ModRimWorld.RaidExtension;

// LordJob_TravelAndExit with modifications.
public class LordJobHostileTravelAndExit : LordJob
{
    private IntVec3 travelDest;
    SurpriseTimer surpriseTimer = new SurpriseTimer();

    public LordJobHostileTravelAndExit()
    {
    }

    public LordJobHostileTravelAndExit(IntVec3 travelDest, bool isSurprise)
    {
        this.travelDest = travelDest;
        surpriseTimer.SetIsSurprise( isSurprise );
    }

    public override void ExposeData()
    {
        Scribe_Values.Look(ref travelDest, "travelDest");
        surpriseTimer.ExposeData();
    }

    public override StateGraph CreateGraph()
    {
        surpriseTimer.InitTimer();
        StateGraph stateGraph = new StateGraph();
        LordToil lordToilTravel = (stateGraph.StartingToil = stateGraph.AttachSubgraph(new LordJob_Travel(travelDest).CreateGraph()).StartingToil);
        LordToil_ExitMap lordToilExitMap = new LordToil_ExitMap();
        stateGraph.AddToil(lordToilExitMap);
        var transitionTravelToExitMap = new Transition(lordToilTravel, lordToilExitMap);
        var triggerArrived = new Trigger_Memo("TravelArrived");
        if( !surpriseTimer.IsSurprise )
            transitionTravelToExitMap.AddTrigger(triggerArrived);
        stateGraph.AddTransition(transitionTravelToExitMap);
        // Surprise attack.
        if( surpriseTimer.IsSurprise )
        {
            var faction = lord.faction;
            LordToil lordToilAttack = stateGraph.AttachSubgraph(new LordJob_AssaultColony(faction).CreateGraph()).StartingToil;
            Transition transitionAttack = new Transition(lordToilTravel, lordToilAttack);
            transitionAttack.AddTrigger(new Trigger_Custom( ( TriggerSignal signal ) => surpriseTimer.ActivateOn( signal )));
            transitionAttack.AddTrigger(triggerArrived);
            // LordJob_Travel adds Trigger_PawnHarmed, which takes precedence over the one here,
            // and so the caravan would prefer to just defend itself rather than start attack the moment it is attacked.
            // Remove the unsuitable trigger.
            foreach( Transition transition in stateGraph.transitions )
                if( transition.sources.Contains( lordToilTravel ))
                    transition.triggers.RemoveAll( x => x is Trigger_PawnHarmed );
            transitionAttack.AddTrigger(new Trigger_PawnHarmed(requireInstigatorWithSpecificFaction : Faction.OfPlayer));
            var label = "SrSurpriseAttackLabel".Translate(faction.Name);
            var letter = "SrSurpriseAttackLetter".Translate(faction.def.pawnsPlural, faction.Name,
                Faction.OfPlayer.def.pawnsPlural).CapitalizeFirst();
            transitionAttack.AddPreAction(new TransitionAction_Letter(label, letter, LetterDefOf.ThreatBig));
            transitionAttack.AddPostAction(new TransitionAction_EndAllJobs());
            transitionAttack.AddPostAction(new TransitionAction_WakeAll());
            transitionAttack.AddPostAction(new TransitionAction_Custom( () => Find.TickManager.slower.SignalForceNormalSpeedShort()));
            stateGraph.AddTransition(transitionAttack);
        }
        return stateGraph;
    }
}
