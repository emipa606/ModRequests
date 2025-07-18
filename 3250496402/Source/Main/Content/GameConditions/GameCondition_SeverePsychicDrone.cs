using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.Grammar;

namespace Kraltech_Industries;

public class GameCondition_SeverePsychicDrone : GameCondition
{
    public Gender gender;

    public override string LetterText => base.LetterText.Formatted(gender.GetLabel().ToLower());

    public override string Description => base.Description.Formatted(gender.GetLabel().ToLower());

    public override void Init()
    {
        base.Init();
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref gender, "gender");
    }

    public static void CheckPawn(Pawn pawn, Gender targetGender)
    {
        if (pawn.RaceProps.Humanlike && pawn.gender == targetGender && !pawn.health.hediffSet.HasHediff(HediffDefOf.SeverePsychicDrone)) pawn.health.AddHediff(HediffDefOf.SeverePsychicDrone);
    }

    public override void GameConditionTick()
    {
        foreach (var map in AffectedMaps)
        foreach (var pawn in map.mapPawns.AllPawns)
            CheckPawn(pawn, gender);
    }

    public override void RandomizeSettings(float points, Map map, List<Rule> outExtraDescriptionRules, Dictionary<string, string> outExtraDescriptionConstants)
    {
        base.RandomizeSettings(points, map, outExtraDescriptionRules, outExtraDescriptionConstants);
        if (map.mapPawns.FreeColonistsCount > 0)
            gender = map.mapPawns.FreeColonists.RandomElement().gender;
        else
            gender = Rand.Element(Gender.Male, Gender.Female);
        outExtraDescriptionRules.Add(new Rule_String("psychicSuppressorGender", gender.GetLabel()));
    }
}