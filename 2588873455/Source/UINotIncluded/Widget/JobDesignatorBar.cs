using System.Collections.Generic;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace UINotIncluded.Widget
{
    public class JobDesignatorBar
    {
        private static readonly float posX = UI.screenWidth;
        private readonly int leftRows = 2;

        private readonly int mainRows = 1;
        private readonly int rigthRows = 2;

        public static List<Designator>[] Jobs => DesignatorManager.GetDesignationConfigs();

        public void JobDesignatorBarOnGUI()
        {
            if (!WorldRendererUtility.WorldBackgroundNow && WorldRendererUtility.WorldRendered) return;

            var rigthWidth = CustomGizmoGridDrawer.CalculateWidth(Jobs[(int)DesignationConfig.right], rigthRows);
            var mainWidth = CustomGizmoGridDrawer.CalculateWidth(Jobs[(int)DesignationConfig.main], mainRows);
            var leftWidth = CustomGizmoGridDrawer.CalculateWidth(Jobs[(int)DesignationConfig.left], leftRows);

            var curX = Settings.designationsOnLeft ? 15f + rigthWidth + mainWidth + leftWidth : posX;
            curX -= rigthWidth + mainWidth;

            Gizmo mousoverGizmo;
            if (Jobs[(int)DesignationConfig.main].Count > 0)
            {
                CustomGizmoGridDrawer.DrawGizmoGrid(Jobs[(int)DesignationConfig.main], mainRows, curX,
                    out mousoverGizmo);
                if (mousoverGizmo != null) DrawTooltip((Designator)mousoverGizmo, false);
            }

            curX -= leftWidth;

            if (Jobs[(int)DesignationConfig.left].Count > 0)
            {
                CustomGizmoGridDrawer.DrawGizmoGrid(Jobs[(int)DesignationConfig.left], leftRows, curX,
                    out mousoverGizmo);
                if (mousoverGizmo != null) DrawTooltip((Designator)mousoverGizmo, true);
            }

            curX += mainWidth + leftWidth;

            if (Jobs[(int)DesignationConfig.right].Count > 0)
            {
                CustomGizmoGridDrawer.DrawGizmoGrid(Jobs[(int)DesignationConfig.right], rigthRows, curX,
                    out mousoverGizmo, true);
                if (mousoverGizmo != null) DrawTooltip((Designator)mousoverGizmo, true);
            }

            CustomGizmoGridDrawer.Clean();
        }

        public static void DrawTooltip(Designator instance, bool detailed)
        {
            var k = instance.hotKey == null ? KeyCode.None : instance.hotKey.MainKey;
            var tipText = "";

            if (k != KeyCode.None && CustomGizmoGridDrawer.drawnHotKeys.ContainsKey(k) &&
                CustomGizmoGridDrawer.drawnHotKeys[k] != instance) k = KeyCode.None;

            if (detailed)
            {
                var hotkeyText = string.Format("Hotkey: {0}\n", k.ToString());
                tipText += string.Format("<b>{0}</b>\n{1}\n", instance.Label, hotkeyText);
            }

            tipText += instance.Desc;

            var mousePos = Event.current.mousePosition;
            var size = new Vector2(999f, 999f);
            var rect = new Rect(mousePos, size);
            var tip = new TipSignal(tipText, 24637);
            TooltipHandler.TipRegion(rect, tip);
        }
    }
}