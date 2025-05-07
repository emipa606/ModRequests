using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;

namespace DTechprinting
{
    class StatWorker_ShardYield : StatWorker
    {

        public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
        {
            if(req.HasThing && Base.thingDic.ContainsKey(req.Thing.def))
                return Mathf.RoundToInt(ShardMaker.CalcNumShardsFor(req.Thing)/req.Thing.stackCount);
            return 0;
        }

        public override string GetExplanationUnfinalized(StatRequest req, ToStringNumberSense numberSense)
        {
            if (!req.HasThing || !Base.thingDic.ContainsKey(req.Thing.def))
                return "";
            ResearchProjectDef rpd = Base.thingDic[req.Thing.def];
            float baseVal = Mathf.RoundToInt(ShardMaker.GetAverageValue(req.Thing.def) / ShardMaker.RequiredValueForOneShard(rpd));
            
            string s = "Base item yield: " + baseVal;
            if (TechprintingSettings.shardYieldRatio != 1f)
            {
                s += "\nShard yield multiplier (from settings): " + TechprintingSettings.shardYieldRatio.ToString("p1");
            }

            QualityCategory q;
            if (req.Thing.TryGetQuality(out q)) 
            {
                float rawValue = (float)ShardMaker.CalcNumShardsForRaw(req.Thing, rpd);
                float multiplier = rawValue / baseVal;
                s += "\nQuality and condition multiplier: " + multiplier.ToString("p1");
                s += "\nAdjusted value: " + rawValue;
                if (rawValue > 100)
                {
                    int maxShards;
                    if (q == QualityCategory.Legendary || q == QualityCategory.Masterwork)
                    {
                        maxShards = TechprintingSettings.printAllItems ? Mathf.RoundToInt(Mathf.Max(rpd.techprintCount, 100)) : Mathf.RoundToInt(rpd.techprintCount);
                    }
                    else
                        maxShards = 100;
                    s += "\n\nMax shards for quality: " + maxShards;
                }
            }

            return s;
        }

        public override string GetStatDrawEntryLabel(StatDef stat, float value, ToStringNumberSense numberSense, StatRequest optionalReq, bool finalized = true)
        {
            return base.GetStatDrawEntryLabel(stat, value, numberSense, optionalReq, finalized);
        }
    }
}
