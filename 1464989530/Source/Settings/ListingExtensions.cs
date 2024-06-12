using UnityEngine;

using Verse;

namespace NightVision
{
    public static class ListingExtensions
    {
        public static Listing_Standard SubListing(this Listing_Standard listing, float height, float contract = 48f)
        {
            Rect subRect = listing.GetRect(height);
            Widgets.DrawMenuSection(subRect);
            subRect.xMin += contract / 2;
            subRect.xMax -= contract / 2;
            Listing_Standard result = new Listing_Standard();
            result.Begin(subRect);

            return result;

        }

        public static void ShortGapLine(this Listing_Standard listing, float gapHeight = 12f)
        {
            var rect = listing.GetRect(gapHeight);
            float x = rect.x + 30f;
            float y = rect.y + gapHeight / 2f;
            Color color = GUI.color;
            GUI.color = color * new Color(1f, 1f, 1f, 0.4f);
            Widgets.DrawLineHorizontal(x, y, listing.ColumnWidth - 60f);
            GUI.color = color;
        }
    }
}