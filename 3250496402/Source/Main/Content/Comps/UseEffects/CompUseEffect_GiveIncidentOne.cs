using System.Linq;
using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace Kraltech_Industries;

public class CompUseEffect_GiveIncidentOne : CompUseEffect
{
    public CompProperties_UseEffectIncidentOne Props => (CompProperties_UseEffectIncidentOne)props;

    public override void DoEffect(Pawn user)
    {
        var slate = new Slate();
        slate.Set("points", StorytellerUtility.DefaultThreatPointsNow(user.Map));
        slate.Set("asker", user);
        if (Props.discovered != null) slate.Set("discovered", Props.discovered.Value);
        var parms = Find.Storyteller.storytellerComps.First(x => x is StorytellerComp_OnOffCycle || x is StorytellerComp_RandomMain).GenerateParms(IncidentCategoryDefOf.Misc, Find.CurrentMap);
        IncidentDefOf.MechanitorCargoShipCrashOne.Worker.TryExecute(parms);
    }
}