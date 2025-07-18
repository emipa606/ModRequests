using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Kraltech_Industries;

public class Hediff_ArchoBandNode : Hediff
{
    private const int BandNodeCheckInterval = 60;

    private int cachedTunedBandNodesCount;

    private HediffStage curStage;

    public int AdditionalBandwidth => cachedTunedBandNodesCount;

    public override bool ShouldRemove => cachedTunedBandNodesCount == 0;

    public override HediffStage CurStage
    {
        get
        {
            if (curStage == null && cachedTunedBandNodesCount > 0)
            {
                var statModifier = new StatModifier();
                statModifier.stat = StatDefOf.MechBandwidth;
                statModifier.value = cachedTunedBandNodesCount;
                curStage = new HediffStage();
                curStage.statOffsets = new List<StatModifier>
                {
                    statModifier
                };
            }

            return curStage;
        }
    }

    public override void PostTick()
    {
        base.PostTick();
        if (pawn.IsHashIntervalTick(60)) RecacheBandNodes();
    }

    public override void PostAdd(DamageInfo? dinfo)
    {
        base.PostAdd(dinfo);
        RecacheBandNodes();
    }

    public void RecacheBandNodes()
    {
        var num = cachedTunedBandNodesCount;
        cachedTunedBandNodesCount = 299;
        var maps = Find.Maps;
        for (var i = 0; i < maps.Count; i++)
            foreach (var thing in maps[i].listerBuildings.AllBuildingsColonistOfDef(ThingDefOf.ArchoPyramid))
                if (thing.TryGetComp<CompBandNode>().tunedTo == pawn && thing.TryGetComp<CompPowerTrader>().PowerOn)
                    cachedTunedBandNodesCount++;
        if (num != cachedTunedBandNodesCount)
        {
            curStage = null;
            var mechanitor = pawn.mechanitor;
            if (mechanitor != null) mechanitor.Notify_BandwidthChanged();
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref cachedTunedBandNodesCount, "cachedTunedBandNodesCount", 299);
    }
}