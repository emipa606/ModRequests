using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.Sound;

namespace Kraltech_Industries;

public class GameCondition_Mjolnir_Thunderstorm : GameCondition
{
    private const int RainDisableTicksAfterConditionEnds = 30000;

    private const int AvoidConditionCauserExpandRect = 2;

    private static readonly IntRange AreaRadiusRange = new(45, 60);

    private static readonly IntRange TicksBetweenStrikes = new(320, 800);

    public bool ambientSound;

    private int areaRadius;

    public IntRange areaRadiusOverride = IntRange.zero;

    public bool avoidConditionCauser;

    public IntVec2 centerLocation = IntVec2.Invalid;

    public IntRange initialStrikeDelay = IntRange.zero;

    private int nextLightningTicks;

    private Sustainer soundSustainer;

    static GameCondition_Mjolnir_Thunderstorm()
    {
    }

    public int AreaRadius => areaRadius;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref centerLocation, "centerLocation");
        Scribe_Values.Look(ref areaRadius, "areaRadius");
        Scribe_Values.Look(ref areaRadiusOverride, "areaRadiusOverride");
        Scribe_Values.Look(ref nextLightningTicks, "nextLightningTicks");
        Scribe_Values.Look(ref initialStrikeDelay, "initialStrikeDelay");
        Scribe_Values.Look(ref ambientSound, "ambientSound");
        Scribe_Values.Look(ref avoidConditionCauser, "avoidConditionCauser");
    }

    public override void Init()
    {
        base.Init();
        areaRadius = areaRadiusOverride == IntRange.zero ? AreaRadiusRange.RandomInRange : areaRadiusOverride.RandomInRange;
        nextLightningTicks = Find.TickManager.TicksGame + initialStrikeDelay.RandomInRange;
        if (centerLocation.IsInvalid) FindGoodCenterLocation();
    }

    public override void GameConditionTick()
    {
        if (Find.TickManager.TicksGame > nextLightningTicks)
        {
            var vector = Rand.UnitVector2 * Rand.Range(0f, areaRadius);
            var intVec = new IntVec3((int)Math.Round(vector.x) + centerLocation.x, 0, (int)Math.Round(vector.y) + centerLocation.z);
            if (IsGoodLocationForStrike(intVec))
            {
                SingleMap.weatherManager.eventHandler.AddEvent(new WeatherEvent_Mjolnir_ThunderStrike(SingleMap, intVec));
                nextLightningTicks = Find.TickManager.TicksGame + TicksBetweenStrikes.RandomInRange;
            }
        }

        if (ambientSound)
        {
            if (soundSustainer == null || soundSustainer.Ended)
            {
                soundSustainer = SoundDefOf.FlashstormAmbience.TrySpawnSustainer(SoundInfo.InMap(new TargetInfo(centerLocation.ToIntVec3, SingleMap), MaintenanceType.PerTick));
                return;
            }

            soundSustainer.Maintain();
        }
    }

    public override void End()
    {
        SingleMap.weatherDecider.DisableRainFor(30000);
        base.End();
    }

    private void FindGoodCenterLocation()
    {
        if (SingleMap.Size.x <= 16 || SingleMap.Size.z <= 16) throw new Exception("Map too small for flashstorm.");
        for (var i = 0; i < 10; i++)
        {
            centerLocation = new IntVec2(Rand.Range(8, SingleMap.Size.x - 8), Rand.Range(8, SingleMap.Size.z - 8));
            if (IsGoodCenterLocation(centerLocation)) break;
        }
    }

    private bool IsGoodLocationForStrike(IntVec3 loc)
    {
        return loc.InBounds(SingleMap) && !loc.Roofed(SingleMap) && loc.Standable(SingleMap) && (!avoidConditionCauser || conditionCauser == null || !conditionCauser.OccupiedRect().ExpandedBy(2).Contains(loc));
    }

    private bool IsGoodCenterLocation(IntVec2 loc)
    {
        var num = 0;
        var num2 = (int)(3.1415927f * areaRadius * areaRadius / 2f);
        foreach (var loc2 in GetPotentiallyAffectedCells(loc))
        {
            if (IsGoodLocationForStrike(loc2)) num++;
            if (num >= num2) break;
        }

        return num >= num2;
    }

    private IEnumerable<IntVec3> GetPotentiallyAffectedCells(IntVec2 center)
    {
        int num;
        for (var x = center.x - areaRadius; x <= center.x + areaRadius; x = num)
        {
            for (var z = center.z - areaRadius; z <= center.z + areaRadius; z = num)
            {
                if ((center.x - x) * (center.x - x) + (center.z - z) * (center.z - z) <= areaRadius * areaRadius) yield return new IntVec3(x, 0, z);
                num = z + 1;
            }

            num = x + 1;
        }
    }
}