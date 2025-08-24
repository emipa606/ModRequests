using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace StuffableCore.Settings.Editor
{
    public class MainEditorModule : ISettings
    {

        private Rect rectL;
        private Rect rectC;
        private Rect rectR;

        private ISettings innerL;
        private ISettings innerC;
        private ISettings innerR;

        private int position = 400;

        public ISettings InnerL { get => innerL; set => innerL = value; }
        public ISettings InnerC { get => innerC; set => innerC = value; }
        public ISettings InnerR { get => innerR; set => innerR = value; }
        public int Position { get => position; set => position = value; }

        public MainEditorModule() { }

        public MainEditorModule(ISettings innerL) : this()
        {
            this.innerL = innerL;
        }

        public MainEditorModule(ISettings innerL, ISettings innerC) : this(innerL)
        {
            this.innerC = innerC;
        }

        public MainEditorModule(ISettings innerL, ISettings innerC, ISettings innerR) : this(innerL, innerC)
        {
            this.innerR = innerR;
        }

        public void GetSettings(Listing_Standard listing_Standard)
        {
            Rect rect = listing_Standard.GetRect(Position);
            rect.x = 0;
            rect.y = 0;
            float width = (rect.width / 3f);
            float widthAdj = width - 15;
            float height = rect.height - 15;

            Widgets.DrawMenuSection(rect);

            Listing_Standard inner = new Listing_Standard();
            Rect innerRect = rect;
            innerRect.x += 7.5f;
            innerRect.y += 7.5f;
            inner.Begin(innerRect);

            rectL = new Rect(rect.x, rect.y, widthAdj, height);
            rectC = new Rect(width, rect.y, widthAdj, height);
            rectR = new Rect(width * 2, rect.y, widthAdj, height);

            if (innerL != null)
                DoInner(innerL, inner, rectL);

            if (innerC != null)
                DoInner(innerC, inner, rectC);

            if (innerR != null)
                DoInner(innerR, inner, rectR);

            inner.End();
        }

        private static void DoInner(ISettings innerWindow, Listing_Standard inner, Rect rect)
        {
            Widgets.DrawMenuSection(rect);
            rect.x += 5;
            rect.y += 5;
            rect.width -= 10;
            inner.Begin(rect);
            innerWindow?.GetSettings(inner);
            inner.End();
        }

        public static MainEditorModule GetDefaultEditor(StuffableCategorySettings selectedSettings)
        {
            return new MainEditorModule()
            {
                InnerL = new BasicInfo(selectedSettings),
                InnerC = new BasicSelector("Stuffable Categorinator".Colorize(Color.green), new CategoryLister(selectedSettings, 10), selectedSettings),
                InnerR = new BasicSelector("Ingredient Selectinator".Colorize(Color.green), new IngredientLister(selectedSettings, 10), selectedSettings)
            };
        }
    }
}
