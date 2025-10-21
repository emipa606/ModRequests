using RimWorld;
using Verse;
using DubsBadHygiene;

namespace zed_0xff.CPS;

public class Plugin_DBH : IPlugin {
    private CompPipe pipeComp = null;

    const float waterPerDay = 2.7f;

    public Plugin_DBH(ThingWithComps parent){
        pipeComp = parent.TryGetComp<CompPipe>();
    }

    public void ProcessPawn(Pawn pawn){
        checkThirst(pawn);
        checkBladder(pawn);
        checkHygiene(pawn);
    }

    void checkBladder(Pawn pawn){
        if( pipeComp == null ) return;

        Need_Bladder bladder = pawn.needs.TryGetNeed<Need_Bladder>();
        if( bladder == null || bladder.CurLevelPercentage >= 0.5 ) return;

        float urineAmount = (1.0f - bladder.CurLevelPercentage) * waterPerDay;
        if (pipeComp.pipeNet.PushSewage(urineAmount * 2)){
            bladder.dump();
            bladder.CurLevel = 1;
        }
        pipeComp.pipeNet.PullWater(urineAmount, out var _); // flush
    }

    void checkThirst(Pawn pawn){
        if( pipeComp == null ) return;

        Need_Thirst thirst = pawn.needs.TryGetNeed<Need_Thirst>();
        if( thirst == null || thirst.CurLevelPercentage >= 0.5 ) return;

        ContaminationLevel WaterTaint = ContaminationLevel.Treated;
        if (pipeComp.pipeNet.PullWater(waterPerDay * thirst.CurLevelPercentage, out WaterTaint) == true) {
            thirst.Drink(100.0f);
            SanitationUtil.ContaminationCheckDrinking(pawn, WaterTaint);
        }
    }

    void checkHygiene(Pawn pawn){
        if( pipeComp == null ) return;

        Need_Hygiene need = pawn.needs.TryGetNeed<Need_Hygiene>();
        if( need == null || need.CurLevelPercentage >= 0.75 ) return;
        if( !pipeComp.pipeNet.PullWater(1, out var _) ) return;

        need.CurLevel = 1;
    }

}
