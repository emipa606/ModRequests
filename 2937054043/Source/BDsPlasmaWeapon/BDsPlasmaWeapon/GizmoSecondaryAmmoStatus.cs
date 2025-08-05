using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;
using BillDoorsFramework;

namespace BDsPlasmaWeapon
{
    [StaticConstructorOnStartup]
    public class GizmoSecondaryAmmoStatus : Gizmo
    {

        //Link
        public CompSecondaryAmmo compAmmo;

        private static Texture2D FullTex;
        private static Texture2D EmptyTex;
        private static Texture2D BGTex;
        private static bool inited = false;

        static GizmoSecondaryAmmoStatus()
        {

        }

        public string Label { get; private set; }
        public override float GetWidth(float maxWidth)
        {
            return 120;
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            if (!inited)
            {
                InitializeTextures();
            }

            Rect overRect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), Height);
            Widgets.DrawBox(overRect);
            GUI.DrawTexture(overRect, BGTex);

            Rect inRect = overRect.ContractedBy(6);

            // Ammo type
            Rect textRect = inRect;
            textRect.height = overRect.height / 2;
            Text.Font = GameFont.Tiny;
            Label = compAmmo.IsSecondaryAmmoSelected ? compAmmo.MainAmmoName : compAmmo.SecondaryAmmoName;
            Widgets.Label(textRect, Label);

            // Bar
            if (compAmmo.Props.secondaryAmmoProps.magazineSize > 0)
            {
                Rect barRect = inRect;
                barRect.yMin = overRect.y + overRect.height / 2f;
                float ePct = (float)compAmmo.ScondaryMagCount / compAmmo.ScondaryMagSize;
                Widgets.FillableBar(barRect, ePct, FullTex);
                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(barRect, compAmmo.ScondaryMagCount + " / " + compAmmo.ScondaryMagSize);
                Text.Anchor = TextAnchor.UpperLeft;
            }

            if (Mouse.IsOver(overRect))
            {
                TooltipHandler.TipRegion(overRect, "PLA_NotInUse".Translate());
            }

            return new GizmoResult(GizmoState.Clear);
        }

        public override bool GroupsWith(Gizmo other)
        {
            return other is GizmoSecondaryAmmoStatus;
        }

        private static void InitializeTextures()
        {
            if (FullTex == null)
                FullTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.2f, 0.2f, 0.24f));
            if (EmptyTex == null)
                EmptyTex = SolidColorMaterials.NewSolidColorTexture(Color.clear);
            if (BGTex == null)
                BGTex = ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true);
            inited = true;
        }
    }
}
