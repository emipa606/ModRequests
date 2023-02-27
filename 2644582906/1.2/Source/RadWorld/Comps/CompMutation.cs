using System.Collections.Generic;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace RadWorld
{
	public class CompMutation : ThingComp
	{
        private Pawn Pawn => this.parent as Pawn;
        public override string CompInspectStringExtra()
        {
            var stringBuilder = new StringBuilder(base.CompInspectStringExtra());
            if (HasReloadableMask(out CompGasMaskReloadable compMask) && compMask.RemainingCharges > 0)
            {
                var remainingHours = compMask.GetRemainingHours();
                stringBuilder.AppendLine("RW.RemainingHours".Translate() + remainingHours.ToStringTicksToPeriod());
            };
            return stringBuilder.ToString().TrimEndNewlines();
        }

        private bool HasReloadableMask(out CompGasMaskReloadable compMask)
        {
            var pawn = Pawn;
            if (pawn.apparel.WornApparel != null)
            {
                foreach (var ap in pawn.apparel.WornApparel)
                {
                    var comp = ap.TryGetComp<CompGasMaskReloadable>();
                    if (comp != null)
                    {
                        compMask = comp;
                        return true;
                    }
                }
            }
            compMask = null;
            return false;
        }
    }
}