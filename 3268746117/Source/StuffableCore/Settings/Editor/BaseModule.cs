using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace StuffableCore.Settings.Editor
{
    public abstract class BaseModule : ISettings
    {

        private static StuffableCategorySettings selected;

        public static StuffableCategorySettings Selected { get => selected; set => selected = value; }

        public BaseModule() { }

        public BaseModule(StuffableCategorySettings selected)
        {
            Selected = selected;
        }

        public virtual void GetSettings(Listing_Standard listingStandard)
        {

        }
    }
}
