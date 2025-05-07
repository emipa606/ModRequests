using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;

namespace DTechprinting
{
    class StatWorker_ShardTech : StatWorker
    {

        public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
        {
            if (req.HasThing && Base.thingDic.ContainsKey(req.Thing.def))
                return 0;
            return -1;
        }

        public override string GetExplanationUnfinalized(StatRequest req, ToStringNumberSense numberSense)
        {
            return "";
        }

        public override string GetExplanationFinalizePart(StatRequest req, ToStringNumberSense numberSense, float finalVal)
        {
            return "";
        }

        public override string GetStatDrawEntryLabel(StatDef stat, float value, ToStringNumberSense numberSense, StatRequest optionalReq, bool finalized = true)
        {
            if (optionalReq.HasThing && Base.thingDic.ContainsKey(optionalReq.Thing.def))
                return Base.thingDic[optionalReq.Thing.def].LabelCap;
            return "None";
        }
        
    }
}
