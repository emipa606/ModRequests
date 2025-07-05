using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimWorld
{
    public class Hediff_BandNodeStellar_GAOTCE_Mech : Hediff
    {
        public override HediffStage CurStage
        {
            get
            {
                if (curStage == null && cachedTunedStellarBandNodesCount > 0)
                {
                    StatModifier statModifier = new StatModifier();
                    statModifier.stat = StatDefOf.MechBandwidth;
                    statModifier.value = AdditionalBandwidth;
                    curStage = new HediffStage();
                    curStage.statOffsets = new List<StatModifier> { statModifier };
                }

                return curStage;
            }
        }
        public int AdditionalBandwidth => cachedTunedStellarBandNodesCount * bandwidthPerNode;
        public override bool ShouldRemove => cachedTunedStellarBandNodesCount == 0;

        private const int BandNodeCheckInterval = 60;
        private int cachedTunedStellarBandNodesCount;
        private HediffStage curStage;
        private int bandwidthPerNode = 4;



        public override void PostTick()
        {
            base.PostTick();
            if (pawn.IsHashIntervalTick(60))
            {
                RecacheBandNodes();
            }
        }
        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);
            RecacheBandNodes();
        }
        public void RecacheBandNodes()
        {
            int num = cachedTunedStellarBandNodesCount;
            cachedTunedStellarBandNodesCount = 0;
            List<Map> maps = Find.Maps;
            for (int i = 0; i < maps.Count; i++)
            {
                foreach (Building item in maps[i].listerBuildings.AllBuildingsColonistOfDef(GreenDefOf.BandNodeStellar_GAOTCE_Mech))
                {
                    if (item.TryGetComp<CompBandNode>().tunedTo == pawn && item.TryGetComp<CompPowerTrader>().PowerOn)
                    {
                        cachedTunedStellarBandNodesCount++;
                    }
                }
            }
            if (num != cachedTunedStellarBandNodesCount)
            {
                curStage = null;
                pawn.mechanitor?.Notify_BandwidthChanged();
            }
        }
    }
}
