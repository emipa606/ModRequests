using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;
using System.Linq;
using Verse.Sound;
using RimWorld;

namespace LobotomyCorp.Abnos
{
    public class ITab_ContainedAbnos : ITab
    {
        private static readonly Vector2 WinSize = new Vector2(350f, 480f);
        private static Vector2 descScrollPos;
        public ITab_ContainedAbnos()
        {
            this.size = ITab_ContainedAbnos.WinSize;
            this.labelKey = "Contained";
            descScrollPos = Vector2.zero;
        }

        public override void OnOpen()
        {
            base.OnOpen();
        }

        protected override void FillTab()
        {
            Rect rect = new Rect(7f, 5f, WinSize.x - 14f, 20f);

            Widgets.Label(rect, "Abnormality");
            Widgets.DrawLineHorizontal(rect.x, rect.y + 20f, rect.width);
            rect.y += rect.height;
            
            Thing abnos = (SelThing as ThingWithComps).GetComp<CompAbnosCore>().abnos;

            if (abnos == null) return;
            rect.y += 10f;
            Widgets.Label(rect, abnos.Label);
            rect.y += 20f;

            Rect image = rect;
            image.height = image.width = 120f;
            Widgets.DrawBox(image);
            image.x += 5f;
            image.y += 5f;
            image.height = image.width = 110f;
            Widgets.DrawTextureFitted(image, abnos.def.uiIcon,abnos.def.uiIconScale);

            CompProperties_AbnosProperties comp = GetComp(abnos);
            Rect desc = image;
            desc.xMin = image.xMax + 10f;
            desc.xMax = WinSize.x - 14f;
            Widgets.Label(desc, comp.danger.ToString());
            desc.y += 20f;
            Widgets.Label(desc, "P: "+comp.pbox);
            desc.xMin += desc.width / 2;
            Widgets.Label(desc, "Q:" + comp.qcounter);
            desc.xMin -= desc.width;
            desc.y += 20f;
            Widgets.Label(desc,comp.virtues.DebugText);

            rect.y += image.height + 15f;
            rect.height = 120f;
            Tool.WindowTool.TextScrollView(rect, ref descScrollPos, abnos.DescriptionDetailed);
            
        }

        private CompProperties_AbnosProperties GetComp(Thing t)
        {
            return t.def.GetCompProperties<CompProperties_AbnosProperties>();
        }
    }

}
