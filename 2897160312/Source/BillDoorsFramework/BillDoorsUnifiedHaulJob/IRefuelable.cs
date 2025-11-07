using System;
using Verse;
using Verse.Noise;

namespace BillDoorsUnifiedHaulJob
{
    public interface IRefuelable : ILoadReferenceable
    {
        //当前需要的物品的request 只需要单种def就是fordef
        ThingRequest CurrentRequest { get; }

        //理论上需要多少个材料
        int RequestAmount { get; }

        //现在是否需要填充 不必在里就搜索材料 可以晚点在workgiver里面再搜索
        bool ShouldRefuelNow { get; }

        Predicate<Thing> FuelValidator { get; }

        void RefuelEffect(Thing fuel, Pawn pawn, params object[] parms);

        float SearchRadius { get; }

        //填充的时候 需要读条等待的时间 为0会跳过这个toil
        int RefuelWaitTick { get; }

        //有时候会多次搜索 感觉很没有必要
        Thing FoundFuel { get; set; }

        //对应的thing。接口可能在thing上面也可能在comp上面。
        Thing Parent { get; }
    }
}
