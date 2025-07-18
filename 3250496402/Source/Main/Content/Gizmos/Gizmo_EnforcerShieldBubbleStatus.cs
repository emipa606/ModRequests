using RimWorld;
using UnityEngine;
using Verse;

namespace Kraltech_Industries;

[StaticConstructorOnStartup]
public class Gizmo_EnforcerShieldBubbleStatus : Gizmo
{
    private static readonly Texture2D FullShieldBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.9f, 0.2f, 0.5f));

    private static readonly Texture2D EmptyShieldBarTex = SolidColorMaterials.NewSolidColorTexture(Color.clear);

    public Apparel_EnforcerShieldBubble shield;

    static Gizmo_EnforcerShieldBubbleStatus()
    {
    }

    public Gizmo_EnforcerShieldBubbleStatus()
    {
        Order = -100f;
    }

    public override float GetWidth(float maxWidth)
    {
        return 140f;
    }

    public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
    {
        var rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
        var rect2 = rect.ContractedBy(6f);
        Widgets.DrawWindowBackground(rect);
        var rect3 = rect2;
        rect3.height = rect.height / 2f;
        Text.Font = GameFont.Tiny;
        Widgets.Label(rect3, shield.LabelCap);
        var rect4 = rect2;
        rect4.yMin = rect2.y + rect2.height / 2f;
        var fillPercent = shield.Energy / Mathf.Max(1f, shield.GetStatValue(StatDefOf.EnergyShieldEnergyMax));
        Widgets.FillableBar(rect4, fillPercent, FullShieldBarTex, EmptyShieldBarTex, false);
        Text.Font = GameFont.Small;
        Text.Anchor = TextAnchor.MiddleCenter;
        var str = (shield.Energy * 100f).ToString("F0");
        var num = shield.GetStatValue(StatDefOf.EnergyShieldEnergyMax) * 100f;
        Widgets.Label(rect4, str + " / " + num.ToString("F0"));
        Text.Anchor = TextAnchor.UpperLeft;
        return new GizmoResult(GizmoState.Clear);
    }
}