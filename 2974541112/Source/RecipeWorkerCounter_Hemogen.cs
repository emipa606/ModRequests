using System.Collections.Generic;
using RimWorld;
using Verse;

namespace zed_0xff.CPS;

// count hemogen in VRE-S network
public class RecipeWorkerCounter_Hemogen : RecipeWorkerCounter {
    public override int CountProducts(Bill_Production bill) {
        int num = base.CountProducts(bill);
        if( bill?.billStack?.billGiver is Building_TSS tss ){
            num += tss.CountAllHemogen();
        }
        return num;
    }
}
