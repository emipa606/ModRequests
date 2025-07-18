using System.Linq;
using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace Kraltech_Industries;

public class CompUseEffect_GiveIncidentTwo : CompUseEffect
{
    public CompProperties_UseEffectIncidentTwo Props => (CompProperties_UseEffectIncidentTwo)props;

    public override void DoEffect(Pawn user)
    {
        var slate = new Slate();
        slate.Set("points", StorytellerUtility.DefaultThreatPointsNow(user.Map));
        slate.Set("asker", user);
        if (Props.discovered != null) slate.Set("discovered", Props.discovered.Value);
        var parms = Find.Storyteller.storytellerComps.First(x => x is StorytellerComp_OnOffCycle || x is StorytellerComp_RandomMain).GenerateParms(IncidentCategoryDefOf.Misc, Find.CurrentMap);
        IncidentDefOf.MechanitorCargoShipCrashTwo.Worker.TryExecute(parms);
    }
}