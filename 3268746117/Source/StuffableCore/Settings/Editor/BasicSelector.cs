using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace StuffableCore.Settings.Editor
{
    public class BasicSelector : BaseModule
    {
        private string title;
        protected string searchText = "";
        private ICarousel innerWindow;

        protected int carouselIndex = 0;
        protected int carouselListSize = 10;

        public int CarouselIndexMax
        {
            get
            {
                return innerWindow.MaxFilterSize() / carouselListSize;
            }
        }

        protected ICarousel InnerWindow { get => innerWindow; set => innerWindow = value; }

        public BasicSelector(StuffableCategorySettings selected) : base(selected)
        {

        }

        public BasicSelector(ICarousel innerWindow, StuffableCategorySettings selected) : this(selected)
        {
            InnerWindow = innerWindow;
        }

        public BasicSelector(string title, ICarousel innerWindow, StuffableCategorySettings selected) : this(innerWindow, selected)
        {
            this.title = title;
        }

        public override void GetSettings(Listing_Standard listing_Standard)
        {
            listing_Standard.Label(title);
            listing_Standard.GapLine();
            searchText = listing_Standard.TextEntryLabeled("Search? ", searchText);
            if (!searchText.NullOrEmpty())
            {
                int oldIndex = carouselIndex;
                innerWindow.Search(searchText, out carouselIndex);
                if(oldIndex != carouselIndex)
                    innerWindow.ChangeIndex(carouselIndex);
            }
            listing_Standard.Gap();

            Rect rect = listing_Standard.GetRect(250);

            if (innerWindow != null)
                DoInner(innerWindow, new Listing_Standard(), new Rect(rect.x, rect.y, rect.width, rect.height));

            Rect selectorRect = listing_Standard.GetRect(30);
            DoInner(new Listing_Standard(), new Rect(selectorRect.x, selectorRect.y + 5, selectorRect.width, selectorRect.height));
        }

        private void DoInner(ICarousel innerWindow, Listing_Standard inner, Rect rect)
        {
            Widgets.DrawMenuSection(rect);
            Rect innerRect = rect;
            innerRect.x += 5;
            innerRect.y += 5;
            innerRect.width -= 10;
            inner.Begin(innerRect);
            innerWindow?.GetSettings(inner);
            inner.End();
        }

        private void DoInner(Listing_Standard inner, Rect rect)
        {
            Widgets.DrawMenuSection(rect);
            float width = rect.width / 3;
            DoLSection(inner, new Rect(rect.x, rect.y, width, rect.height));
            DoCSection(inner, new Rect(rect.x + width, rect.y, width, rect.height));
            DoRSection(inner, new Rect(rect.x + (width * 2), rect.y, width, rect.height));
        }

        private void DoLSection(Listing_Standard inner, Rect rect)
        {
            inner.Begin(rect);
            if (inner.ButtonText("◄"))
            {
                carouselIndex = Math.Max(--carouselIndex, 0);
                innerWindow.ChangeIndex(carouselIndex);
            }
            inner.End();
        }

        private void DoCSection(Listing_Standard inner, Rect rect)
        {
            Rect center = rect;
            center.x += 25;
            center.y += 5;
            inner.Begin(center);
            carouselIndex = Math.Min(carouselIndex, CarouselIndexMax);
            inner.Label("{0} / {1}".Formatted(carouselIndex, CarouselIndexMax));
            inner.End();
        }

        private void DoRSection(Listing_Standard inner, Rect rect)
        {
            inner.Begin(rect);
            if(inner.ButtonText("►"))
            {
                carouselIndex = Math.Min(++carouselIndex, CarouselIndexMax);
                innerWindow.ChangeIndex(carouselIndex);
            }
            inner.End();
        }
    }
}
