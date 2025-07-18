using RimWorld;
using UnityEngine;
using Verse;

namespace Kraltech_Industries;

[StaticConstructorOnStartup]
public class Gizmo_BossEnergyShieldStatus : Gizmo
{
    private static readonly Texture2D FullShieldBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.2f, 0.2f, 0.24f));

    private static readonly Texture2D EmptyShieldBarTex = SolidColorMaterials.NewSolidColorTexture(Color.clear);

    public CompBetterShield shield;

    static Gizmo_BossEnergyShieldStatus()
    {
    }

    public Gizmo_BossEnergyShieldStatus()
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
        Widgets.Label(rect3, shield.IsApparel ? shield.parent.LabelCap : "ShieldInbuilt".Translate().Resolve());
        var rect4 = rect2;
        rect4.yMin = rect2.y + rect2.height / 2f;
        var fillPercent = shield.Energy / Mathf.Max(1f, shield.parent.GetStatValue(StatDefOf.EnergyShieldEnergyMax));
        Widgets.FillableBar(rect4, fillPercent, FullShieldBarTex, EmptyShieldBarTex, false);
        Text.Font = GameFont.Small;
        Text.Anchor = TextAnchor.MiddleCenter;
        Widgets.Label(rect4, (shield.Energy * 100f).ToString("F0") + " / " + (shield.parent.GetStatValue(StatDefOf.EnergyShieldEnergyMax) * 100f).ToString("F0"));
        Text.Anchor = TextAnchor.UpperLeft;
        TooltipHandler.TipRegion(rect2, "ShieldPersonalTip".Translate());
        return new GizmoResult(GizmoState.Clear);
    }
}