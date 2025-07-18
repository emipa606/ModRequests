using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Kraltech_Industries;

public class CompCauseGameCondition_SeverePsychicDrone : CompCauseGameCondition
{
    public Gender gender;

    public override void Initialize(CompProperties props)
    {
        base.Initialize(props);
        gender = Rand.Bool ? Gender.Male : Gender.Female;
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref gender, "gender");
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        if (!Prefs.DevMode || !DebugSettings.godMode) yield break;
        yield return new Command_Action
        {
            defaultLabel = gender.GetLabel(),
            action = delegate
            {
                if (gender == Gender.Female)
                    gender = Gender.Male;
                else
                    gender = Gender.Female;
                ReSetupAllConditions();
            },
            hotKey = KeyBindingDefOf.Misc1
        };
    }

    public override void CompTick()
    {
        base.CompTick();
        if (Active && MyTile != -1)
            foreach (var caravan in Find.World.worldObjects.Caravans)
                if (Find.WorldGrid.ApproxDistanceInTiles(caravan.Tile, MyTile) < Props.worldRange)
                    foreach (var pawn in caravan.pawns)
                        GameCondition_SeverePsychicDrone.CheckPawn(pawn, gender);
    }

    protected override void SetupCondition(GameCondition condition, Map map)
    {
        base.SetupCondition(condition, map);
        ((GameCondition_SeverePsychicDrone)condition).gender = gender;
    }

    public override string CompInspectStringExtra()
    {
        var text = base.CompInspectStringExtra();
        if (!text.NullOrEmpty()) text += "\n";
        return text + ("AffectedGender".Translate() + ": " + gender.GetLabel().CapitalizeFirst());
    }

    public override void RandomizeSettings(Site site)
    {
        gender = Rand.Bool ? Gender.Male : Gender.Female;
    }
}