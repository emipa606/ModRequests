using System.Collections.Generic;

using RimWorld;
using Verse;

namespace SurvivalistsAdditions {

  public class ThoughtWorker_WearingBurlapApparel : ThoughtWorker {

    protected override ThoughtState CurrentStateInternal(Pawn p) {
      string text = null;
      int num = 0;
      List<Apparel> wornApparel = p.apparel.WornApparel;
      for (int i = 0; i < wornApparel.Count; i++) {
        if (wornApparel[i].Stuff == SrvDefOf.SRV_Burlap && (wornApparel[i].def.apparel.layers.Contains(ApparelLayerDefOf.OnSkin) || wornApparel[i].def.apparel.layers.Contains(ApparelLayerDefOf.Overhead))) {
          if (text == null) {
            text = wornApparel[i].def.label;
          }
          num++;
        }
      }
      if (num == 0) {
        return ThoughtState.Inactive;
      }
      if (num >= 4) {
        return ThoughtState.ActiveAtStage(3, text);
      }
      return ThoughtState.ActiveAtStage(num - 1, text);
    }
  }
}
