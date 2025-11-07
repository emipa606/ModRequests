using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Verse;

namespace BillDoorsFramework
{
    public class ThingWithHeldGraphic : ThingWithComps
    {
        ModExtension_HeldGraphic graphicExt;

        ModExtension_HeldGraphic GraphicExt
        {
            get
            {
                if (graphicExt == null)
                {
                    graphicExt = def.GetModExtension<ModExtension_HeldGraphic>();
                }
                return graphicExt;
            }
        }
        string prepend = "";
        string postpend = "";

        List<ModExtension_Description> descExt = new List<ModExtension_Description>();

        List<ModExtension_Description> DescExt
        {
            get
            {
                if (descExt.NullOrEmpty())
                {
                    prepend = "";
                    postpend = "";
                    foreach (var e in def.modExtensions.Where(e => e is ModExtension_Description))
                    {
                        var ext = e as ModExtension_Description;
                        descExt.Add(ext);

                        if (ext.prepend != null) prepend += ext.prepend + "\n\n";
                        if (ext.postpend != null) postpend += "\n\n" + ext.postpend;
                    }
                }
                return descExt;
            }
        }

        Graphic heldGraphic;

        Graphic HeldGraphic
        {
            get
            {
                if (GraphicExt != null)
                {
                    heldGraphic = GraphicExt.heldGraphic.GraphicColoredFor(this);
                }
                return heldGraphic;
            }
        }

        public override Graphic Graphic
        {
            get
            {
                if (HeldGraphic != null && ParentHolder is Pawn_EquipmentTracker)
                {
                    return HeldGraphic;
                }
                return base.Graphic;
            }
        }

        public override string DescriptionFlavor
        {
            get
            {
                if (DescExt != null && DescExt.Any())
                {
                    return prepend + base.DescriptionFlavor + postpend;
                }
                return base.DescriptionFlavor;
            }
        }

        public override string DescriptionDetailed
        {
            get
            {
                if (DescExt != null && DescExt.Any())
                {
                    return prepend + base.DescriptionFlavor + postpend;
                }
                return base.DescriptionDetailed;
            }
        }
    }

    public class ModExtension_HeldGraphic : DefModExtension
    {
        public GraphicData heldGraphic;
    }

    public class ModExtension_Description : DefModExtension
    {
        [MustTranslate]
        public string prepend;

        [MustTranslate]
        public string postpend;
    }
}
