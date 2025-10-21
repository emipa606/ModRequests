using System.Collections.Generic;
using UnityEngine;
using Verse;
using RimWorld;

namespace zed_0xff.CPS;

[StaticConstructorOnStartup]
public abstract class Gizmo_Resource : Gizmo
{
	protected IResourceStore store;

	private Texture2D barTex;
	private Texture2D barHighlightTex;

	protected Rect barRect;
	protected bool draggableBar;
	private static bool draggingBar;
	private float targetValuePct;
	private const float Spacing = 8f;

	private static readonly Texture2D EmptyBarTex = SolidColorMaterials.NewSolidColorTexture(GenUI.FillableBar_Empty);
	private static readonly Texture2D ResourceTargetTex = SolidColorMaterials.NewSolidColorTexture(Color.white);

	protected virtual float Width => 160f;

    protected abstract Color BarColor { get; }
    protected abstract Color BarHighlightColor { get; }

	public Gizmo_Resource(IResourceStore store)
	{
		this.store = store;
		barTex = SolidColorMaterials.NewSolidColorTexture(BarColor);
		barHighlightTex = SolidColorMaterials.NewSolidColorTexture(BarHighlightColor);
		Order = -100f;
		targetValuePct = Mathf.Clamp(store.TargetValue / store.Max, 0f, store.Max - store.MaxLevelOffset);
	}

	public override float GetWidth(float maxWidth)
	{
		return Width;
	}

	public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
	{
		Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
		Rect rect2 = rect.ContractedBy(8f);
		Widgets.DrawWindowBackground(rect);
		Text.Font = GameFont.Small;
		Rect labelRect = rect2;
		labelRect.height = Text.LineHeight;
		bool mouseOverAnyHighlightableElement = false;
		DrawLabel(labelRect, ref mouseOverAnyHighlightableElement);
		rect2.yMin += labelRect.height + 8f;
		barRect = rect2;
		if (!draggableBar)
		{
			Widgets.FillableBar(barRect, store.ValuePercent, barTex, EmptyBarTex, doBorder: true);
			if (!store.resourceGizmoThresholds.NullOrEmpty())
			{
				for (int i = 0; i < store.resourceGizmoThresholds.Count; i++)
				{
					float num = store.resourceGizmoThresholds[i];
					Rect position = default(Rect);
					position.x = barRect.x + 3f + (barRect.width - 8f) * num;
					position.y = barRect.y + barRect.height - 9f;
					position.width = 2f;
					position.height = 6f;
					GUI.DrawTexture(position, (store.Value < num) ? BaseContent.GreyTex : BaseContent.BlackTex);
				}
			}
		}
		else
		{
			Widgets.DraggableBar(barRect, barTex, barHighlightTex, EmptyBarTex, ResourceTargetTex, ref draggingBar, store.ValuePercent, ref targetValuePct, store.resourceGizmoThresholds, store.MaxForDisplay);
			targetValuePct = Mathf.Clamp(targetValuePct, 0f, (store.Max - store.MaxLevelOffset) / store.Max);
			store.SetTargetValuePct(targetValuePct);
		}
		int valueForDisplay = store.ValueForDisplay;
		string label = string.Concat(arg2: store.MaxForDisplay, arg0: valueForDisplay, arg1: " / ");
		Text.Anchor = TextAnchor.MiddleCenter;
		Widgets.Label(barRect, label);
		Text.Anchor = TextAnchor.UpperLeft;
		if (Mouse.IsOver(rect) && !mouseOverAnyHighlightableElement)
		{
			Widgets.DrawHighlight(rect);
			string tip = GetTooltip();
			if (!tip.NullOrEmpty())
			{
				TooltipHandler.TipRegion(rect, () => tip, Gen.HashCombineInt(store.GetHashCode(), 17626387));
			}
		}
		return new GizmoResult(GizmoState.Clear);
	}

	protected virtual void DrawLabel(Rect labelRect, ref bool mouseOverAnyHighlightableElement)
	{
		string text = store.ResourceLabel.CapitalizeFirst();
		text = text.Truncate(labelRect.width);
		Widgets.Label(labelRect, text);
	}

	protected abstract string GetTooltip();
}
