using UnityEngine;
using Verse;

namespace SettlementQuests
{
    [StaticConstructorOnStartup]
    public class Command_ActionWithCooldown : Command_Action
    {
        private static readonly Texture2D cooldownBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(Color.grey.r, Color.grey.g, Color.grey.b, 0.6f));
        private int lastUsedTick;
        private int cooldownTicks;
        public Command_ActionWithCooldown(int lastUsedTick, int cooldownTicks)
        {
            this.lastUsedTick = lastUsedTick;
            this.cooldownTicks = cooldownTicks;
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
            GizmoResult result = base.GizmoOnGUI(topLeft, maxWidth, parms);
            if (this.lastUsedTick > 0)
            {
                var cooldownTicksRemaining = Find.TickManager.TicksGame - this.lastUsedTick;
                if (cooldownTicksRemaining < this.cooldownTicks)
                {
                    float num = Mathf.InverseLerp(this.cooldownTicks, 0, cooldownTicksRemaining);
                    Widgets.FillableBar(rect, Mathf.Clamp01(num), cooldownBarTex, null, doBorder: false);
                }
            }
            if (result.State == GizmoState.Interacted)
            {
                return result;
            }
            return new GizmoResult(result.State);
        }
    }
}
