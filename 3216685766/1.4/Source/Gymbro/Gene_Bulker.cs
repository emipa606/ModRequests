using System;
using RimWorld;
using Verse;

namespace Gymbro;

public class Gene_Bulker : Gene
{
    public static Lazy<GeneDef> HulkGeneDef = new(() => DefDatabase<GeneDef>.GetNamed("Body_Hulk"));

    public int lastSkillLevel = 0;

    public SimpleCurve curve;

    public override void PostMake()
    {
        base.PostMake();
        curve = def.GetModExtension<CurveDefModExtension>()?.curve;
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref lastSkillLevel, "lastSkillLevel", 0);
        if (curve == null && (Scribe.mode == LoadSaveMode.ResolvingCrossRefs || Scribe.mode == LoadSaveMode.PostLoadInit))
        {
            curve = def.GetModExtension<CurveDefModExtension>()?.curve;
        }
    }

    public override void PostAdd()
    {
        base.PostAdd();
        TryChangeBodyType();
    }

    public override void Reset()
    {
        base.Reset();
        TryChangeBodyType();
    }

    public override void Tick()
    {
        base.Tick();
        if (!pawn.IsHashIntervalTick(3000)) return;
        TryChangeBodyType();
    }

    public void TryChangeBodyType()
    {
        curve ??= def.GetModExtension<CurveDefModExtension>()?.curve;
        int newSkillLevel = pawn.skills.GetSkill(SkillDefOf.Melee).GetUnclampedLevel();
        if (newSkillLevel <= lastSkillLevel) return;
        lastSkillLevel = newSkillLevel;
        if (pawn == null || curve == null || pawn.story.bodyType == BodyTypeDefOf.Hulk || !Rand.Chance(curve.Evaluate(lastSkillLevel))) return;
        pawn.story.bodyType = BodyTypeDefOf.Hulk;
        if (!(pawn.genes?.HasGene(HulkGeneDef.Value) ?? true))
        {
            pawn.genes.AddGene(HulkGeneDef.Value, xenogene: true);
        }
    }
}
