using System.Collections.Generic;
using UnityEngine;
using RimWorld;
using Verse;
using Verse.Sound;

namespace zed_0xff.CPS;

[StaticConstructorOnStartup]
public class Gizmo_ResourceHemogen : Gizmo_Resource {

    protected override Color BarColor => new ColorInt(138, 3, 3).ToColor;
    protected override Color BarHighlightColor => new ColorInt(145, 42, 42).ToColor;

    public Gizmo_ResourceHemogen(IResourceStore store) : base(store) {
        draggableBar = true;
    }

    protected override void DrawLabel(Rect labelRect, ref bool mouseOverAnyHighlightableElement)
    {
        labelRect.xMax -= 24f;
        Rect rect = new Rect(labelRect.xMax, labelRect.y, 24f, 24f);
        Widgets.DefIcon(rect, ThingDefOf.HemogenPack);
        GUI.DrawTexture(new Rect(rect.center.x, rect.y, rect.width / 2f, rect.height / 2f), store.ResourceAllowed ? Widgets.CheckboxOnTex : Widgets.CheckboxOffTex);
        if (Widgets.ButtonInvisible(rect))
        {
            store.ResourceAllowed = !store.ResourceAllowed;
            if (store.ResourceAllowed)
            {
                SoundDefOf.Tick_High.PlayOneShotOnCamera();
            }
            else
            {
                SoundDefOf.Tick_Low.PlayOneShotOnCamera();
            }
        }
        if (Mouse.IsOver(rect))
        {
            Widgets.DrawHighlight(rect);
            string onOff = (store.ResourceAllowed ? "On" : "Off").Translate().ToString().UncapitalizeFirst();
            TooltipHandler.TipRegion(rect, () => "AutoDeliverHemogenDesc".Translate(store.PostProcessValue(store.TargetValue).Named("MIN"), onOff.Named("ONOFF")).Resolve(), 828267374);
            mouseOverAnyHighlightableElement = true;
        }
        base.DrawLabel(labelRect, ref mouseOverAnyHighlightableElement);
    }

    protected override string GetTooltip() {
        string text = $"{store.ResourceLabel.CapitalizeFirst().Colorize(ColoredText.TipSectionTitleColor)}: {store.ValueForDisplay} / {store.MaxForDisplay}\n";
        text = ((!(store.TargetValue <= 0f)) ? (text + (string)("DeliverHemogenBelow".Translate() + ": ") + store.PostProcessValue(store.TargetValue)) : (text + "NeverDeliverHemogen".Translate().ToString()));
        return text;
    }
}
