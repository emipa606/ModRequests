using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Kraltech_Industries;

public class GameCondition_AntiOrgToxicFallout : GameCondition
{
    private const float MaxSkyLerpFactor = 0.5f;

    private const float SkyGlow = 0.85f;

    public const int CheckInterval = 3451;

    private const float ToxicPerDay = 0.9f;

    private const float PlantKillChance = 0.0065f;

    private const float CorpseRotProgressAdd = 3000f;

    private readonly List<SkyOverlay> overlays = new()
    {
        new WeatherOverlay_Fallout()
    };

    private readonly SkyColorSet ToxicFalloutColors = new(new ColorInt(160, 32, 240).ToColor, new ColorInt(234, 200, 255).ToColor, new Color(138f, 43f, 226f), 2f);

    public override int TransitionTicks => 5000;

    public override void Init()
    {
        LessonAutoActivator.TeachOpportunity(ConceptDefOf.ForbiddingDoors, OpportunityType.Critical);
        LessonAutoActivator.TeachOpportunity(ConceptDefOf.AllowedAreas, OpportunityType.Critical);
    }

    public override void GameConditionTick()
    {
        var affectedMaps = AffectedMaps;
        if (Find.TickManager.TicksGame % 3451 == 0)
            for (var i = 0; i < affectedMaps.Count; i++)
                DoPawnsToxicDamage(affectedMaps[i]);
        for (var j = 0; j < overlays.Count; j++)
        for (var k = 0; k < affectedMaps.Count; k++)
            overlays[j].TickOverlay(affectedMaps[k]);
    }

    private void DoPawnsToxicDamage(Map map)
    {
        var allPawnsSpawned = map.mapPawns.AllPawnsSpawned;
        for (var i = 0; i < allPawnsSpawned.Count; i++) DoPawnToxicDamage(allPawnsSpawned[i]);
    }

    public static void DoPawnToxicDamage(Pawn p, bool protectedByRoof = true, float extraFactor = 1f)
    {
        if (!p.Spawned || !protectedByRoof || !p.Position.Roofed(p.Map))
        {
            var num = 0.023006668f;
            num *= Mathf.Max(1f - p.GetStatValue(StatDefOf_NanitesEX.AntiOrgToxinResistance), 0f);
            if (ModsConfig.BiotechActive) num *= Mathf.Max(1f - p.GetStatValue(StatDefOf_NanitesEX.AntiOrgToxinEnvironmentResistance), 0f);
            num *= extraFactor;
            if (num != 0f)
            {
                var num2 = Mathf.Lerp(0.85f, 1.15f, Rand.ValueSeeded(p.thingIDNumber ^ 74374237));
                num *= num2;
                HealthUtility.AdjustSeverity(p, HediffDefOf.AntiOrganicToxin, num);
            }
        }
    }

    public override void DoCellSteadyEffects(IntVec3 c, Map map)
    {
        if (!c.Roofed(map))
        {
            var thingList = c.GetThingList(map);
            for (var i = 0; i < thingList.Count; i++)
            {
                var thing = thingList[i];
                if (thing is Plant)
                {
                    if (thing.def.plant.dieFromToxicFallout && Rand.Value < 0.0065f) thing.Kill();
                }
                else if (thing.def.category == ThingCategory.Item)
                {
                    var compRottable = thing.TryGetComp<CompRottable>();
                    if (compRottable != null && compRottable.Stage < RotStage.Dessicated) compRottable.RotProgress += 3000f;
                }
            }
        }
    }

    public override void GameConditionDraw(Map map)
    {
        for (var i = 0; i < overlays.Count; i++) overlays[i].DrawOverlay(map);
    }

    public override float SkyTargetLerpFactor(Map map)
    {
        return GameConditionUtility.LerpInOutValue(this, TransitionTicks, 0.5f);
    }

    public override SkyTarget? SkyTarget(Map map)
    {
        return new SkyTarget(0.85f, ToxicFalloutColors, 1f, 1f);
    }

    public override float AnimalDensityFactor(Map map)
    {
        return 0f;
    }

    public override float PlantDensityFactor(Map map)
    {
        return 0f;
    }

    public override bool AllowEnjoyableOutsideNow(Map map)
    {
        return false;
    }

    public override List<SkyOverlay> SkyOverlays(Map map)
    {
        return overlays;
    }
}