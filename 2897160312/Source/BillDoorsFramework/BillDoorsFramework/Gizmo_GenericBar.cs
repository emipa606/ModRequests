using UnityEngine;
using Verse;

namespace BillDoorsFramework
{
    public interface IGenericBar
    {
        float MaxFillableBarLength { get; }
        float CurrentFillableBarLength { get; }
        string FillableBarTitle { get; }

        Color? FillableBarColor { get; }
        Color? FillableBarBGColor { get; }
    }


    [StaticConstructorOnStartup]
    public class Gizmo_GenericBar : Gizmo
    {
        public IGenericBar mag;

        public bool displayDecimal;

        public float stageCount = 0;

        public float width = 140f;

        string toStringStyle => displayDecimal ? "F1" : "F0";

        public Gizmo_GenericBar()
        {
            Order = -100f;
        }

        public override float GetWidth(float maxWidth)
        {
            return width;
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            float actualLength = Mathf.Max(mag.CurrentFillableBarLength, 0);
            float pct = (float)actualLength / mag.MaxFillableBarLength;
            Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
            Rect rect2 = rect.ContractedBy(6f);
            Widgets.DrawWindowBackground(rect);
            Rect rect3 = rect2;
            rect3.height = rect.height / 2f;
            Text.Font = GameFont.Tiny;
            Widgets.Label(rect3, mag.FillableBarTitle);
            Rect barRect = rect2;
            barRect.yMin = rect2.y + rect2.height / 2f;
            float f = Mathf.Lerp(0, 0.33f, pct);

            FillableBarVariable(barRect, pct, mag.FillableBarColor ?? Color.HSVToRGB(f, 0.8f, 0.5f), mag.FillableBarBGColor ?? Color.HSVToRGB(f, 1, 0.3f), doBorder: false);

            if (stageCount > 1)
            {
                float pctDelta = 1 / stageCount;
                Log.Message(pctDelta);
                for (float thresholdPct = pctDelta; thresholdPct < 1; thresholdPct += pctDelta)
                {
                    DoBarThreshold(thresholdPct);
                }
            }

            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(barRect, actualLength.ToString(toStringStyle) + " / " + mag.MaxFillableBarLength.ToString(toStringStyle));
            Text.Anchor = TextAnchor.UpperLeft;

            return new GizmoResult(GizmoState.Clear);

            void DoBarThreshold(float percent)
            {
                Rect position = default;
                position.x = barRect.x + barRect.width * percent;
                position.y = barRect.y + barRect.height - 6f;
                position.width = 2f;
                position.height = 6f;
                GUI.DrawTexture(position, BaseContent.BlackTex);
            }
        }

        public static Rect FillableBarVariable(Rect rect, float fillPercent, Color fillTex, Color bgTex, bool doBorder)
        {
            if (doBorder)
            {
                GUI.DrawTexture(rect, BaseContent.BlackTex);
                rect = rect.ContractedBy(3f);
            }
            Color color2 = GUI.color;
            if (bgTex != null)
            {
                GUI.color = bgTex;
                GUI.DrawTexture(rect, BaseContent.WhiteTex);
            }
            Rect result = rect;
            rect.width *= fillPercent;
            GUI.color = fillTex;
            GUI.DrawTexture(rect, BaseContent.WhiteTex);
            GUI.color = color2;
            return result;
        }
    }
}
