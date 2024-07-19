using RimWorld;
using Verse;

namespace FinishingTouchesByTheBestBuilder
{
    public class CompFinishingTouchPawn : ThingComp
    {
        //このTickは使わずにWorldComponentからやらせようか？　目的: 最小限の負荷で取り漏らしをなくしたい→ここでは登録　DataContextで定期的妥当性チェック
        public override void CompTick()
        {
            base.CompTick();
            if (this.parent.IsHashIntervalTick(CommonHelper.TICK_INTERVAL_COMP))
            {
                //Pawn個人の情報を更新
                DataContext.UpdatePawnState(this.parent as Pawn);
            }
        }
    }
}
